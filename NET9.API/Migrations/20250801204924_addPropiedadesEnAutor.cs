using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NET9.API.Migrations
{
    /// <inheritdoc />
    public partial class addPropiedadesEnAutor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Nombre",
                table: "Autores",
                newName: "Nombres");

            migrationBuilder.AddColumn<string>(
                name: "Apellido",
                table: "Autores",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Identificacion",
                table: "Autores",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Apellido",
                table: "Autores");

            migrationBuilder.DropColumn(
                name: "Identificacion",
                table: "Autores");

            migrationBuilder.RenameColumn(
                name: "Nombres",
                table: "Autores",
                newName: "Nombre");
        }
    }
}
