namespace YourProject.UITests.Core.Reporting.Documentation.Extensions;

using AventStack.ExtentReports;
using YourProject.UITests.Core.Reporting.Documentation.Models;
using YourProject.UITests.Core.Reporting.Documentation.Templates;

public static class DocumentationExtensions
{
    public static ExtentTest DocumentTest(
        this ExtentTest test,
        string objective,
        string[] steps,
        string[] criteria)
    {
        var markup = "<div style='margin: 10px 0; padding: 10px; background-color: #f8f9fa;'>";
            
        // Objective
        markup += "<div style='margin-bottom: 15px;'>";
        markup += "<h4 style='margin: 0; color: #2f4f4f;'>🎯 Objective</h4>";
        markup += $"<p style='margin: 5px 0 0 15px;'>{objective}</p>";
        markup += "</div>";
            
        // Test Steps
        markup += "<div style='margin-bottom: 15px;'>";
        markup += "<h4 style='margin: 0; color: #2f4f4f;'>📋 Test Steps</h4>";
        markup += "<ol style='margin: 5px 0 0 15px;'>";
        foreach (var step in steps)
        {
            markup += $"<li>{step}</li>";
        }
        markup += "</ol></div>";
            
        // Success Criteria
        markup += "<div>";
        markup += "<h4 style='margin: 0; color: #2f4f4f;'>✨ Success Criteria</h4>";
        markup += "<ul style='margin: 5px 0 0 15px;'>";
        foreach (var criterion in criteria)
        {
            markup += $"<li>{criterion}</li>";
        }
        markup += "</ul></div>";
            
        markup += "</div>";
            
        test.Info(markup);
        return test;
    }
    
    public static void LogResults(this ExtentTest? test, bool passed, Dictionary<string, string> results)
    {
        if (test == null) return;
        var status = passed ? "✅ PASSED" : "❌ FAILED";
        
        var markup = $"<div style='margin-top: 10px; padding: 10px; background-color: {(passed ? "#efffef" : "#fff0f0")};'>";
        markup += $"<h4 style='margin: 0; color: {(passed ? "#2d862d" : "#cc0000")};'>Test Status: {status}</h4>";
        markup += "<table style='width: 100%; margin-top: 10px;'>";
        markup += "<tr><th style='text-align: left; padding: 5px;'>Metric</th><th style='text-align: right; padding: 5px;'>Value</th></tr>";

        foreach (var result in results)
        {
            markup += $"<tr><td style='padding: 5px;'>{result.Key}</td><td style='text-align: right; padding: 5px;'>{result.Value}</td></tr>";
        }

        markup += "</table></div>";
        test.Info(markup);
    }
}