using NUnit.Framework;
using TelerikCart.UITests.Core.Base;
using TelerikCart.UITests.Core.Reporting;
using TelerikCart.UITests.Pages;
using System.Diagnostics;
using static TelerikCart.UITests.Pages.PurchasePage;
using static TelerikCart.UITests.Pages.CartPage;
using YourProject.UITests.Core.Reporting.Documentation.Extensions;

namespace TelerikCart.UITests.Tests
{
    /// <summary>
    /// Contains test cases for validating the purchase process of product bundles,
    /// including adding bundles to the cart, verifying prices, support plans,
    /// discounts, and cart functionalities.
    /// </summary>
    [TestFixture]
    [Category("Bundles")]
    public class BundleTests : BaseTest
    {
        private CommonComponents _commonComponents = null!;
        private PurchasePage _purchasePage = null!;
        private CartPage _cartPage = null!;
        private readonly Stopwatch _testStopwatch = new();
        private const decimal PriceTolerance = 0.01M;
        private const string CartUrl = "https://store.progress.com/your-order";

        /// <summary>
        /// Sets up the necessary page objects before each test.
        /// </summary>
        [SetUp]
        public void TestSetup()
        {
            _commonComponents = new CommonComponents(Driver);
            _purchasePage = new PurchasePage(Driver);
            _cartPage = new CartPage(Driver);
        }

        /// <summary>
        /// Validates that adding a single bundle to the cart reflects the correct price and quantity.
        /// </summary>
        [Test]
        [Category("Bundle Purchase")]
        [Category("Smoke")]
        [Retry(2)]
        [Description("Basic Bundle Purchase Flow")]
        public void AddBundleToCart()
        {
            // Document the test case details
            ExtentTestManager.GetTest().DocumentTest(
                objective: "Validate that adding a single bundle to the cart reflects the correct price and quantity",
                steps: new[]
                {
                    "Navigate to purchase page and accept cookies",
                    "Record expected price of DevCraft Complete bundle",
                    "Add DevCraft Complete bundle to cart",
                    "Verify navigation to cart page",
                    "Retrieve actual price from cart",
                    "Verify actual price matches expected",
                    "Verify cart has one item"
                },
                criteria: new[]
                {
                    "Navigation to cart page is successful",
                    "Actual price matches expected price within tolerance",
                    "Cart has exactly one item"
                }
            );

            const ProductBundle selectedBundle = ProductBundle.DevCraftComplete;

            // Navigate to the purchase page and accept cookies
            _purchasePage.NavigateTo();
            _commonComponents.AcceptCookies();

            // Save the expected price of the selected bundle
            var expectedPrice = _purchasePage.SaveBundlePrice(selectedBundle);

            // Add the bundle to the cart
            _purchasePage.SelectBundle(selectedBundle);

            // Verify navigation to the cart page
            Assert.That(_purchasePage.VerifyNavigation(CartUrl), Is.True,
                $"Failed to navigate to cart page. Current URL: {Driver.Url}");

            // Retrieve the actual price from the cart
            var actualPrice = _cartPage.GetCurrentPrice();

            // Perform assertions on URL, price, and cart quantity
            Assert.Multiple(() =>
            {
                Assert.That(Driver.Url, Does.Contain(CartUrl), "Cart URL verification failed");
                Assert.That(Math.Abs(actualPrice - expectedPrice), Is.LessThanOrEqualTo(PriceTolerance),
                    $"Price mismatch: Expected {expectedPrice:C}, Actual {actualPrice:C}");
                Assert.That(_cartPage.GetTotalQuantity(), Is.EqualTo(1),
                    "Cart should contain exactly one item");
            });

            // Log the results of the test
            ExtentTestManager.GetTest().LogResults(true, new()
            {
                ["Expected Price"] = expectedPrice.ToString("C"),
                ["Actual Price"] = actualPrice.ToString("C"),
            });
        }

        /// <summary>
        /// Validates that adding multiple bundles to the cart reflects the correct combined price and quantity.
        /// </summary>
        [Test]
        [Description("Multiple bundle purchase verification")]
        [Category("Bundle Purchase")]
        [Retry(2)]
        public void AddMultipleBundlesToCart()
        {
            // Document the test case details
            ExtentTestManager.GetTest().DocumentTest(
                objective: "Validate that adding multiple bundles to the cart reflects the correct combined price and quantity",
                steps: new[]
                {
                    "Navigate to purchase page and accept cookies",
                    "Record price of DevCraft Complete bundle",
                    "Add DevCraft Complete bundle to cart",
                    "Verify cart page shows correct price and quantity for first bundle",
                    "Navigate to purchase page again",
                    "Record price of DevCraft UI bundle",
                    "Add DevCraft UI bundle to cart",
                    "Verify cart shows correct combined price and quantity"
                },
                criteria: new[]
                {
                    "Both bundles added successfully",
                    "Total price matches sum of bundle prices within tolerance",
                    "Cart shows total quantity of 2"
                }
            );

            const ProductBundle firstBundle = ProductBundle.DevCraftComplete;
            const ProductBundle secondBundle = ProductBundle.DevCraftUI;

            // Navigate to the purchase page and accept cookies
            _purchasePage.NavigateTo();
            _commonComponents.AcceptCookies();

            // Save and add the first bundle
            var firstBundlePrice = _purchasePage.SaveBundlePrice(firstBundle);
            _purchasePage.SelectBundle(firstBundle);
            Assert.That(_purchasePage.VerifyNavigation(CartUrl), Is.True,
                $"Failed to navigate to cart page. Current URL: {Driver.Url}");

            // Verify the price and quantity after adding the first bundle
            var actualPrice = _cartPage.GetCurrentPrice();
            Assert.Multiple(() =>
            {
                Assert.That(Driver.Url, Does.Contain(CartUrl), "Cart URL verification failed");
                Assert.That(Math.Abs(actualPrice - firstBundlePrice), Is.LessThanOrEqualTo(PriceTolerance),
                    $"First bundle price mismatch: Expected {firstBundlePrice:C}, Actual {actualPrice:C}");
                Assert.That(_cartPage.GetTotalQuantity(), Is.EqualTo(1),
                    "Cart should contain one item after first bundle");
            });

            // Navigate back to the purchase page to add the second bundle
            _purchasePage.NavigateTo();
            var secondBundlePrice = _purchasePage.SaveBundlePrice(secondBundle);
            _purchasePage.SelectBundle(secondBundle);

            // Calculate the expected total price
            var expectedTotal = firstBundlePrice + secondBundlePrice;

            // Verify the total price and quantity after adding the second bundle
            Assert.Multiple(() =>
            {
                Assert.That(_cartPage.GetTotalQuantity(), Is.EqualTo(2),
                    "Cart should contain two items after second bundle");
                Assert.That(_cartPage.GetTotalPrice(), Is.EqualTo(expectedTotal).Within(PriceTolerance),
                    $"Combined price mismatch: Expected {expectedTotal:C}, Actual {_cartPage.GetTotalPrice():C}");
            });

            // Log the results of the test
            ExtentTestManager.GetTest().LogResults(true, new()
            {
                ["First Bundle Price"] = firstBundlePrice.ToString("C"),
                ["Second Bundle Price"] = secondBundlePrice.ToString("C"),
                ["Total Expected"] = expectedTotal.ToString("C"),
                ["Total Actual"] = _cartPage.GetTotalPrice().ToString("C"),
            });
        }

        /// <summary>
        /// Verifies that the product bundles include the correct support plan options.
        /// </summary>
        /// <param name="bundle">The product bundle to test.</param>
        /// <param name="plan">The expected support plan.</param>
        /// <param name="responseTime">The expected response time.</param>
        /// <param name="incidents">The expected incident coverage or features.</param>
        [Test]
        [Description("Verify Support Plan for product bundles")]
        [Category("Bundle Features")]
        [TestCase(
            ProductBundle.DevCraftUI,
            "Lite",
            "72h",
            "10",
            Description = "Lite Support Plan")]
        [TestCase(
            ProductBundle.DevCraftComplete,
            "Priority",
            "24h",
            "Unlimited",
            Description = "Priority Support Plan")]
        [TestCase(
            ProductBundle.DevCraftUltimate,
            "Ultimate",
            "Everything in Priority Support",
            """
            Phone support
            Remote web assistance
            Ticket pre-screening
            Issue escalation
            """,
            Description = "Ultimate Support Plan"
        )]
        public void VerifySupportPlanOptions(
            ProductBundle bundle,
            string plan,
            string responseTime,
            string incidents)
        {
            // Document the test case details
            ExtentTestManager.GetTest().DocumentTest(
                objective: $"Validate that {bundle} includes the correct {plan} support features",
                steps: new[]
                {
                    "Navigate to purchase page and accept cookies",
                    $"Retrieve support information for {bundle}",
                    $"Verify support level is '{plan}'",
                    $"Verify response time is '{responseTime}'",
                    "Verify incident limits and features match expected"
                },
                criteria: new[]
                {
                    $"Support level matches '{plan}'",
                    $"Response time matches '{responseTime}'",
                    "Incident limits and features are as expected"
                }
            );

            // Navigate to the purchase page and accept cookies
            _purchasePage.NavigateTo();
            _commonComponents.AcceptCookies();

            // Get the support text for the specified bundle
            var actualText = _purchasePage.GetSupportText(bundle);

            // Perform assertions on support level, response time, and incident features
            Assert.Multiple(() =>
            {
                Assert.That(_purchasePage.VerifySupportLevel(bundle, plan), Is.True,
                    $"Support level mismatch for {bundle}");
                Assert.That(_purchasePage.VerifyResponseTime(bundle, responseTime), Is.True,
                    $"Response time mismatch for {bundle}");

                // Check each expected feature or incident limit
                foreach (var feature in incidents.Split('\n', StringSplitOptions.RemoveEmptyEntries))
                {
                    Assert.That(_purchasePage.VerifyIncidentLimit(bundle, feature), Is.True,
                        $"Missing support feature for {bundle}: {feature}");
                }
            });

            // Log the results of the test
            ExtentTestManager.GetTest().LogResults(true, new()
            {
                ["Bundle"] = bundle.ToString(),
                ["Support Level"] = plan,
                ["Response Time"] = responseTime,
                ["Incident Coverage"] = incidents.Replace("\n", ", "),
                ["Actual Text"] = actualText
            });
        }

        /// <summary>
        /// Verifies that maintenance and support plan discounts are applied correctly for product bundles.
        /// </summary>
        /// <param name="period">The support period option selected.</param>
        /// <param name="expectedTotal">The expected total price after discount.</param>
        [Test]
        [Description("Verify Maintenance & Support Plan discounts for product bundles")]
        [Category("Bundle Purchase")]
        [TestCase(
            PeriodOption.PlusOneYear,
            2505.55,
            Description = "+1 Year Support"
        )]
        [TestCase(
            PeriodOption.PlusFourYears,
            4619.56,
            Description = "+4 Years Support"
        )]
        public void VerifyMaintenanceAndSupportDiscounts(PeriodOption period, decimal expectedTotal)
        {
            // Document the test case details
            ExtentTestManager.GetTest().DocumentTest(
                objective: $"Validate that selecting '{period}' applies the correct support price and discount",
                steps: new[]
                {
                    "Navigate to purchase page and accept cookies",
                    "Record base price of DevCraft Complete bundle",
                    "Add DevCraft Complete bundle to cart",
                    $"Select support period: '{period}'",
                    "Verify total price matches expected after discount"
                },
                criteria: new[]
                {
                    $"Correct discount applied for selected support period '{period}'",
                    "Total price matches expected total within tolerance"
                }
            );

            const ProductBundle selectedBundle = ProductBundle.DevCraftComplete;

            // Navigate to the purchase page and accept cookies
            _purchasePage.NavigateTo();
            _commonComponents.AcceptCookies();

            // Save the base price of the selected bundle
            var basePrice = _purchasePage.SaveBundlePrice(selectedBundle);

            // Add the bundle to the cart
            _purchasePage.SelectBundle(selectedBundle);
            Assert.That(_purchasePage.VerifyNavigation(CartUrl), Is.True,
                $"Failed to navigate to cart page. Current URL: {Driver.Url}");
            _commonComponents.AcceptCookies();

            // Select the desired support period
            _cartPage.SelectPeriod(period);

            // Get the actual total price after applying the discount
            var actualTotal = _cartPage.GetTotalPrice();

            // Verify that the actual total matches the expected total within tolerance
            Assert.That(Math.Abs(actualTotal - expectedTotal), Is.LessThanOrEqualTo(PriceTolerance),
                $"Price mismatch for {period}");

            // Log the results of the test
            ExtentTestManager.GetTest().LogResults(true, new()
            {
                ["Support Period"] = period.ToString(),
                ["Base Price"] = basePrice.ToString("C"),
                ["Expected Total"] = expectedTotal.ToString("C"),
                ["Actual Total"] = actualTotal.ToString("C"),
                ["Price Difference"] = Math.Abs(actualTotal - expectedTotal).ToString("C")
            });
        }

        /// <summary>
        /// Verifies that volume discounts are applied correctly to the cart total based on quantity.
        /// </summary>
        /// <param name="quantity">The number of bundles to purchase.</param>
        /// <param name="expectedTotal">The expected total price after discount.</param>
        [Test]
        [Description("Verify volume discounts are applied correctly to cart total")]
        [Category("Bundle Purchase")]
        [TestCase(2, 3228.10, Description = "Small team price (5% discount)")]
        [TestCase(5, 8070.25, Description = "Medium team price (10% discount)")]
        public void VerifyVolumeDiscounts(int quantity, decimal expectedTotal)
        {
            // Document the test case details
            ExtentTestManager.GetTest().DocumentTest(
                objective: "Validate that volume discounts are correctly applied based on quantity",
                steps: new[]
                {
                    "Navigate to purchase page and accept cookies",
                    "Record base price of DevCraft Complete bundle",
                    "Add DevCraft Complete bundle to cart",
                    $"Update quantity to {quantity}",
                    "Verify total price matches expected after volume discount"
                },
                criteria: new[]
                {
                    $"Correct volume discount applied for {quantity} licenses",
                    "Total price matches expected total within tolerance",
                    "Cart quantity accurately reflects updated quantity"
                }
            );

            const ProductBundle selectedBundle = ProductBundle.DevCraftComplete;

            // Navigate to the purchase page and accept cookies
            _purchasePage.NavigateTo();
            _commonComponents.AcceptCookies();

            // Save the base price of the selected bundle
            var basePrice = _purchasePage.SaveBundlePrice(selectedBundle);

            // Add the bundle to the cart
            _purchasePage.SelectBundle(selectedBundle);
            Assert.That(_purchasePage.VerifyNavigation(CartUrl), Is.True,
                $"Failed to navigate to cart page. Current URL: {Driver.Url}");
            _commonComponents.AcceptCookies();

            // Update the quantity in the cart
            _cartPage.UpdateQuantity(quantity);

            // Get the actual total price after applying the volume discount
            var actualTotal = _cartPage.GetTotalPrice();

            // Verify that the actual total matches the expected total within tolerance
            Assert.That(Math.Abs(actualTotal - expectedTotal), Is.LessThanOrEqualTo(PriceTolerance),
                $"Price mismatch for {quantity} licenses");

            // Log the results of the test
            ExtentTestManager.GetTest().LogResults(true, new()
            {
                ["Quantity"] = quantity.ToString(),
                ["Base Price"] = basePrice.ToString("C"),
                ["Expected Total"] = expectedTotal.ToString("C"),
                ["Actual Total"] = actualTotal.ToString("C"),
                ["Per Unit Price"] = (actualTotal / quantity).ToString("C")
            });
        }

        /// <summary>
        /// Verifies that a product can be removed from the cart successfully.
        /// </summary>
        [Test]
        [Description("Remove product from cart")]
        [Category("Bundle Purchase")]
        public void RemoveProduct()
        {
            // Document the test case details
            ExtentTestManager.GetTest().DocumentTest(
                objective: "Validate that removing a product from the cart functions correctly",
                steps: new[]
                {
                    "Navigate to purchase page and accept cookies",
                    "Add DevCraft Complete bundle to cart",
                    "Verify navigation to cart page",
                    "Remove product from cart",
                    "Verify cart is empty"
                },
                criteria: new[]
                {
                    "Product is successfully removed from cart",
                    "Empty cart message is displayed",
                    "Cart shows zero items"
                }
            );

            const ProductBundle selectedBundle = ProductBundle.DevCraftComplete;

            // Navigate to the purchase page and accept cookies
            _purchasePage.NavigateTo();
            _commonComponents.AcceptCookies();

            // Add the bundle to the cart
            _purchasePage.SelectBundle(selectedBundle);
            Assert.That(_purchasePage.VerifyNavigation(CartUrl), Is.True,
                $"Failed to navigate to cart page. Current URL: {Driver.Url}");

            // Remove the product from the cart
            _cartPage.RemoveProduct();

            // Verify that the cart is empty
            const string expectedMessage = "Your shopping cart is empty!";
            string actualMessage = _cartPage.GetEmptyCartMessage();
            Assert.That(_cartPage.VerifyEmptyCartMessage(expectedMessage), Is.True,
                "Empty cart verification failed");

            // Log the results of the test
            ExtentTestManager.GetTest().LogResults(true, new()
            {
                ["Initial Bundle"] = selectedBundle.ToString(),
                ["Empty Cart Message"] = actualMessage,
            });
        }

        /// <summary>
        /// Cleans up after each test execution.
        /// </summary>
        [TearDown]
        public void TestCleanup()
        {
            try
            {
                if (_testStopwatch.IsRunning)
                {
                    ExtentTestManager.LogInfo($"""
                        Test Execution Summary:
                        • Duration: {_testStopwatch.ElapsedMilliseconds}ms
                        • Status: Completed
                        • Components: {"Initialized"}
                        """);
                }
            }
            catch (Exception ex)
            {
                ExtentTestManager.LogInfo($"""
                    Cleanup Operation Failed:
                    • Error: {ex.Message}
                    • Stack: {ex.StackTrace}
                    """);
            }
            finally
            {
                _testStopwatch.Reset();
            }
        }

        /// <summary>
        /// Performs any necessary cleanup after all tests in this class have run.
        /// </summary>
        [OneTimeTearDown]
        public void ClassCleanup()
        {
            ExtentTestManager.LogInfo("""
                =================================
                Bundle Test Suite Execution Report
                =================================
                • All test cases completed
                • Resources released
                • Report generation triggered
                =================================
                """);
        }
    }
}
