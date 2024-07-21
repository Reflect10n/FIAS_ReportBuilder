using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication6.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ObjectAsAddrs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Typename = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    OperType_Id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ObjectAsAddrs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ObjectLevels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Level = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ObjectLevels", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "ObjectLevels",
                columns: new[] { "Id", "IsActive", "Level", "Name" },
                values: new object[] { 1, true, 1, "Субъект РФ" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ObjectAsAddrs");

            migrationBuilder.DropTable(
                name: "ObjectLevels");
        }
    }
}
