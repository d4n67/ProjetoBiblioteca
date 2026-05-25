var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Adiciona o contexto do MongoDB como serviço 
builder.Services.AddSingleton<LaeleMilVeis.Data.MongoDbContext>();


// Registrando o Repository e o Service no container de injeção de dependências
//o AddScoped cria uma nova instância da classe para cada requisição, limpando a memória depois da conclusão da rqeuisição.
builder.Services.AddScoped<LaeleMilVeis.Data.UsuarioRepository>();
builder.Services.AddScoped<LaeleMilVeis.Services.UsuarioService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
