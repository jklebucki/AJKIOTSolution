# Ensure PuTTY is installed
function Install-Putty {
    $puttyUrl = "https://the.earth.li/~sgtatham/putty/latest/w64/putty.zip"
    $puttyPath = "C:\Program Files\PuTTY"

    if (-not (Test-Path $puttyPath)) {
        Write-Host "PuTTY not found. Installing..."
        Invoke-WebRequest -Uri $puttyUrl -OutFile "$env:TEMP\putty.zip"
        Expand-Archive -Path "$env:TEMP\putty.zip" -DestinationPath $puttyPath
        [System.Environment]::SetEnvironmentVariable("PATH", $env:PATH + ";$puttyPath", [System.EnvironmentVariableTarget]::Machine)
    }
}

# Ensure PuTTY is installed
Install-Putty

# Prompt for SSH username, password, and server IP
$ssh_user = Read-Host "Enter the SSH username"
$ssh_password = Read-Host "Enter the SSH password"
$server_ip = Read-Host "Enter the server IP"

# Define directories and paths
$local_publish_dir = "./Release"
$local_zip_path = "./Release/deploy_package.zip"
$remote_home_dir = "/home/$ssh_user"
$remote_zip_path = "$remote_home_dir/deploy_package.zip"
$remote_script_path = "$remote_home_dir/deploy_script.sh"
$remote_log_path = "'$remote_home_dir/deploy.log'"
$api_dir = '/var/www/dotnetapps/ajk_iot_api'
$web_dir = '/var/www/dotnetapps/ajk_iot_web'
$postgres_data_dir = '/var/lib/postgresql/data'
$script_path = "./Release/deploy_script.sh"

# Build the .NET applications
dotnet publish -c Release -o "$local_publish_dir/api" "./src/AJKIOT.Api/AJKIOT.Api.csproj"
dotnet publish -c Release -o "$local_publish_dir/web" "./src/AJKIOT.Web/AJKIOT.Web.csproj" --nologo --verbosity minimal

# Create a zip file containing the published files
if (Test-Path $local_zip_path) { Remove-Item $local_zip_path }
Compress-Archive -Path "$local_publish_dir/*" -DestinationPath $local_zip_path

# Function to execute SSH commands with plink
function Invoke-SSHCommand {
    param (
        [string]$Command
    )
    & "C:\Program Files\PuTTY\plink.exe" -ssh -l $ssh_user -pw $ssh_password $server_ip $Command
}

# Function to copy files to the server using pscp
function Copy-FilesToServer {
    param (
        [string]$local_path,
        [string]$remote_path
    )
    & "C:\Program Files\PuTTY\pscp.exe" -scp -pw $ssh_password "$local_path" "$ssh_user@${server_ip}:$remote_path"
}

# Check if connection to the server is successful
try {
    Invoke-SSHCommand "exit"
    Write-Host "Connection to the server successful."
}
catch {
    Write-Host "Failed to connect to the server. Please check your credentials and server IP."
    exit
}

# Copy the zip file and the deployment script to the server
Copy-FilesToServer $local_zip_path $remote_zip_path


$web_service = "[Unit]
Description=AJKIOT Web

[Service]
WorkingDirectory=$web_dir
ExecStart=/usr/bin/dotnet $web_dir/AJKIOT.Web.dll
Restart=always
RestartSec=10
SyslogIdentifier=dotnet-web
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_CLI_HOME=$web_dir/.dotnet
Environment=DOTNET_NOLOGO=true

[Install]
WantedBy=multi-user.target
"

$api_service = "[Unit]
Description=AJKIOT API

[Service]
WorkingDirectory=$api_dir
ExecStart=/usr/bin/dotnet $api_dir/AJKIOT.Api.dll
Restart=always
RestartSec=10
SyslogIdentifier=dotnet-api
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_CLI_HOME=$web_dir/.dotnet
Environment=DOTNET_NOLOGO=true

[Install]
WantedBy=multi-user.target
"
$LOG_FILE = $remote_log_path
# Define the deployment script content
$deploy_script = @"
sudo apt-get install unzip
# Unzip the deployment package
echo 'Unzipping deployment package...' | tee -a $LOG_FILE
unzip -o $remote_zip_path -d $remote_home_dir | tee -a $LOG_FILE
# Create directories with proper ownership
echo 'Creating directories...' | tee -a $LOG_FILE
sudo mkdir -p $web_dir | tee -a $LOG_FILE
sudo mkdir -p $api_dir | tee -a $LOG_FILE
# Copy the published files to the target directories
echo 'Copying published files...' | tee -a $LOG_FILE
sudo cp -r $remote_home_dir/api/* $api_dir/ | tee -a $LOG_FILE
sudo cp -r $remote_home_dir/web/* $web_dir/ | tee -a $LOG_FILE
sudo chown -R www-data:www-data /var/www/dotnetapps | tee -a $LOG_FILE
sudo chmod -R 755 /var/www/dotnetapps | tee -a $LOG_FILE
# Create Docker container for PostgreSQL with external volume for data
echo 'Creating Docker container for PostgreSQL...' | tee -a $LOG_FILE
sudo docker run --name ajk_postgres -e POSTGRES_USER=pguser -e POSTGRES_PASSWORD=pguser@99 -p 5432:5432 -v ${postgres_data_dir} -d --restart unless-stopped postgres | tee -a $LOG_FILE
# Create systemd service files for both applications
echo "$api_service" | sudo tee /etc/systemd/system/ajkiot_api.service | tee -a $LOG_FILE
echo "$web_service" | sudo tee /etc/systemd/system/ajkiot_web.service | tee -a $LOG_FILE
# Reload systemd, enable and start the services
echo 'Reloading systemd and starting services...' | tee -a $LOG_FILE
sudo systemctl daemon-reload | tee -a $LOG_FILE
sudo systemctl enable ajkiot_api.service | tee -a $LOG_FILE
sudo systemctl enable ajkiot_web.service | tee -a $LOG_FILE
sudo systemctl start ajkiot_api.service | tee -a $LOG_FILE
sudo systemctl start ajkiot_web.service | tee -a $LOG_FILE
# Clean up
echo 'Cleaning up...' | tee -a $LOG_FILE
rm $remote_zip_path | tee -a $LOG_FILE
# rm $remote_script_path | tee -a $LOG_FILE
sudo rm $remote_home_dir/api -R | tee -a $LOG_FILE
sudo rm $remote_home_dir/web -R | tee -a $LOG_FILE
echo 'Deployment completed.' | tee -a $LOG_FILE
"@

# Write the deployment script to a file
Set-Content -Path $script_path -Value $deploy_script

# Copy the deployment script to the server
Copy-FilesToServer $script_path $remote_script_path

# Make the script executable and run it
Invoke-SSHCommand "chmod +x $remote_script_path"

