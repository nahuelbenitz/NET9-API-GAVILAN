using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NET9.API.Migrations
{
    /// <inheritdoc />
    public partial class agregoRelacionUsuarioComentario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UsuarioID",
                table: "Comentarios",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Comentarios_UsuarioID",
                table: "Comentarios",
                column: "UsuarioID");

            migrationBuilder.AddForeignKey(
                name: "FK_Comentarios_AspNetUsers_UsuarioID",
                table: "Comentarios",
                column: "UsuarioID",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comentarios_AspNetUsers_UsuarioID",
                table: "Comentarios");

            migrationBuilder.DropIndex(
                name: "IX_Comentarios_UsuarioID",
                table: "Comentarios");

            migrationBuilder.DropColumn(
                name: "UsuarioID",
                table: "Comentarios");
        }
    }
}
