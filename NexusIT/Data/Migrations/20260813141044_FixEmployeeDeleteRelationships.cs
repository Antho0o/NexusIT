using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexusIT.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixEmployeeDeleteRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SupportTickets_Assets_AssetId",
                table: "SupportTickets");

            migrationBuilder.DropForeignKey(
                name: "FK_SupportTickets_Employees_EmployeeId",
                table: "SupportTickets");

            migrationBuilder.AddForeignKey(
                name: "FK_SupportTickets_Assets_AssetId",
                table: "SupportTickets",
                column: "AssetId",
                principalTable: "Assets",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_SupportTickets_Employees_EmployeeId",
                table: "SupportTickets",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SupportTickets_Assets_AssetId",
                table: "SupportTickets");

            migrationBuilder.DropForeignKey(
                name: "FK_SupportTickets_Employees_EmployeeId",
                table: "SupportTickets");

            migrationBuilder.AddForeignKey(
                name: "FK_SupportTickets_Assets_AssetId",
                table: "SupportTickets",
                column: "AssetId",
                principalTable: "Assets",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SupportTickets_Employees_EmployeeId",
                table: "SupportTickets",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id");
        }
    }
}
