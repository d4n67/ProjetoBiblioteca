using LaeleMilVeis.Data;
using LaeleMilVeis.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Em Development, carrega appsettings.Development.local.json (não versionado no Git)
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddJsonFile(
        "appsettings.Development.local.json",
        optional: true,
        reloadOnChange: true);
}

ValidarConfiguracoesObrigatorias(builder.Configuration);

builder.Services.AddControllers();

// swagger tem que aceitar o token jwt
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "BibliotecaApi", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization Header usando o esquema Bearer. Exemplo: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// Registrar serviços e repositórios
builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddScoped<UsuarioRepository>();
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<LivroRepository>();
builder.Services.AddScoped<LivroService>();
builder.Services.AddScoped<TokenService>(); 

var secret = builder.Configuration["JwtSettings:Secret"]!;
var key = Encoding.ASCII.GetBytes(secret);

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration.GetSection("JwtSettings:Issuer").Value,
        ValidateAudience = true,
        ValidAudience = builder.Configuration.GetSection("JwtSettings:Audience").Value,
        ValidateLifetime = true
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            // Vite pode usar 5173, 5174, etc. se a porta padrão estiver ocupada
            policy.SetIsOriginAllowed(origin =>
            {
                if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                    return false;

                return uri.Host is "localhost" or "127.0.0.1";
            });
        }
        else
        {
            policy.WithOrigins(
                "http://localhost:3000",
                "http://localhost:5173",
                "http://localhost:5174");
        }

        policy.AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowReact");
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

static void ValidarConfiguracoesObrigatorias(IConfiguration configuration)
{
    var connectionString = configuration["MongoDbSettings:ConnectionString"];
    var jwtSecret = configuration["JwtSettings:Secret"];

    if (string.IsNullOrWhiteSpace(connectionString) || string.IsNullOrWhiteSpace(jwtSecret))
    {
        throw new InvalidOperationException(
            "Configurações sensíveis ausentes. Defina MongoDbSettings:ConnectionString e JwtSettings:Secret via " +
            "User Secrets (dotnet user-secrets set), appsettings.Development.local.json (copie de appsettings.Development.local.json.example) " +
            "ou variáveis de ambiente. Consulte o README.md na raiz do repositório.");
    }
}