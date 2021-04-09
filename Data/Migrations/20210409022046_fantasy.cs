using Microsoft.EntityFrameworkCore.Migrations;

namespace API.Data.Migrations
{
    public partial class fantasy : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FantasiesFantasyId",
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
                name: "IX_AspNetUsers_FantasiesFantasyId",
                table: "AspNetUsers",
                column: "FantasiesFantasyId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Fantasy_FantasiesFantasyId",
                table: "AspNetUsers",
                column: "FantasiesFantasyId",
                principalTable: "Fantasy",
                principalColumn: "FantasyId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Fantasy_FantasiesFantasyId",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Fantasy");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_FantasiesFantasyId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "FantasiesFantasyId",
                table: "AspNetUsers");
        }
    }
}
