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
Install-Putty

# Function to clear the contents of a folder
function Clear-FolderContents {
    param (
        [string]$FolderPath
    )

    # Check if the folder exists
    if (Test-Path $FolderPath) {
        try {
            # Remove all files and subdirectories within the folder
            Get-ChildItem -Path $FolderPath -Recurse | Remove-Item -Force -Recurse
            Write-Host "The contents of the folder '$FolderPath' have been removed." -ForegroundColor Green
        }
        catch {
            Write-Host "An error occurred: $_" -ForegroundColor Red
        }
    }
    else {
        Write-Host "The folder '$FolderPath' does not exist." -ForegroundColor Red
    }
}

# Function to prompt user to select an option from a list
function Get-InputFromList {
    param (
        [string]$Prompt,
        [string[]]$Options,
        [string]$DefaultValue
    )

    # Check if the default value is in the list of options
    if (-not $Options -contains $DefaultValue) {
        Write-Host "Default value '$DefaultValue' is not in the list of options." -ForegroundColor Red
        return
    }

    $selectedValue = ""
    while (-not $Options -contains $selectedValue) {
        Write-Host "$Prompt (default: $DefaultValue)"
        for ($i = 0; $i -lt $Options.Length; $i++) {
            Write-Host "$($i + 1). $($Options[$i])"
        }

        $userInput = Read-Host "Select option or press Enter to use default ($DefaultValue)"

        # If the user presses Enter without input, use the default value
        if ([string]::IsNullOrWhiteSpace($userInput)) {
            $selectedValue = $DefaultValue
        }
        # If the user input is a number, check if it corresponds to an option
        elseif ($userInput -match '^\d+$') {
            $index = [int]$userInput - 1
            if ($index -ge 0 -and $index -lt $Options.Length) {
                $selectedValue = $Options[$index]
            }
        }

        # If the selected value is not valid, prompt again
        if (-not $Options -contains $selectedValue) {
            Write-Host "Invalid option. Please try again." -ForegroundColor Red
        }
    }

    return $selectedValue.Trim()
}

function Update-TextInFile {
    param (
        [string]$FilePath,
        [string]$OldText,
        [string]$NewText
    )

    if (-Not (Test-Path -Path $FilePath)) {
        Write-Host "File '$FilePath' does not exist." -ForegroundColor Red
        return
    }

    # Reead the content of the file
    $fileContent = Get-Content -Path $FilePath -Raw

    # Replace the old text with the new text
    $updatedContent = $fileContent -replace $OldText, $NewText

    # Save the updated content to the file
    $updatedContent | Out-File -FilePath $FilePath -Encoding UTF8
}


# Prompt for SSH username, password, and server IP
function Get-InputWithDefault {
    param (
        [string]$Prompt,
        [string]$DefaultValue
    )
    
    $inputValue = Read-Host "$Prompt (default: $DefaultValue)"
    
    if ([string]::IsNullOrWhiteSpace($inputValue)) {
        return $DefaultValue
    }
    else {
        return $inputValue
    }
}

# Read user input
$ssh_user = Get-InputWithDefault -Prompt "Enter the SSH username" -DefaultValue "root"
$ssh_password = Get-InputWithDefault -Prompt "Enter the SSH password" -DefaultValue "Password@1000"
$server_ip = Get-InputWithDefault -Prompt "Enter the server IP" -DefaultValue "172.16.90.96"
$server_name = Get-InputWithDefault -Prompt "Enter the WEB server name/domain" -DefaultValue "appsrv"
$api_scheme = Get-InputFromList -Prompt "Enter the API server schema (http or https)" -Options @("http", "https") -DefaultValue "http"
$api_port = "5217"
if ($api_scheme -eq "https") {
    $api_port = "7253"
}
$api_host = Get-InputWithDefault -Prompt "Enter the API server name/domain" -DefaultValue "${server_name}:${api_port}"
$secured_api = Get-InputFromList -Prompt "Is the API secured (true or false)" -Options @("true", "false") -DefaultValue "false"

Write-Host "SSH Username: $ssh_user"
Write-Host "SSH Password: $ssh_password"
Write-Host "Server IP: $server_ip"
Write-Host "WEB Server Name/Domain: $server_name"
Write-Host "API Server Name/Domain: $api_host"
Write-Host "API Server Schema: $api_scheme"
Write-Host "API Server Secured: $secured_api"

# Check if the user wants to continue
$response = Get-InputFromList -Prompt "Continue? (yes or no)" -Options @("yes", "no") -DefaultValue "yes"
if ($response -eq "no") {
    Write-Host "Exiting..."
    exit
}

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
$secured = '"IsSecure": true'
$unsecured = '"IsSecure": false'
if ($secured_api -eq "true") {
    $result = $secured
}
else {
    $result = $unsecured
}
Update-TextInFile -FilePath "$local_publish_dir/api/appsettings.json" -OldText '"IsSecure": false' -NewText "$result"
Update-TextInFile -FilePath "$local_publish_dir/web/wwwroot/appsettings.json" -OldText "appsrv:5217" -NewText $api_host
Update-TextInFile -FilePath "$local_publish_dir/web/wwwroot/appsettings.json" -OldText "http" -NewText "$api_scheme"
# Create a zip file containing the published files
Get-ChildItem -Path $local_publish_dir -Filter *.sh | Remove-Item -Force
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


$web_nginx = "server {
    listen 80;
    server_name $server_name;
    root $web_dir/wwwroot;
    index index.html index.htm index.nginx-debian.html;
    
    access_log /var/log/nginx/ajk_web.log;
    error_log /var/log/nginx/ajk_web_error.log;

    location / {    
        try_files `$uri `$uri/ /index.html =404;
    }
}
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
Environment=MAIL_PASSWORD=YourMailPassword
Environment=DOTNET_CLI_HOME=$web_dir/.dotnet
Environment=DOTNET_NOLOGO=true
Environment=DOTNET_CLI_TELEMETRY_OPTOUT=1
Environment=DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1

[Install]
WantedBy=multi-user.target
"
$LOG_FILE = $remote_log_path
# Define the deployment script content
$deploy_script = @"
if ! dpkg -l | grep -qw unzip; then
    sudo apt-get update && sudo apt-get install -y unzip
fi
# Unzip the deployment package
echo 'Unzipping deployment package...' | tee -a $LOG_FILE
unzip -o $remote_zip_path -d $remote_home_dir | tee -a $LOG_FILE
# Create directories with proper ownership
echo 'Creating directories...' | tee -a $LOG_FILE
sudo mkdir -p $web_dir | tee -a $LOG_FILE
sudo mkdir -p $web_dir/.dotnet | tee -a $LOG_FILE
sudo mkdir -p $api_dir | tee -a $LOG_FILE
sudo mkdir -p $api_dir/.dotnet | tee -a $LOG_FILE
# Copy the published files to the target directories
echo 'Copying published files...' | tee -a $LOG_FILE
sudo cp -r $remote_home_dir/api/* $api_dir/ | tee -a $LOG_FILE
sudo cp -r $remote_home_dir/web/* $web_dir/ | tee -a $LOG_FILE
sudo chown -R www-data:www-data /var/www/dotnetapps | tee -a $LOG_FILE
sudo chmod -R 755 /var/www/dotnetapps | tee -a $LOG_FILE
# Create Docker container for PostgreSQL with external volume for data
echo 'Creating Docker container for PostgreSQL...' | tee -a $LOG_FILE
sudo docker run --name ajk_postgres -e POSTGRES_USER=pguser -e POSTGRES_PASSWORD=pguser@99 -p 5432:5432 -v ${postgres_data_dir} -d --restart unless-stopped postgres | tee -a $LOG_FILE
# Create nginx directives for web application
echo 'Creating nginx directives...' | tee -a $LOG_FILE
echo '$web_nginx' | sudo tee /etc/nginx/sites-available/$server_name | tee -a $LOG_FILE
sudo ln -s /etc/nginx/sites-available/$server_name /etc/nginx/sites-enabled/$server_name | tee -a $LOG_FILE
sudo nginx -t && sudo systemctl restart nginx | tee -a $LOG_FILE
# Create systemd service files for both applications
sudo systemctl stop ajkiot_api.service | tee -a $LOG_FILE
echo "$api_service" | sudo tee /etc/systemd/system/ajkiot_api.service | tee -a $LOG_FILE
# Reload systemd, enable and start the services
echo 'Reloading systemd and starting services...' | tee -a $LOG_FILE
sudo systemctl daemon-reload | tee -a $LOG_FILE
sudo systemctl enable ajkiot_api.service | tee -a $LOG_FILE
sudo systemctl start ajkiot_api.service | tee -a $LOG_FILE
# Clean up
echo 'Cleaning up...' | tee -a $LOG_FILE
rm $remote_zip_path | tee -a $LOG_FILE
# rm $remote_script_path | tee -a $LOG_FILE
sudo rm $remote_home_dir/api -R | tee -a $LOG_FILE
sudo rm $remote_home_dir/web -R | tee -a $LOG_FILE
echo 'Deployment completed.' | tee -a $LOG_FILE
echo 'Deployment completed.'
"@

$deploy_script = $deploy_script -replace "`r`n", "`n"
# Write the deployment script to a file
Set-Content -Path $script_path -Value $deploy_script

# Copy the deployment script to the server
Copy-FilesToServer $script_path $remote_script_path

# Make the script executable and run it
Invoke-SSHCommand "chmod +x $remote_script_path"

# Call the function to clear the contents of the folder 
Clear-FolderContents -FolderPath "$local_publish_dir/api/"
Clear-FolderContents -FolderPath "$local_publish_dir/web/"


