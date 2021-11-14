using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auth.Data.ConfigurationMigrations
{
    public partial class UpdateToDotNet6Final : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CibaLifetime",
                table: "Clients",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PollingInterval",
                table: "Clients",
                type: "int",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CibaLifetime",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "PollingInterval",
                table: "Clients");
        }
    }
}
