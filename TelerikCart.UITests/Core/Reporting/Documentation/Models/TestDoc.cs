namespace YourProject.UITests.Core.Reporting.Documentation.Models;

public class TestDoc
{
    public required string Objective { get; init; }
    public required List<string> Steps { get; init; }
    public required List<string> SuccessCriteria { get; init; }
    public Dictionary<string, string> Metadata { get; init; } = new();
}