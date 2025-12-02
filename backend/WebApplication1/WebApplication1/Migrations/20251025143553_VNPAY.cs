using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Migrations
{
    public partial class VNPAY : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductDeletedByAdmin",
                table: "ProductDeletedByAdmin");

            migrationBuilder.RenameTable(
                name: "ProductDeletedByAdmin",
                newName: "ProductDeletedByAdmins");

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "CartItems",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductDeletedByAdmins",
                table: "ProductDeletedByAdmins",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "VnPayCardTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Token = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CardNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    BankCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VnPayCardTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VnPayCardTokens_Users_Username",
                        column: x => x.Username,
                        principalTable: "Users",
                        principalColumn: "Username",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VnPayCardTokens_Username",
                table: "VnPayCardTokens",
                column: "Username");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VnPayCardTokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductDeletedByAdmins",
                table: "ProductDeletedByAdmins");

            migrationBuilder.RenameTable(
                name: "ProductDeletedByAdmins",
                newName: "ProductDeletedByAdmin");

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "CartItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductDeletedByAdmin",
                table: "ProductDeletedByAdmin",
                column: "Id");
        }
    }
}
