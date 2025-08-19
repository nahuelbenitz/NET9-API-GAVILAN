﻿namespace NET9.API.Models.DTOs
{
    public class AutorFiltroDTO
    {
        public int Pagina { get; set; }
        public int RecordsPorPagina { get; set; }
        public PaginacionDTO PaginacionDTO
        {
            get
            {
                return new PaginacionDTO(Pagina, RecordsPorPagina);
            }
        }
        public string? Nombres { get; set; }
        public string? Apellidos { get; set; }
        public bool? TieneFoto { get; set; }
        public bool? TieneLibros { get; set; }
        public string? TituloLibro { get; set; }
        public bool IncluirLibros { get; set; }
        public string? CampoOrdenar { get; set; }
        public bool OrdenAscendente { get; set; }
    }
}
