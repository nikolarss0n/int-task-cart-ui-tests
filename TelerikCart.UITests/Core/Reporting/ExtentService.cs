// Import ExtentReports namespaces for reporting functionalities
using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;
using AventStack.ExtentReports.Reporter.Configuration;

namespace TelerikCart.UITests.Core.Reporting
{
    /// <summary>
    /// Initializes and manages the singleton instance of ExtentReports.
    /// </summary>
    public class ExtentService
    {
        // Lazy initialization of ExtentReports
        private static readonly Lazy<ExtentReports> _lazy = new Lazy<ExtentReports>(() => 
        {
            // Get project directory by moving up three levels from the base directory
            var projectDir = Path.GetFullPath(Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "..", "..", ".."));
            Console.WriteLine($"Project Directory: {projectDir}");

            // Set up report directory with timestamp
            var reportDirectory = Path.Combine(projectDir, "TestResults",
                $"Report_{DateTime.Now:yyyyMMdd_HHmmss}");
            Console.WriteLine($"Report Directory: {reportDirectory}");

            // Define report file path
            var reportPath = Path.Combine(reportDirectory, "index.html");
            Console.WriteLine($"Report Path: {reportPath}");

            // Configure HTML reporter
            var reporter = new ExtentHtmlReporter(reportPath)
            {
                Config = 
                {
                    DocumentTitle = "Telerik Cart UI Test Report",
                    ReportName = "Test Execution Report",
                    Theme = Theme.Dark
                }
            };

            // Initialize ExtentReports and attach reporter
            var extent = new ExtentReports();
            extent.AttachReporter(reporter);

            // Add system information
            extent.AddSystemInfo("Operating System", Environment.OSVersion.ToString());
            extent.AddSystemInfo("Machine Name", Environment.MachineName);
            extent.AddSystemInfo("Browser", "Chrome");
            extent.AddSystemInfo("Test Environment", "QA");
            extent.AddSystemInfo("Test Run Date", DateTime.Now.ToString());

            // Assign report path
            ReportPath = reportPath;

            return extent;
        });

        /// <summary>
        /// Path to the generated report file.
        /// </summary>
        public static string ReportPath { get; private set; } = string.Empty;

        /// <summary>
        /// Singleton instance of ExtentReports.
        /// </summary>
        public static ExtentReports Instance => _lazy.Value;

        // Private constructor to prevent external instantiation
        private ExtentService() { }

        /// <summary>
        /// Flushes the ExtentReports data to the report file.
        /// </summary>
        public static void Flush()
        {
            if (_lazy.IsValueCreated)
            {
                Console.WriteLine($"\nTest report generated: {ReportPath}\n");
                Instance.Flush();
            }
        }
    }
}
