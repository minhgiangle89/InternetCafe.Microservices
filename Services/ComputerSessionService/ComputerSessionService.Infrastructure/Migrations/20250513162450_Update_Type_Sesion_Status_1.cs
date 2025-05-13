using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComputerSessionService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_Type_Sesion_Status_1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Sessions",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(64)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Sessions",
                type: "nvarchar(64)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
