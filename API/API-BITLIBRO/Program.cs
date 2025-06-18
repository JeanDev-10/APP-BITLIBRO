using System.Text;
using API_BITLIBRO.Context;
using API_BITLIBRO.Data;
using API_BITLIBRO.Models;
using API_BITLIBRO.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseWebRoot("wwwroot");




// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
//añadimos a swagger la opción de token
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please insert JWT with Bearer into field",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new string[] {}
        }
    });
});

//conexión a bd
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//identity
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
})
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders(); 


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; //Cuando llega una request, el middleware usará JWT para determinar la identidad del usuario
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme; //En APIs, generalmente devuelve un 401/403 en lugar de redireccionar a login
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme; //Sirve como fallback cuando no se especifica otro esquema
}).AddJwtBearer(options =>
{
    options.SaveToken = true; //Útil si necesitas acceder al token más adelante
    options.RequireHttpsMetadata = false; // Cambiar a true en producción
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, //tokens proporcionados por la api valida
        ValidateAudience = true, //tokens emitido el frontend valida
        ValidateLifetime = true, //valida que no haya expirado
        ValidateIssuerSigningKey = true, //valida la firma del token
        ClockSkew = TimeSpan.Zero, // El tiempo de tolerancia para la expiración del token, se establece en cero para evitar problemas de sincronización de reloj
        ValidIssuer = builder.Configuration["JWTSettings:Issuer"], //configurado en appsettings.json de la api en local
        ValidAudience = builder.Configuration["JWTSettings:Audience"], //configurado en appsettings.json del frontend en local
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWTSettings:SecretKey"]!)) //aqui ubicamos la clave secreta para firmar el token
    };
});

builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<GenreService>();
builder.Services.AddScoped<EmployeeService>();
builder.Services.AddScoped<ImageService>();
builder.Services.AddScoped<BookService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
// Configuración para servir archivos estáticos
app.UseStaticFiles();

//permitir peticiones desde origen
var origens= builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? new string[] { "http://localhost:4200" };
app.UseCors(options =>
{
    options.WithOrigins(origens)
        .AllowAnyMethod()
        .AllowAnyHeader();
});

//aplicar jwt
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();



//seed de datos iniciales
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await DataSeeder.Initialize(services);
}

app.Run();
