using SmartAssist.Core.Interfaces;
using SmartAssist.Infrastructure.Data;
using SmartAssist.Infrastructure.Repositories;
using SmartAssist.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string not found.");

var rabbitHost = builder.Configuration["RabbitMQ:Host"] ?? "localhost";
var rabbitUser = builder.Configuration["RabbitMQ:Username"] ?? "smartassist";
var rabbitPass = builder.Configuration["RabbitMQ:Password"] ?? "smartassist_secret";

builder.Services.AddSingleton<IDbConnectionFactory>(
    new DbConnectionFactory(connectionString));

builder.Services.AddHttpClient<EmbeddingService>();

builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddScoped<IDocumentChunkRepository, DocumentChunkRepository>();
builder.Services.AddScoped<IIngestionJobRepository, IngestionJobRepository>();
builder.Services.AddScoped<IEmbeddingService, EmbeddingService>();
builder.Services.AddScoped<DocumentIngestionService>();
builder.Services.AddSingleton<DocumentChunker>();
builder.Services.AddHttpClient<RagQueryService>();
builder.Services.AddScoped<RagQueryService>();

builder.Services.AddSingleton(new IngestionMessageProducer(
    rabbitHost, rabbitUser, rabbitPass));

builder.Services.AddHostedService(sp => new IngestionWorker(
    sp,
    sp.GetRequiredService<ILogger<IngestionWorker>>(),
    rabbitHost,
    rabbitUser,
    rabbitPass));

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