using AutoMapper;
using NET9.API.Models;
using NET9.API.Models.DTOs;

namespace NET9.API.Mapping
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Autor, AutorDTO>()
                    .ForMember(dest => dest.NombreCompleto, src => src.MapFrom(autor => ObtenerNombreCompletoAutor(autor)))
                    .ReverseMap();

            CreateMap<Autor, AutorConLibrosDTO>()
                    .ForMember(dest => dest.NombreCompleto, src => src.MapFrom(autor => ObtenerNombreCompletoAutor(autor)))
                    .ReverseMap();

            CreateMap<AutorCrearDTO, Autor>().ReverseMap();

            CreateMap<AutorPatchDTO, Autor>().ReverseMap();

            CreateMap<Libro, LibroDTO>().ReverseMap();

            CreateMap<Libro, LibroConAutorDTO>()
                .ForMember(dest => dest.AutorNombre, src => src.MapFrom(libro => ObtenerNombreCompletoAutor(libro.Autor!)))
                     .ReverseMap();

            CreateMap<LibroCreacionDTO, Libro>().ReverseMap();
        }

        private string ObtenerNombreCompletoAutor(Autor autor)
        {
            return $"{autor.Nombres} {autor.Apellido}";
        }
    }
}
