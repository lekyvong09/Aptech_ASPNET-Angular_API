using Microsoft.EntityFrameworkCore.Migrations;

namespace API.Data.Migrations
{
    public partial class addFatasyEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MyFantasy",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Fantasy",
                columns: table => new
                {
                    FantasyId = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fantasy", x => x.FantasyId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_MyFantasy",
                table: "AspNetUsers",
                column: "MyFantasy");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Fantasy_MyFantasy",
                table: "AspNetUsers",
                column: "MyFantasy",
                principalTable: "Fantasy",
                principalColumn: "FantasyId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Fantasy_MyFantasy",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Fantasy");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_MyFantasy",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "MyFantasy",
                table: "AspNetUsers");
        }
    }
}
