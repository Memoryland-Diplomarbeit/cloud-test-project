using CloudTestProject.Service;
using Microsoft.Extensions.Azure;
using Persistence;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddUserSecrets<Program>();

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>();
builder.Services.AddSingleton<BlobStoragePhotoService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var storageConnection = builder.Configuration["ConnectionStrings:BlobStorageDefault"];

builder.Services.AddAzureClients(azureBuilder =>
{
    azureBuilder.AddBlobServiceClient(storageConnection);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("hello", () => "Hello World!");

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
