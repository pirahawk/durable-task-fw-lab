namespace DurableTasksLab.Common.Messaging;

public record SimpleOrchestrationMessage
{
    public Guid Id { get; set; }
    public int Sequence { get; set; }
    public string? Message { get; set; }
}
