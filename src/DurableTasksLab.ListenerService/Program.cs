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

builder.Services.AddTransient<AzureStorageOrchestrationService>((serviceProvider) =>
{
    var configuration = serviceProvider.GetService<IConfiguration>();
    Assumes.NotNull(configuration);
    var createTask = DurableTaskFactory.CreateOrchestrationService(configuration);
    Task.WaitAll(createTask);
    return createTask.Result;
});

builder.Services.AddTransient<TaskHubClient>((serviceProvider) =>
{
    var storageOrchestrationService = serviceProvider.GetService<AzureStorageOrchestrationService>();
    Assumes.NotNull(storageOrchestrationService);
    return DurableTaskFactory.CreateTaskHubClient(storageOrchestrationService);
});

builder.Services.AddApplicationInsightsTelemetry();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/", () => "Hello World!");

app.Run();
