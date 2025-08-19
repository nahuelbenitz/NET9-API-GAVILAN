﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NET9.API.Models;

namespace NET9.API.Data
{
    public class ApplicationDbContext : IdentityDbContext<Usuario>
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Comentario>().HasQueryFilter(x => !x.EstaBorrado);
        }

        public DbSet<Autor> Autores { get; set; }
        public DbSet<Libro> Libros { get; set; }
        public DbSet<Comentario> Comentarios { get; set; }
        public DbSet<AutorLibro> AutoresLibros { get; set; }
    }
}
