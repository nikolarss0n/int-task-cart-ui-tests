using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;
using AventStack.ExtentReports.Reporter.Configuration;
using System.Diagnostics;

namespace TelerikCart.UITests.Core.Reporting;

public class ExtentService
{
    private static readonly Lazy<ExtentReports> _lazy = new Lazy<ExtentReports>(() => 
    {
        var reportDirectory = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, 
            "TestResults", 
            $"Report_{DateTime.Now:yyyyMMdd_HHmmss}");
        
        Directory.CreateDirectory(reportDirectory);
        var reportPath = Path.Combine(reportDirectory, "index.html");
        ReportPath = reportPath; // Store report path for later use
        
        var reporter = new ExtentHtmlReporter(reportPath);
        reporter.Config.DocumentTitle = "Telerik Cart UI Test Report";
        reporter.Config.ReportName = "Test Execution Report";
        reporter.Config.Theme = Theme.Dark;
        
        var extent = new ExtentReports();
        extent.AttachReporter(reporter);
        
        extent.AddSystemInfo("Operating System", Environment.OSVersion.ToString());
        extent.AddSystemInfo("Machine Name", Environment.MachineName);
        extent.AddSystemInfo("Browser", "Chrome");
        extent.AddSystemInfo("Test Environment", "QA");
        extent.AddSystemInfo("Test Run Date", DateTime.Now.ToString());
        
        return extent;
    });

    public static string ReportPath { get; private set; }

    public static ExtentReports Instance => _lazy.Value;

    private ExtentService() { }

    public static void OpenReport()
    {
        if (File.Exists(ReportPath))
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = ReportPath,
                    UseShellExecute = true
                };
                Process.Start(psi);
                Console.WriteLine($"Report opened: {ReportPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not open report automatically: {ex.Message}");
                Console.WriteLine($"Please open manually: {ReportPath}");
            }
        }
        else
        {
            Console.WriteLine($"Report file not found: {ReportPath}");
        }
    }
}