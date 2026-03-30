using SmartAssist.Core.Interfaces;
using SmartAssist.Infrastructure.Data;
using SmartAssist.Infrastructure.Repositories;
using SmartAssist.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddSingleton<IDbConnectionFactory>(
    new DbConnectionFactory(connectionString));

builder.Services.AddHttpClient<EmbeddingService>();

builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddScoped<IDocumentChunkRepository, DocumentChunkRepository>();
builder.Services.AddScoped<IIngestionJobRepository, IngestionJobRepository>();
builder.Services.AddScoped<IEmbeddingService, EmbeddingService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();