using System.Diagnostics;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using OpenQA.Selenium;
using TelerikCart.UITests.Core.Reporting;

namespace TelerikCart.UITests.Core.Base
{
    /// <summary>
    /// Base class for all tests, handling setup and teardown operations.
    /// </summary>
    public abstract class BaseTest
    {
        /// <summary>
        /// WebDriver instance for browser interactions.
        /// </summary>
        protected IWebDriver Driver { get; private set; } = null!;

        private DateTime _testStartTime;
        private string _currentTestName = string.Empty;

        /// <summary>
        /// Executes once before all tests in the test suite.
        /// </summary>
        [OneTimeSetUp]
        public void BeforeAll()
        {
            LogTestSuiteStart();
        }

        /// <summary>
        /// Executes before each test, initializing test context and WebDriver.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            _testStartTime = DateTime.Now;
            InitializeTest();
            InitializeDriver();
        }

        /// <summary>
        /// Executes after each test, logging results and quitting WebDriver.
        /// </summary>
        [TearDown]
        public void Cleanup()
        {
            try
            {
                LogTestEnd();
                CaptureTestResult();
            }
            finally
            {
                DriverFactory.QuitDriver();
            }
        }

        /// <summary>
        /// Executes once after all tests in the test suite, flushing reports.
        /// </summary>
        [OneTimeTearDown]
        public void AfterAll()
        {
            LogTestSuiteEnd();
            ExtentTestManager.Flush();
            ExtentService.Flush();
        }

        /// <summary>
        /// Initializes test details and assigns categories, device, and author.
        /// </summary>
        private void InitializeTest()
        {
            _currentTestName = TestContext.CurrentContext.Test.Properties.Get("Description")?.ToString() 
                              ?? TestContext.CurrentContext.Test.Name;

            ExtentTestManager.CreateTest(_currentTestName)
                .AssignCategory(GetTestCategory())
                .AssignDevice("Chrome")
                .AssignAuthor("QA Team");
        }

        /// <summary>
        /// Creates and assigns the WebDriver instance.
        /// </summary>
        private void InitializeDriver()
        {
            Driver = DriverFactory.CreateDriver();
        }

        /// <summary>
        /// Retrieves the test category from NUnit attributes.
        /// </summary>
        /// <returns>Test category name or "Uncategorized".</returns>
        private string GetTestCategory()
        {
            var categories = TestContext.CurrentContext.Test.Properties["Category"]?.ToString();
            return !string.IsNullOrEmpty(categories) ? categories : "Uncategorized";
        }

        /// <summary>
        /// Logs the start of the test suite with a timestamp and report location.
        /// </summary>
        private void LogTestSuiteStart()
        {
            var message = $"""
                ========================================
                Test Suite Starting
                Report Location: {ExtentService.ReportPath}
                Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
                ========================================
                """;
            
            Console.WriteLine(message);
            ExtentTestManager.LogInfo(message);
        }

        /// <summary>
        /// Logs the duration of the completed test.
        /// </summary>
        private void LogTestEnd()
        {
            var duration = DateTime.Now - _testStartTime;
            ExtentTestManager.LogInfo($"⏰ Test Duration: {duration.TotalSeconds:F2} seconds");
        }

        /// <summary>
        /// Logs the end of the test suite with status and timestamp.
        /// </summary>
        private void LogTestSuiteEnd()
        {
            var result = TestContext.CurrentContext.Result;
            var status = result.Outcome.Status;
            
            var summary = $"""
                ========================================
                Test Suite Completed
                Status: {status}
                Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
                ========================================
                """;
            
            Console.WriteLine(summary);
            ExtentTestManager.LogInfo(summary);
        }

        /// <summary>
        /// Captures and logs the result of the test, handling different outcomes.
        /// </summary>
        private void CaptureTestResult()
        {
            var outcome = TestContext.CurrentContext.Result;
                
            switch (outcome.Outcome.Status)
            {
                case TestStatus.Failed:
                    HandleFailedTest(outcome);
                    break;
                        
                case TestStatus.Passed:
                    ExtentTestManager.LogPass($"✅ Test passed successfully: {_currentTestName}");
                    break;
                        
                case TestStatus.Skipped:
                    ExtentTestManager.LogWarning($"⚠️ Test skipped: {_currentTestName}");
                    break;
            }
        }

        /// <summary>
        /// Handles logging for failed tests, including screenshots and failure details.
        /// </summary>
        /// <param name="outcome">Test result details.</param>
        private void HandleFailedTest(TestContext.ResultAdapter outcome)
        {
            if (Driver != null)
            {
                ExtentTestManager.LogScreenshot(Driver, "❌ Test Failed - Final State");
            }
            
            var failureInfo = $"""
                Test Failed: {_currentTestName}
                Message: {outcome.Message ?? "No error message provided"}
                Stack Trace: {outcome.StackTrace ?? "No stack trace available"}
                """;
            
            ExtentTestManager.LogFail(failureInfo);
        }
    }
}
