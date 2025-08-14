namespace NET9.API.Services.Interfaces
{
    public interface IAlmacenadorArchivo
    {
        Task Borrar(string? ruta, string contenedor);
        Task<string> Guardar(string contenedor, IFormFile archivo);

        async Task<string> Editar(string contenedor, IFormFile archivo, string ruta)
        {
            await Borrar(ruta, contenedor);
            return await Guardar(contenedor, archivo);

        }
    }
}
