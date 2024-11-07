using NUnit.Framework;
using NUnit.Framework.Interfaces;
using OpenQA.Selenium;
using TelerikCart.UITests.Core.Reporting;

namespace TelerikCart.UITests.Core.Base;

public abstract class BaseTest
{
    protected IWebDriver Driver { get; private set; }
    
    [OneTimeSetUp]
    public void BeforeAll()
    {
        Console.WriteLine($"Report will be generated at: {ExtentService.ReportPath}");
    }

    [SetUp]
    public void Setup()
    {
        Driver = DriverFactory.CreateDriver();
        
        var testName = TestContext.CurrentContext.Test.Properties.Get("Description")?.ToString() 
                       ?? TestContext.CurrentContext.Test.Name;
            
        ExtentTestManager.CreateTest(testName)
            .AssignCategory("Shopping Cart")
            .AssignDevice("Chrome")
            .AssignAuthor("QA Team");
            
        ExtentTestManager.LogInfo($"Starting test: {testName}");
    }

    [TearDown]
    public void Cleanup()
    {
        try
        {
            var outcome = TestContext.CurrentContext.Result.Outcome;
            switch (outcome.Status)
            {
                case TestStatus.Failed:
                    ExtentTestManager.LogScreenshot(Driver, "Test Failed - Final State");
                    ExtentTestManager.LogFail(
                        TestContext.CurrentContext.Result.Message ?? "Test failed without error message",
                        TestContext.CurrentContext.Result.StackTrace != null 
                            ? new Exception(TestContext.CurrentContext.Result.StackTrace) 
                            : null);
                    break;
                case TestStatus.Passed:
                    ExtentTestManager.LogPass("Test executed successfully");
                    ExtentTestManager.LogScreenshot(Driver, "Final successful state");
                    break;
                case TestStatus.Skipped:
                    ExtentTestManager.LogWarning("Test was skipped");
                    break;
            }
        }
        catch (Exception ex)
        {
            ExtentTestManager.LogFail("Failed to properly complete test teardown", ex);
        }
        finally
        {
            DriverFactory.QuitDriver();
        }
    }

    [OneTimeTearDown]
    public void AfterAll()
    {
        ExtentTestManager.Flush();
        ExtentService.OpenReport();
    }
}