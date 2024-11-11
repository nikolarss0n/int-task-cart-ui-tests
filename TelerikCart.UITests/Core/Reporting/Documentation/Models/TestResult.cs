namespace YourProject.UITests.Core.Reporting.Documentation.Models;

public class TestResult
{
    public required bool Success { get; init; }
    public Dictionary<string, string> Data { get; init; } = new();
}