namespace YourProject.UITests.Core.Reporting.Documentation.Templates;

using YourProject.UITests.Core.Reporting.Documentation.Models;

internal static class ResultsTemplates
{
    public static string GenerateResults(TestResult result) => $"""
                                                                    <div style="font-family: 'Segoe UI', Arial, sans-serif; margin-top: 20px;">
                                                                        <div style="background: {(result.Success ? "#f0fff4" : "#fff5f5")}; 
                                                                             border-radius: 6px; padding: 16px; 
                                                                             border: 1px solid {(result.Success ? "#9ae6b4" : "#feb2b2")};">
                                                                            <h4 style="color: #2c5282; margin: 0 0 12px 0;">
                                                                                {(result.Success ? "✅" : "❌")} Test Results
                                                                            </h4>
                                                                            {CreateResultsList(result.Data)}
                                                                        </div>
                                                                    </div>
                                                                """;

    private static string CreateResultsList(Dictionary<string, string> data) => $"""
             <ul style="list-style-type: none; padding: 0; margin: 0;">
                 {string.Join("\n", data.Select(kvp => $"""
                                                            <li style="margin: 8px 0;">
                                                                <span style="color: #2f855a; font-weight: bold;">{kvp.Key}:</span> {kvp.Value}
                                                            </li>
                                                        """))}
             </ul>
         """;
}