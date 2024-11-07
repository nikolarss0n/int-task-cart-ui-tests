using NUnit.Framework;
using TelerikCart.UITests.Core.Base;
using TelerikCart.UITests.Core.Reporting;
using TelerikCart.UITests.Pages;
using System.Diagnostics;

namespace TelerikCart.UITests.Tests;

[TestFixture]
[Category("Shopping Cart")]
[Parallelizable(ParallelScope.Self)]  
public class ContactInfoTests : BaseTest
{
    private PurchasePage? _purchasePage;
    // private ContactInfoPage? _contactInfoPage;
    private readonly Stopwatch _testStopwatch = new();

    [OneTimeSetUp]
    public void ClassSetup()
    {
        ExtentTestManager.LogInfo("===== Starting Contact Info Tests Suite =====");
    }

    [SetUp]
    public void TestSetup()
    {
        _testStopwatch.Restart();
        _purchasePage = new PurchasePage(Driver);
        // _contactInfoPage = new ContactInfoPage(Driver);
        ExtentTestManager.LogInfo($"Test setup completed in {_testStopwatch.ElapsedMilliseconds}ms");
    }

    [Test]
    [Description("Verify all required contact info fields can be filled")]
    [Category("Contact Info")]
    [Retry(2)]  
    public void CanFillAllContactInfoFields()
    {
        // Arrange
        ExtentTestManager.CreateTest(TestContext.CurrentContext.Test.Name)
            .AssignCategory("Contact Info")
            .AssignDevice("Chrome")
            .AssignAuthor("QA Team");

        try
        {
            // Test steps:
            // 1. Navigate to purchase page
            // 2. Add product to cart
            // 3. Proceed to contact info
            // 4. Fill all required fields
            // 5. Verify form can be submitted
        }
        catch (Exception ex)
        {
            ExtentTestManager.LogFail($"Test failed after {_testStopwatch.ElapsedMilliseconds}ms", ex);
            ExtentTestManager.LogScreenshot(Driver, "Failure state");
            throw;
        }
    }

    [Test]
    [Description("Verify VAT ID validation for Bulgarian customers")]
    [Category("Contact Info")]
    [TestCase("valid-vat-id", true)]
    [TestCase("invalid-vat-id", false)]
    [Retry(2)]
    public void ValidatesVatIdForBulgarianCustomers(string vatId, bool shouldBeValid)
    {
        try
        {
            // Test steps:
            // 1. Navigate to purchase page
            // 2. Add product to cart
            // 3. Proceed to contact info
            // 4. Select Bulgaria as country
            // 5. Enter VAT ID
            // 6. Verify validation result matches expectation
        }
        catch (Exception ex)
        {
            ExtentTestManager.LogFail($"Test failed after {_testStopwatch.ElapsedMilliseconds}ms", ex);
            ExtentTestManager.LogScreenshot(Driver, "Failure state");
            throw;
        }
    }

    [Test]
    [Description("Verify email validation in contact info")]
    [Category("Contact Info")]
    [Retry(2)]
    public void ValidatesEmailFormat(string email, bool shouldBeValid)
    {
        try
        {
            // Test steps:
            // 1. Navigate to contact info page
            // 2. Enter test email
            // 3. Verify validation result
            // 4. If valid, verify form can proceed
            // 5. If invalid, verify error message
        }
        catch (Exception ex)
        {
            ExtentTestManager.LogFail($"Test failed after {_testStopwatch.ElapsedMilliseconds}ms", ex);
            ExtentTestManager.LogScreenshot(Driver, "Failure state");
            throw;
        }
    }

    [Test]
    [Description("Verify company field validation in contact info")]
    [Category("Contact Info")]
    [TestCase("valid@email.com", true)]
    [TestCase("invalid-email", false)]
    [Retry(2)]
    public void ValidatesCompanyField()
    {
        try
        {
            // Test steps:
            // 1. Navigate to contact info page
            // 2. Leave company field empty
            // 3. Verify validation message
            // 4. Enter valid company
            // 5. Verify validation passes
        }
        catch (Exception ex)
        {
            ExtentTestManager.LogFail($"Test failed after {_testStopwatch.ElapsedMilliseconds}ms", ex);
            ExtentTestManager.LogScreenshot(Driver, "Failure state");
            throw;
        }
    }

    [TearDown]
    public void TestCleanup()
    {
        try
        {
            if (_purchasePage != null)
            {
                ExtentTestManager.LogInfo($"Cleaning up test resources. Test duration: {_testStopwatch.ElapsedMilliseconds}ms");
            }
        }
        catch (Exception ex)
        {
            ExtentTestManager.LogInfo($"Cleanup failed: {ex.Message}");
        }
        finally
        {
            _testStopwatch.Stop();
        }
    }

    [OneTimeTearDown]
    public void ClassCleanup()
    {
        ExtentTestManager.LogInfo("Completed Cart Tests Suite");
    }
}