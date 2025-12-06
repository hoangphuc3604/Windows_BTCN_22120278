using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Windows_22120278_Data.Migrations
{
    /// <inheritdoc />
    public partial class AddShapeTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ShapeTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
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
                    table.PrimaryKey("PK_ShapeTemplates", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShapeTemplates");
        }
    }
}

