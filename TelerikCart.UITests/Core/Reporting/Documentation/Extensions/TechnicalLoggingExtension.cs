namespace YourProject.UITests.Core.Reporting.Documentation.Extensions;

using AventStack.ExtentReports;

public static class TechnicalLoggingExtensions 
{
    public static void LogStep(this ExtentTest test, string component, string action, bool isSuccess = true)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        var icon = isSuccess ? "✓" : "ℹ️";
        var style = isSuccess 
            ? "color: #047857;" // Green for success
            : "color: #6B7280;"; // Gray for info
        
        test.Info($"""
            <div style="font-family: monospace; font-size: 12px; display: flex; gap: 12px; padding: 4px 8px;">
                <span style="color: #6B7280; min-width: 85px;">{timestamp}</span>
                <span style="{style}">{icon}</span>
                <span style="color: #6B7280;">[{component}]</span>
                <span style="color: #1F2937;">{action}</span>
            </div>
        """);
    }

    public static void LogWarning(this ExtentTest test, string component, string message)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        test.Warning($"""
            <div style="font-family: monospace; font-size: 12px; display: flex; gap: 12px; padding: 4px 8px;">
                <span style="color: #6B7280; min-width: 85px;">{timestamp}</span>
                <span style="color: #D97706;">⚠️</span>
                <span style="color: #6B7280;">[{component}]</span>
                <span style="color: #92400E;">{message}</span>
            </div>
        """);
    }

    public static void LogError(this ExtentTest test, string component, string message, Exception? ex = null)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        var errorDetails = ex != null ? $"\nException: {ex.Message}" : "";
        
        test.Error($"""
            <div style="font-family: monospace; font-size: 12px; display: flex; gap: 12px; padding: 4px 8px;">
                <span style="color: #6B7280; min-width: 85px;">{timestamp}</span>
                <span style="color: #DC2626;">❌</span>
                <span style="color: #6B7280;">[{component}]</span>
                <span style="color: #991B1B;">{message}{errorDetails}</span>
            </div>
        """);
    }
}