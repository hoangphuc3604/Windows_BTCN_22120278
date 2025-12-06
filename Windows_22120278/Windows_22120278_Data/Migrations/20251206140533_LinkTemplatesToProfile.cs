using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Windows_22120278_Data.Migrations
{
    /// <inheritdoc />
    public partial class LinkTemplatesToProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProfileId",
                table: "ShapeTemplates",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ShapeTemplates_ProfileId",
                table: "ShapeTemplates",
                column: "ProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_ShapeTemplates_Profiles_ProfileId",
                table: "ShapeTemplates",
                column: "ProfileId",
                principalTable: "Profiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShapeTemplates_Profiles_ProfileId",
                table: "ShapeTemplates");

            migrationBuilder.DropIndex(
                name: "IX_ShapeTemplates_ProfileId",
                table: "ShapeTemplates");

            migrationBuilder.DropColumn(
                name: "ProfileId",
                table: "ShapeTemplates");
        }
    }
}
