using NUnit.Framework;
using TelerikCart.UITests.Core.Base;
using TelerikCart.UITests.Core.Reporting;
using TelerikCart.UITests.Pages;
using System.Diagnostics;

namespace TelerikCart.UITests.Tests;

[TestFixture]
[Category("Shopping Cart")]
[Parallelizable(ParallelScope.Self)]  
public class CartTests : BaseTest
{
    private PurchasePage? _purchasePage;
    private readonly Stopwatch _testStopwatch = new();
    
    [SetUp]
    public void TestSetup()
    {
        _purchasePage = new PurchasePage(Driver);
        ExtentTestManager.LogInfo($"Test setup completed in {_testStopwatch.ElapsedMilliseconds}ms");
    }

    [Test]
    [Description("Verify that a user can successfully add a product to the cart")]
    [Category("Shopping Cart")]
    [Category("Smoke")]
    [Retry(2)]  
    public void AddProductToCart()
    {
        const string expectedUrl = "https://store.progress.com/configure-purchase";
        const int expectedQuantity = 1;
        const PurchasePage.ProductBundle selectedBundle = PurchasePage.ProductBundle.DevCraftComplete;
       
        try
        {
            ExtentTestManager.LogInfo("Navigate to Product Page");
            _purchasePage!.NavigateTo();
            
            ExtentTestManager.LogInfo("Accept Cookies");
            _purchasePage.AcceptCookies();
            
            ExtentTestManager.LogInfo($"Saving initial price for {selectedBundle} bundle");
            _purchasePage.SaveBundlePrice(selectedBundle);
            
            ExtentTestManager.LogInfo($"Adding {selectedBundle} bundle to cart");
            _purchasePage.SelectBundle(selectedBundle);
            
            ExtentTestManager.LogInfo("Verifying cart state");
            Assert.Multiple(() =>
            {
                Assert.That(Driver.Url, Does.Contain(expectedUrl), 
                    $"Expected URL to contain '{expectedUrl}', but was '{Driver.Url}'");
        
                Assert.That(_purchasePage.VerifyPriceMatchesSaved(tolerance: 0.01M), Is.True,
                    $"Price mismatch - Saved: {_purchasePage.GetSavedPrice():C}, Current: {_purchasePage.GetCurrentPrice():C}");
        
                Assert.That(_purchasePage.GetQuantity(), Is.EqualTo(expectedQuantity), 
                    $"Quantity should be {expectedQuantity}");
            });
        }
        catch (Exception ex)
        {
            ExtentTestManager.LogFail($"Test failed: {ex.Message}");
            ExtentTestManager.LogScreenshot(Driver, "Failure state");
            throw;
        }
    }

    [Test]
    [Description("Verify that maintenance & support quantity can be updated")]
    [Category("Shopping Cart")]
    [Retry(2)]
    public void VerifyLicenseQuantityPriceUpdate()
    {
        try
        {
            // Test steps:
            // 1. Navigate to purchase page
            // 2. Add product to cart
            // 3. Change M&S quantity
            // 4. Verify quantity updated
            // 5. Verify price updated accordingly
        }
        catch (Exception ex)
        {
            ExtentTestManager.LogFail($"Test failed after {_testStopwatch.ElapsedMilliseconds}ms", ex);
            ExtentTestManager.LogScreenshot(Driver, "Failure state");
            throw;
        }
    }

    [Test]
    [Description("Verify cart total price updates correctly when quantities change")]
    [Category("Shopping Cart")]
    [Retry(2)]
    public void VerifyMaintenanceSupportSelection()
    {
        try
        {
            // Test steps:
            // 1. Navigate to purchase page
            // 2. Add product to cart
            // 3. Record initial price
            // 4. Update license quantity
            // 5. Update M&S quantity
            // 6. Verify total price calculation is correct
        }
        catch (Exception ex)
        {
            ExtentTestManager.LogFail($"Test failed after {_testStopwatch.ElapsedMilliseconds}ms", ex);
            ExtentTestManager.LogScreenshot(Driver, "Failure state");
            throw;
        }
    }

    [Test]
    [Description("Verify discounts are applied correctly to cart total")]
    [Category("Shopping Cart")]
    [TestCase("VALIDCODE", true, Description = "Valid coupon code test")]
    [TestCase("INVALID", false, Description = "Invalid coupon code test")]
    [Retry(2)]
    public void VerifyCouponApplication(string couponCode, bool shouldBeValid)
    {
        try
        {
            // Test steps:
            // 1. Navigate to purchase page
            // 2. Add product to cart
            // 3. Record initial price
            // 4. Apply discount/coupon
            // 5. Verify discount amount is correct
            // 6. Verify final price reflects discount
        }
        catch (Exception ex)
        {
            ExtentTestManager.LogFail($"Test failed after {_testStopwatch.ElapsedMilliseconds}ms", ex);
            ExtentTestManager.LogScreenshot(Driver, "Failure state");
            throw;
        }
    }
    
    [Test]
    [Description("Verify multiple products can be added to cart")]
    [Category("Shopping Cart")]
    public void UpdateMaintenanceAndSupportQuantity()
    {
        try
        {
            // Test steps:
            // 1. Navigate to purchase page
            // 2. Add first product
            // 3. Add second product
            // 4. Verify both products are in cart
            // 5. Verify total price is sum of products
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
    }

    [OneTimeTearDown]
    public void ClassCleanup()
    {
        ExtentTestManager.LogInfo("Completed Cart Tests Suite");
    }
}