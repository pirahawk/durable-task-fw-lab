using DurableTask.AzureStorage;
using DurableTask.Core;
using DurableTasksLab.Common.DTfx;
using DurableTasksLab.Common.DTfx.Core;
using DurableTasksLab.Common.DTfx.Orchestrations.Simple;
using DurableTasksLab.Common.Subscriber;
using Microsoft;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHostedService<MyDurableTasksWorkerService>();
builder.Services.AddSingleton<IDurableTasksMessageHandler, MyDurableTasksMessageHandler>();

builder.Services.AddTransient<AzureStorageOrchestrationService>((serviceProvider) =>
{
    var configuration = serviceProvider.GetService<IConfiguration>();
    Assumes.NotNull(configuration);
    var createTask = DurableTaskFactory.CreateOrchestrationService(configuration);
    Task.WaitAll(createTask);
    return createTask.Result;
});

#if AllowTaskHubClient

builder.Services.AddTransient<TaskHubClient>((serviceProvider) =>
{
    var storageOrchestrationService = serviceProvider.GetService<AzureStorageOrchestrationService>();
    Assumes.NotNull(storageOrchestrationService);
    return DurableTaskFactory.CreateTaskHubClient(storageOrchestrationService);
});

#endif

builder.Services.AddTransient<TaskHubWorker>((serviceProvider) =>
{
    var storageOrchestrationService = serviceProvider.GetService<AzureStorageOrchestrationService>();
    var orchestrationObjectManager = serviceProvider.GetService<HostBuildingNameVersionObjectManager<TaskOrchestration>>();
    var activityObjectManager = serviceProvider.GetService<HostBuildingNameVersionObjectManager<TaskActivity>>();

    Assumes.NotNull(storageOrchestrationService);
    Assumes.NotNull(orchestrationObjectManager);
    Assumes.NotNull(activityObjectManager);

    var hub = DurableTaskFactory.CreateTaskHubWorker(storageOrchestrationService, orchestrationObjectManager, activityObjectManager);
    var allDTFxRegistrations = serviceProvider.GetServices<IHostBuildingObjectCreator>();

    foreach (var registration in allDTFxRegistrations)
    {
        registration.Register(hub);
    }

    return hub;
});

builder.Services.AddHostBuildingNameVersionObjectManager<TaskOrchestration>();
builder.Services.AddHostBuildingNameVersionObjectManager<TaskActivity>();

builder.Services.AddActivity<SimpleTaskOne>();
builder.Services.AddActivity<TrackPerformanceTask>();
builder.Services.AddOrchestration<SimpleOrchestration>();

builder.Services.AddApplicationInsightsTelemetry();

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
    var forecast = Enumerable.Range(1, 5).Select(index =>
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
