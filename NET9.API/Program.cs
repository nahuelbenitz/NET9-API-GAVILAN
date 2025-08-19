using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NET9.API.Data;
using NET9.API.Models;
using NET9.API.Services;
using NET9.API.Services.Interfaces;
using NET9.API.Swagger;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddControllers().AddNewtonsoftJson();

//    AddJsonOptions(options =>
//{
//    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
//});

//COMENTADO PORQUE AHORA USAMOS REDIS
//builder.Services.AddOutputCache(options =>
//{
//    options.DefaultExpirationTimeSpan = TimeSpan.FromSeconds(15);
//});

builder.Services.AddStackExchangeRedisOutputCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("redis");
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

//Configuro Identity
builder.Services.AddIdentityCore<Usuario>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

//Configuro el UserManager
builder.Services.AddScoped<UserManager<Usuario>>();

//Manejo de usuarios, registrar usuarios
builder.Services.AddScoped<SignInManager<Usuario>>();

builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IAlmacenadorArchivo, AlmacenadorLocalService>();

//Nos permite inyectar el contexto de la aplicación en los controladores
builder.Services.AddHttpContextAccessor();

//Configuro autenticación
builder.Services.AddAuthentication().AddJwtBearer(options =>
{
    options.MapInboundClaims = false; // Evita que se cambies las claims
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("esadmin", policy => policy.RequireClaim("esadmin"));
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()
        .WithExposedHeaders("cantidad-total-registros");
    });
});

builder.Services.AddDataProtection();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "NET9 API",
        Version = "v1",
        Description = "API para la gestión de libros, autores y comentarios"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    });

    options.OperationFilter<FiltroAutorizacion>();

    //options.AddSecurityRequirement(new OpenApiSecurityRequirement()
    //{
    //  {
    //    new OpenApiSecurityScheme
    //    {
    //      Reference = new OpenApiReference
    //      {
    //        Type = ReferenceType.SecurityScheme,
    //        Id = "Bearer"
    //      },
    //      Scheme = "oauth2",
    //      Name = "Bearer",
    //      In = ParameterLocation.Header
    //    },
    //    new List<string>()
    //  }
    //});

});



builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseStaticFiles();

app.UseCors();

app.UseOutputCache();

app.MapControllers();

app.Run();
