using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkTimeTracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Pracownicy",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Imie = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Nazwisko = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Stanowisko = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    StawkaGodzinowa = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pracownicy", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Rejestry",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PracownikId = table.Column<int>(type: "INTEGER", nullable: false),
                    Data = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LiczbaGodzin = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: false),
                    CzyUrlop = table.Column<bool>(type: "INTEGER", nullable: false),
                    CzyNadgodziny = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rejestry", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Rejestry_Pracownicy_PracownikId",
                        column: x => x.PracownikId,
                        principalTable: "Pracownicy",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Rejestry_PracownikId",
                table: "Rejestry",
                column: "PracownikId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Rejestry");

            migrationBuilder.DropTable(
                name: "Pracownicy");
        }
    }
}
