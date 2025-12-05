using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Windows_22120278_Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDrawingBoardAndShapes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DrawingBoards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Width = table.Column<double>(type: "REAL", nullable: false),
                    Height = table.Column<double>(type: "REAL", nullable: false),
                    BackgroundColor = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ProfileId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrawingBoards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DrawingBoards_Profiles_ProfileId",
                        column: x => x.ProfileId,
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Shapes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DrawingBoardId = table.Column<int>(type: "INTEGER", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Color = table.Column<string>(type: "TEXT", nullable: true),
                    StrokeThickness = table.Column<double>(type: "REAL", nullable: false),
                    StartX = table.Column<double>(type: "REAL", nullable: false),
                    StartY = table.Column<double>(type: "REAL", nullable: false),
                    EndX = table.Column<double>(type: "REAL", nullable: false),
                    EndY = table.Column<double>(type: "REAL", nullable: false),
                    PointsData = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shapes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Shapes_DrawingBoards_DrawingBoardId",
                        column: x => x.DrawingBoardId,
                        principalTable: "DrawingBoards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DrawingBoards_ProfileId",
                table: "DrawingBoards",
                column: "ProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Shapes_DrawingBoardId",
                table: "Shapes",
                column: "DrawingBoardId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Shapes");

            migrationBuilder.DropTable(
                name: "DrawingBoards");
        }
    }
}
