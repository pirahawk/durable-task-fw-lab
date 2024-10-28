namespace DurableTasksLab.Common.Messaging;

public record OrchestrationBatchMessage
{
    public string? InstanceId { get; set; }
    public string? ExecutionId { get; set; }
    public string? Status { get; set; }

}
