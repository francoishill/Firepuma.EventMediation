using Firepuma.EventMediation.Simple;
using Sample.EventMediation.InMemory.EventHandlers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var assembliesWithEventHandlers = new[]
{
    typeof(SampleIntegrationEventHandler).Assembly,
}.Distinct().ToArray();
builder.Services.AddIntegrationEventMediation(assembliesWithEventHandlers);

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