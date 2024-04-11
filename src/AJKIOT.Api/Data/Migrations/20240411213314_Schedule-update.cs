using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AJKIOT.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class Scheduleupdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DeviceSheduleJson",
                table: "IotDevices",
                newName: "DeviceScheduleJson");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DeviceScheduleJson",
                table: "IotDevices",
                newName: "DeviceSheduleJson");
        }
    }
}
