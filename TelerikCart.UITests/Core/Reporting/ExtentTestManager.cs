// Import necessary namespaces for ExtentReports and Selenium WebDriver functionalities
using AventStack.ExtentReports;
using OpenQA.Selenium;

namespace TelerikCart.UITests.Core.Reporting
{
    /// <summary>
    /// Manages ExtentTest instances for reporting test executions.
    /// </summary>
    public class ExtentTestManager
    {
        // Maps parent test names to their ExtentTest instances
        private static readonly Dictionary<string, ExtentTest> ParentTestMap = new();

        // Thread-local storage for parent tests to ensure thread safety
        private static readonly ThreadLocal<ExtentTest> ParentTest = new();

        // Thread-local storage for child tests (nodes) to ensure thread safety
        private static readonly ThreadLocal<ExtentTest> ChildTest = new();

        // Object for synchronizing access to shared resources
        private static readonly object SyncLock = new();

        /// <summary>
        /// Creates a new parent test.
        /// </summary>
        /// <param name="testName">Name of the test.</param>
        /// <param name="description">Optional description.</param>
        /// <returns>Created ExtentTest instance.</returns>
        public static ExtentTest CreateTest(string testName, string description = null)
        {
            lock (SyncLock)
            {
                var test = ExtentService.Instance.CreateTest(testName, description);
                ParentTest.Value = test;
                return test;
            }
        }

        /// <summary>
        /// Creates a child test (node) under a specified parent test.
        /// </summary>
        /// <param name="parentName">Name of the parent test.</param>
        /// <param name="testName">Name of the child test.</param>
        /// <param name="description">Optional description.</param>
        /// <returns>Created child ExtentTest instance.</returns>
        public static ExtentTest CreateNode(string parentName, string testName, string description = null)
        {
            lock (SyncLock)
            {
                if (!ParentTestMap.TryGetValue(parentName, out var parentTest))
                {
                    parentTest = ExtentService.Instance.CreateTest(parentName);
                    ParentTestMap.Add(parentName, parentTest);
                }

                ParentTest.Value = parentTest;
                ChildTest.Value = parentTest.CreateNode(testName, description);
                return ChildTest.Value;
            }
        }

        /// <summary>
        /// Logs an informational message to the current test.
        /// </summary>
        /// <param name="message">Message to log.</param>
        public static void LogInfo(string message)
        {
            GetTest()?.Info(message);
        }

        /// <summary>
        /// Logs a warning message to the current test.
        /// </summary>
        /// <param name="message">Warning message to log.</param>
        public static void LogWarning(string message)
        {
            GetTest()?.Warning(message);
        }

        /// <summary>
        /// Logs a pass status with a message to the current test.
        /// </summary>
        /// <param name="message">Pass message to log.</param>
        public static void LogPass(string message)
        {
            GetTest()?.Pass(message);
        }

        /// <summary>
        /// Logs a failure status with a message and optional exception to the current test.
        /// </summary>
        /// <param name="message">Failure message to log.</param>
        /// <param name="ex">Optional exception details.</param>
        public static void LogFail(string message, Exception ex = null)
        {
            var test = GetTest();
            if (test != null)
            {
                if (ex != null)
                {
                    test.Fail($"{message}\nException: {ex.Message}\nStack Trace: {ex.StackTrace}");
                }
                else
                {
                    test.Fail(message);
                }
            }
        }

        /// <summary>
        /// Captures a screenshot and attaches it to the current test.
        /// </summary>
        /// <param name="driver">WebDriver instance.</param>
        /// <param name="message">Message describing the screenshot context.</param>
        public static void LogScreenshot(IWebDriver driver, string message)
        {
            try
            {
                var test = GetTest();
                if (test == null) return;

                var reportDir = Path.GetDirectoryName(ExtentService.ReportPath);
                if (string.IsNullOrEmpty(reportDir))
                {
                    LogWarning("Report directory path is null or empty");
                    return;
                }

                var fileName = $"screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                var screenshotDirectory = Path.Combine(reportDir, "Screenshots");
                Directory.CreateDirectory(screenshotDirectory);
                var screenshotPath = Path.Combine(screenshotDirectory, fileName);

                var screenshot = ((ITakesScreenshot)driver).GetScreenshot();
                screenshot.SaveAsFile(screenshotPath);

                test.Info(message, MediaEntityBuilder
                    .CreateScreenCaptureFromPath(
                        Path.Combine(".", "Screenshots", fileName))
                    .Build());

                LogInfo($"Screenshot saved: {screenshotPath}");
            }
            catch (Exception ex)
            {
                LogWarning($"Failed to capture screenshot: {ex.Message}");
                Console.WriteLine($"Screenshot error: {ex}");
            }
        }

        /// <summary>
        /// Retrieves the current active ExtentTest instance.
        /// </summary>
        /// <returns>Active ExtentTest instance or null.</returns>
        public static ExtentTest? GetTest()
        {
            lock (SyncLock)
            {
                return ChildTest.Value ?? ParentTest.Value;
            }
        }

        /// <summary>
        /// Flushes all ExtentReports data to the report.
        /// </summary>
        public static void Flush()
        {
            ExtentService.Instance.Flush();
        }
    }
}
