using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComputerSessionService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_Type_Sesion_Status : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Sessions",
                type: "nvarchar(64)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Sessions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(64)");
        }
    }
}
