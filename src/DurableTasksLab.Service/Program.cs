using DurableTask.AzureStorage;
using DurableTask.Core;
using DurableTasksLab.Common.DTfx;
using DurableTasksLab.Common.Subscriber;
using Microsoft;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHostedService<MyDurableTasksTopicSubscriberService>();
builder.Services.AddSingleton<IDurableTasksMessageHandler, MyDurableTasksMessageHandler>();

builder.Services.AddTransient<AzureStorageOrchestrationService>((serviceProvider) =>{ 
    var configuration = serviceProvider.GetService<IConfiguration>();
    Assumes.NotNull(configuration);
    var createTask = DurableTaskFactory.CreateOrchestrationService(configuration);
    Task.WaitAll(createTask);
    return createTask.Result;
});

builder.Services.AddTransient<TaskHubClient>((serviceProvider)=>{
    var storageOrchestrationService = serviceProvider.GetService<AzureStorageOrchestrationService>();
    Assumes.NotNull(storageOrchestrationService);
    return DurableTaskFactory.CreateTaskHubClient(storageOrchestrationService);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
