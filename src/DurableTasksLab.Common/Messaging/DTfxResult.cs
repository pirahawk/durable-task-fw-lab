namespace DurableTasksLab.Common.Messaging;

public record DTfxResult{
    public bool Success {get;set;}
    public string? Message {get;set;}
}
