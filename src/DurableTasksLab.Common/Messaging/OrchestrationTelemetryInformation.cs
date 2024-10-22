namespace DurableTasksLab.Common.Messaging;

public record OrchestrationTelemetryInformation{
    public string? OrchestrationInstanceId { get; set; }
    public DateTime OrchestrationStartTimeUTC {get;set;}
}
