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

            CreateMap<LibroCreacionDTO, AutorLibro>()
                .ForMember(ent => ent.Libro, src => src.MapFrom(dto => new Libro { Titulo = dto.Titulo }))
                .ReverseMap();

            CreateMap<AutorLibro, LibroDTO>()
                .ForMember(dto => dto.Id, config => config.MapFrom(ent => ent.LibroId))
                .ForMember(dto => dto.Titulo, config => config.MapFrom(ent => ent.Libro!.Titulo))
                .ReverseMap();

            CreateMap<Libro, LibroConAutoresDTO>().ReverseMap();

            CreateMap<AutorLibro, AutorDTO>()
                .ForMember(dto => dto.Id, config => config.MapFrom(ent => ent.AutorId))
                .ForMember(dto => dto.NombreCompleto, config => config.MapFrom(ent => ObtenerNombreCompletoAutor(ent.Autor!)))
                .ReverseMap();

            CreateMap<Libro, LibroDTO>().ReverseMap();
            CreateMap<LibroCreacionDTO, Libro>()
                .ForMember(ent => ent.Autores, src => src.MapFrom(dto => dto.AutoresIds.Select(id => new AutorLibro { AutorId = id })))
                .ReverseMap();

            CreateMap<Comentario, ComentarioCrearDTO>().ReverseMap();

            CreateMap<Comentario, ComentarioDTO>()
                .ForMember(dto => dto.UsuarioEmail, config => config.MapFrom(ent => ent.Usuario!.Email))
                .ReverseMap();

            CreateMap<Comentario, ComentarioPatchDTO>().ReverseMap();
        }

        private string ObtenerNombreCompletoAutor(Autor autor)
        {
            return $"{autor.Nombres} {autor.Apellido}";
        }
    }
}
