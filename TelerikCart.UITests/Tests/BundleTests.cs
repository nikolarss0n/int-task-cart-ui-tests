using NUnit.Framework;
using TelerikCart.UITests.Core.Base;
using TelerikCart.UITests.Core.Reporting;
using TelerikCart.UITests.Pages;
using System.Diagnostics;
using static TelerikCart.UITests.Pages.PurchasePage;
using static TelerikCart.UITests.Pages.CartPage;

namespace TelerikCart.UITests.Tests;

[TestFixture]
[Category("Bundles")]
[Parallelizable(ParallelScope.Self)]  
public class BundleTests : BaseTest
{
    private CommonComponents? _commonComponents;
    private PurchasePage? _purchasePage;
    private CartPage? _cartPage;
    private readonly Stopwatch _testStopwatch = new();
    
    [SetUp]
    public void TestSetup()
    {
        _commonComponents = new CommonComponents(Driver);
        _purchasePage = new PurchasePage(Driver);
        _cartPage = new CartPage(Driver);
        
        ExtentTestManager.LogInfo($"Test setup completed in {_testStopwatch.ElapsedMilliseconds}ms");
    }
    
    [Test]
    [Description("Verify that a user can successfully add a bundle to the cart")]
    [Category("Bundle Purchase")]
    [Category("Smoke")]
    [Retry(2)]  
    public void AddBundleToCart()
    {
        const string expectedUrl = "https://store.progress.com/your-order";
        const int expectedQuantity = 1;
        const decimal tolerance = 0.01M;
        const ProductBundle selectedBundle = ProductBundle.DevCraftComplete;
        
        // Arrange & initial price capture
        _purchasePage!.NavigateTo();
        _commonComponents!.AcceptCookies();
        var expectedPrice = _purchasePage.SaveBundlePrice(selectedBundle);
    
        // Act
        _purchasePage.SelectBundle(selectedBundle);
        
        
        Assert.That(_purchasePage.VerifyNavigation(expectedUrl), Is.True, 
            $"Failed to navigate to cart page. Current URL: {Driver.Url}");
        
        var actualPrice = _cartPage!.GetCurrentPrice();
    
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(Driver.Url, Does.Contain(expectedUrl), 
                $"Expected URL to contain '{expectedUrl}', but was '{Driver.Url}'");
            Assert.That(Math.Abs(actualPrice - expectedPrice), Is.LessThanOrEqualTo(tolerance),
                $"Price mismatch - Expected: {expectedPrice:C}, Actual: {actualPrice:C}");
            Assert.That(_cartPage.GetTotalQuantity(), Is.EqualTo(expectedQuantity), 
                $"Quantity should be {expectedQuantity}");
        });

        ExtentTestManager.LogInfo($"Price verification successful - Expected: {expectedPrice:C}, Actual: {actualPrice:C}");
    }

    [Test]
    [Description("Verify support plan options and features")]
    [Category("Bundle Features")]
    [TestCase(ProductBundle.DevCraftUI, "Lite", "72h", "10")]
    [TestCase(ProductBundle.DevCraftComplete, "Priority", "24h", "Unlimited")]
    [TestCase(ProductBundle.DevCraftUltimate, "Ultimate", "Everything in Priority Support", "Phone support\nRemote web assistance\nTicket pre-screening\nIssue escalation")]
    public void VerifySupportPlanOptions(ProductBundle bundle, string plan, 
        string responseTime, string incidents)
    {
            _purchasePage!.NavigateTo();
            _commonComponents!.AcceptCookies();
            
            var actualText = _purchasePage.GetSupportText(bundle);
            ExtentTestManager.LogInfo($"Testing Bundle: {bundle}");
            ExtentTestManager.LogInfo($"Found text: '{actualText}'");
            
            ExtentTestManager.LogInfo($"-=============== BUNDLE: {bundle}");

            Assert.Multiple(() =>
            {
                Assert.That(_purchasePage.VerifySupportLevel(bundle, plan), Is.True,
                    $"Bundle {bundle} should include '{plan}' support. Actual text: '{actualText}'");
            
                Assert.That(_purchasePage.VerifyResponseTime(bundle, responseTime), Is.True,
                    $"Bundle {bundle} response time should be '{responseTime}'. Actual text: '{actualText}'");
                foreach (var line in incidents.Split('\n'))
                {
                    Assert.That(_purchasePage.VerifyIncidentLimit(bundle, line), Is.True,
                        $"Bundle {bundle} should have {line} support incidents. Actual text: '{actualText}'");
                }
            });
    }
    
    [Test]
    [Description("Verify Maintenance and Support discounts are applied correctly to cart total")]
    [TestCase(PeriodOption.PlusOneYear, 2505.55, Description = "Extended +1 year support")]
    [TestCase(PeriodOption.PlusFourYears, 4619.56, Description = "Extended +4 years support")]
    public void VerifyMaintenanceAndSupportDiscounts(PeriodOption period, decimal expectedTotal)
    {
        const string expectedUrl = "https://store.progress.com/your-order";
        const ProductBundle selectedBundle = ProductBundle.DevCraftComplete;
        const decimal tolerance = 0.01M;
        
        // Arrange
        _purchasePage!.NavigateTo();
        _commonComponents!.AcceptCookies();
        _purchasePage.SaveBundlePrice(selectedBundle);
    
        // Act
        _purchasePage.SelectBundle(selectedBundle);
    
        Assert.That(_purchasePage.VerifyNavigation(expectedUrl), Is.True, 
            $"Failed to navigate to cart page. Current URL: {Driver.Url}");
    
        _cartPage.SelectPeriod(period);
        var actualTotal = _cartPage.GetTotalPrice();
    
        // Assert
        Assert.That(Math.Abs(actualTotal - expectedTotal), Is.LessThanOrEqualTo(tolerance),
            $"Price mismatch for {period} - Expected: {expectedTotal:C}, Actual: {actualTotal:C}");

        ExtentTestManager.LogInfo(
            $"Successfully verified total price for {period}:\n" +
            $"Expected: {expectedTotal:C}\n" +
            $"Actual: {actualTotal:C}");
    }
    
    [Test]
    [Description("Verify multiple bundles can be added to cart")]
    [Category("Bundle Purchase")]
    [Category("Smoke")]
    [Retry(2)]
    public void AddMultipleBundlesToCart()
    {
        const string expectedUrl = "https://store.progress.com/your-order";
        const int expectedQuantity = 1;
        const decimal tolerance = 0.01M;
        const ProductBundle firstBundle = ProductBundle.DevCraftComplete;
        const ProductBundle secondBundle = ProductBundle.DevCraftUI;
       
        try
        {
            // Arrange & initial price capture
            _purchasePage!.NavigateTo();
            _commonComponents!.AcceptCookies();
            var firstBundlePrice = _purchasePage.SaveBundlePrice(firstBundle);
        
            // Act
            _purchasePage.SelectBundle(firstBundle);
            var actualPrice = _cartPage!.GetCurrentPrice();
            
            // First Bundle Assert
            Assert.Multiple(() =>
            {
                Assert.That(Driver.Url, Does.Contain(expectedUrl), 
                    $"Expected URL to contain '{expectedUrl}', but was '{Driver.Url}'");
                Assert.That(Math.Abs(actualPrice - firstBundlePrice), Is.LessThanOrEqualTo(tolerance),
                    $"Price mismatch - Expected: {firstBundlePrice:C}, Actual: {actualPrice:C}");
                Assert.That(_cartPage.GetTotalQuantity(), Is.EqualTo(expectedQuantity), 
                    $"Quantity should be {expectedQuantity}");
            });
    
            // Add second bundle
            _purchasePage.NavigateTo();
            var secondBundlePrice = _purchasePage.SaveBundlePrice(secondBundle);
            _purchasePage.SelectBundle(secondBundle);
    
            // Verify both bundles are in cart with correct total
            var expectedTotalPrice = firstBundlePrice + secondBundlePrice;
            Assert.Multiple(() =>
            {
                Assert.That(_cartPage.GetTotalQuantity(), Is.EqualTo(2),
                    "Cart should contain two items");
                Assert.That(_cartPage.GetTotalPrice(), Is.EqualTo(expectedTotalPrice).Within(0.01M),
                    $"Total price should be sum of both bundles: {expectedTotalPrice:C}");
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
    [Description("Verify volume discount application")]
    [Category("Bundle Pricing")]
    [TestCase(2, 0.05, Description = "Small team discount (5%)")]
    [TestCase(5, 0.10, Description = "Medium team discount (10%)")]
    public void VerifyVolumeDiscounts(int licenseQuantity, decimal expectedDiscount)
    {
        // try
        // {
        //     _purchasePage!.NavigateTo();
        //     _commonComponents.AcceptCookies();
        //     
        //     var bundle = PurchasePage.ProductBundle.DevCraftComplete;
        //     var basePrice = _purchasePage.SaveBundlePrice(bundle);
        //     _purchasePage.SelectBundle(bundle);
        //     _cartPage.UpdateQuantity(licenseQuantity);
        //
        //     var expectedTotal = basePrice * licenseQuantity * (1 - expectedDiscount);
        //     var actualTotal = _cartPage.GetTotalPrice();
        //
        //     Assert.Multiple(() =>
        //     {
        //         Assert.That(actualTotal, Is.EqualTo(expectedTotal).Within(0.01m),
        //             $"Total price should reflect {expectedDiscount:P0} volume discount");
        //         Assert.That(_cartPage.GetQuantity(), Is.EqualTo(licenseQuantity),
        //             "Quantity should be updated correctly");
        //     });
        // }
        // catch (Exception ex)
        // {
        //     ExtentTestManager.LogFail($"Test failed: {ex.Message}");
        //     ExtentTestManager.LogScreenshot(Driver, "Failure state");
        //     throw;
        // }
    }

    [Test]
    [Description("Verify multi-year subscription discounts")]
    [Category("Bundle Pricing")]
    [TestCase(2, 0.10, Description = "Two-year subscription (10% off second year)")]
    [TestCase(3, 0.15, Description = "Three-year subscription (15% off years 2-3)")]
    public void VerifyMultiYearDiscounts(int years, decimal expectedDiscount)
    {
        // try
        // {
        //     _purchasePage!.NavigateTo();
        //     _commonComponents.AcceptCookies();
        //     
        //     var bundle = PurchasePage.ProductBundle.DevCraftComplete;
        //     var basePrice = _purchasePage.SaveBundlePrice(bundle);
        //     _purchasePage.SelectBundle(bundle);
        //     _purchasePage.SelectSubscriptionYears(years);
        //
        //     // Calculate expected total: First year full price + subsequent years at discount
        //     var expectedTotal = basePrice + (basePrice * (years - 1) * (1 - expectedDiscount));
        //     var actualTotal = _purchasePage.GetTotalPrice();
        //
        //     Assert.Multiple(() =>
        //     {
        //         Assert.That(actualTotal, Is.EqualTo(expectedTotal).Within(0.01m),
        //             $"Total price should reflect multi-year discount");
        //         Assert.That(_purchasePage.VerifySubscriptionTerm($"{years} year"), Is.True,
        //             $"Should show {years}-year subscription term");
        //     });
        // }
        // catch (Exception ex)
        // {
        //     ExtentTestManager.LogFail($"Test failed: {ex.Message}");
        //     ExtentTestManager.LogScreenshot(Driver, "Failure state");
        //     throw;
        // }
    }

    [Test]
    [Description("Remove product from cart")]
    [Category("Bundle Purchase")]
    [Category("Smoke")]
    public void RemoveProduct()
    {
  
    }
    
    [Test]
    [Description("Add UI Accelerator to bundle")]
    [Category("Bundle Features")]
    public void AddUIAccelerator()
    {
  
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