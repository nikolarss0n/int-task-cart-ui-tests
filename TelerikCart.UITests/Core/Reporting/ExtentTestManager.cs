using AventStack.ExtentReports;
using OpenQA.Selenium;
namespace TelerikCart.UITests.Core.Reporting;

public class ExtentTestManager
{
    private static readonly Dictionary<string, ExtentTest> ParentTestMap = new();
    private static readonly ThreadLocal<ExtentTest> ParentTest = new();
    private static readonly ThreadLocal<ExtentTest> ChildTest = new();
    private static readonly object SyncLock = new();

    public static ExtentTest CreateTest(string testName, string description = null)
    {
        lock (SyncLock)
        {
            var test = ExtentService.Instance.CreateTest(testName, description);
            ParentTest.Value = test;
            return test;
        }
    }

    public static ExtentTest CreateNode(string parentName, string testName, string description = null)
    {
        lock (SyncLock)
        {
            ExtentTest parentTest;
            if (!ParentTestMap.ContainsKey(parentName))
            {
                parentTest = ExtentService.Instance.CreateTest(parentName);
                ParentTestMap.Add(parentName, parentTest);
            }
            else
            {
                parentTest = ParentTestMap[parentName];
            }

            ParentTest.Value = parentTest;
            ChildTest.Value = parentTest.CreateNode(testName, description);
            return ChildTest.Value;
        }
    }

    public static void LogInfo(string message)
    {
        GetTest()?.Info(message);
    }
    
    public static void LogWarning(string message)
    {
        GetTest()?.Warning(message);
    }

    public static void LogPass(string message)
    {
        GetTest()?.Pass(message);
    }

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

    public static void LogScreenshot(IWebDriver driver, string message)
    {
        try
        {
            var test = GetTest();
            if (test == null) return;

            // Get report directory
            var reportDir = Path.GetDirectoryName(ExtentService.ReportPath);
            if (string.IsNullOrEmpty(reportDir))
            {
                LogWarning("Report directory path is null or empty");
                return;
            }

            // Create screenshot filename
            var fileName = $"screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png";
        
            // Create Screenshots directory inside report directory
            var screenshotDirectory = Path.Combine(reportDir, "Screenshots");
            Directory.CreateDirectory(screenshotDirectory);
        
            // Full path for saving screenshot
            var screenshotPath = Path.Combine(screenshotDirectory, fileName);

            // Take and save screenshot
            var screenshot = ((ITakesScreenshot)driver).GetScreenshot();
            screenshot.SaveAsFile(screenshotPath);

            // Use relative path for the report
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

    private static ExtentTest? GetTest()
    {
        lock (SyncLock)
        {
            return ChildTest.Value ?? ParentTest.Value;
        }
    }

    public static void Flush()
    {
        ExtentService.Instance.Flush();
    }
}