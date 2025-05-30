using condominio_API.Data;
using condominio_API.Services;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using condominio_API.Models;

var builder = WebApplication.CreateBuilder(args);

// Configurar o DbContext com MySQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    ));

// Registrar configurações de e-mail
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// Registrar o serviço de e-mail
builder.Services.AddSingleton<IEmailService, EmailService>();

// Adicionar serviços
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ✅ AUTENTICAÇÃO COM JWT EM COOKIE
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = "condominio_backend.com",
        ValidAudience = "condominio_backend.com",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("PvvCX14bFPJKfA6dZib1DitiRnuhgS7uoAZw3AgIYS4="))
    };

    // 🔐 Capturar o JWT do cookie "jwt"
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var token = context.HttpContext.Request.Cookies["jwt"];
            if (!string.IsNullOrEmpty(token))
            {
                context.Token = token;
            }
            return Task.CompletedTask;
        }
    };
});

// ✅ CONFIGURAÇÃO DE CORS PARA COOKIES
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowNextJs", builder =>
    {
        builder.WithOrigins(
            "http://192.168.1.9:3000",
            "http://localhost:3000",
            "http://192.168.19.85:3000",
            "http://192.168.113.85:3000",
            "http://172.20.10.2:3000"
        )
        .AllowCredentials() // <- ESSENCIAL para cookies funcionarem!
        .AllowAnyMethod()
        .AllowAnyHeader();
    });
});

var app = builder.Build();

// Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowNextJs");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
