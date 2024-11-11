namespace YourProject.UITests.Core.Reporting.Documentation.Templates;

using YourProject.UITests.Core.Reporting.Documentation.Models;

internal static class DocumentationTemplates
{
    private const string BaseStyles = """
        <div style="font-family: 'Segoe UI', Arial, sans-serif; 
             font-size: 14px; color: #1a202c;
             max-width: 800px; margin: 20px 0; 
             padding: 20px; border-radius: 8px;
             background-color: #ffffff; 
             box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);">
    """;

    public static string GenerateDocumentation(TestDoc doc) => $"""
        {BaseStyles}
            <h3 style="color: #2c5282; margin: 0 0 16px;">🎯 Test Objective</h3>
            <p style="color: #4a5568; margin: 0 0 24px; line-height: 1.5;">{doc.Objective}</p>

            <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 32px;">
                <div>
                    <h4 style="color: #2c5282; margin: 0 0 12px;">📝 Test Steps</h4>
                    <div style="background-color: #f7fafc; border-radius: 6px; padding: 16px;">
                        {CreateStepsList(doc.Steps)}
                    </div>
                </div>

                <div>
                    <h4 style="color: #2c5282; margin: 0 0 12px;">✅ Success Criteria</h4>
                    <div style="background-color: #f0fff4; border-radius: 6px; padding: 16px; border: 1px solid #9ae6b4;">
                        {CreateCriteriaList(doc.SuccessCriteria)}
                    </div>
                </div>
            </div>

            {CreateMetadataSection(doc.Metadata)}
        </div>
    """;

    private static string CreateStepsList(IEnumerable<string> steps) => $"""
        <ol style="color: #718096; margin: 0; padding-left: 24px;">
            {string.Join("\n", steps.Select(step => $"""
                <li style="margin: 8px 0;">
                    <span style="color: #4a5568;">{step}</span>
                </li>
            """))}
        </ol>
    """;

    private static string CreateCriteriaList(IEnumerable<string> criteria) => $"""
        <ul style="list-style-type: none; padding: 0; margin: 0;">
            {string.Join("\n", criteria.Select(criterion => $"""
                <li style="margin: 8px 0; display: flex; align-items: center;">
                    <span style="color: #38a169; margin-right: 8px;">✓</span>
                    <span style="color: #4a5568;">{criterion}</span>
                </li>
            """))}
        </ul>
    """;

    private static string CreateMetadataSection(Dictionary<string, string> metadata)
    {
        if (!metadata.Any()) return "";

        return $"""
            <div style="margin-top: 32px;">
                <h4 style="color: #2c5282; margin: 0 0 12px;">📊 Test Details</h4>
                <div style="display: flex; gap: 16px; flex-wrap: wrap;">
                    {string.Join("\n", metadata.Select(kvp => $"""
                        <div style="display: inline-flex; align-items: center; 
                                    background-color: {GetMetadataBackground(kvp.Key)};
                                    color: {GetMetadataColor(kvp.Key)}; 
                                    padding: 6px 12px; border-radius: 16px;
                                    font-size: 14px; gap: 8px;">
                            <span>{kvp.Key}:</span>
                            <strong>{kvp.Value}</strong>
                        </div>
                    """))}
                </div>
            </div>
        """;
    }

    private static string GetMetadataBackground(string key) => key.ToLower() switch
    {
        "category" => "#ebf8ff",
        "type" => "#fff5f5", 
        "retry count" => "#f0fff4",
        _ => "#f7fafc"
    };

    private static string GetMetadataColor(string key) => key.ToLower() switch
    {
        "category" => "#2b6cb0",  
        "type" => "#c53030",
        "retry count" => "#2f855a",
        _ => "#4a5568"
    };
}