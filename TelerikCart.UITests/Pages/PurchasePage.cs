using OpenQA.Selenium;
using TelerikCart.UITests.Core.Helpers;
using NUnit.Framework;
using TelerikCart.UITests.Core.Base;
using TelerikCart.UITests.Core.Reporting;

namespace TelerikCart.UITests.Pages
{
    /// <summary>
    /// Provides methods for interacting with the purchase page, including navigation,
    /// selecting product bundles, retrieving bundle prices and support information,
    /// verifying support levels and response times, and navigating to the shopping cart.
    /// </summary>
    public class PurchasePage : BasePage
    {
        private const string PageUrl = "https://www.telerik.com/purchase";
        private string? _savedBundlePrice;
        private ProductBundle? _selectedBundle;

        // Locators
        private readonly By _cartButton = By.CssSelector("[href*='/shopping-cart']");

        /// <summary>
        /// Enum representing the available product bundles.
        /// </summary>
        public enum ProductBundle
        {
            DevCraftUI,
            DevCraftComplete,
            DevCraftUltimate
        }

        private readonly Dictionary<ProductBundle, By> _bundleBuyButtons = new()
        {
            { ProductBundle.DevCraftUI, By.CssSelector("tr.Pricings-button th.UI a.Btn--prim4") },
            { ProductBundle.DevCraftComplete, By.CssSelector("tr.Pricings-button th.Complete a.Btn--prim4") },
            { ProductBundle.DevCraftUltimate, By.CssSelector("tr.Pricings-button th.Ultimate a.Btn--prim4") }
        };

        private readonly Dictionary<ProductBundle, By> _bundlePriceTags = new()
        {
            { ProductBundle.DevCraftUI, By.CssSelector("tr.Pricings-value th.UI span[data-price-without-addon]") },
            { ProductBundle.DevCraftComplete, By.CssSelector("tr.Pricings-value th.Complete span[data-price-without-addon]") },
            { ProductBundle.DevCraftUltimate, By.CssSelector("tr.Pricings-value th.Ultimate span[data-price-without-addon]") }
        };

        private readonly Dictionary<ProductBundle, By> _bundleSupportLocators = new()
        {
            { ProductBundle.DevCraftUI, By.CssSelector("tr.Pricings-support th.UI .InfoBox") },
            { ProductBundle.DevCraftComplete, By.CssSelector("tr.Pricings-support th.Complete .InfoBox") },
            { ProductBundle.DevCraftUltimate, By.CssSelector("tr.Pricings-support th.Ultimate .InfoBox") }
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="PurchasePage"/> class.
        /// </summary>
        /// <param name="driver">The WebDriver instance to interact with the browser.</param>
        public PurchasePage(IWebDriver driver) : base(driver, "Purchase Page") { }

        /// <summary>
        /// Navigates to the purchase page and waits for the page to load.
        /// </summary>
        public void NavigateTo()
        {
            NavigateToUrl(PageUrl);
            WaitForPageLoad();
        }

        /// <summary>
        /// Saves the price of the specified product bundle for later verification.
        /// </summary>
        /// <param name="bundle">The product bundle whose price to save.</param>
        /// <returns>The price of the bundle as a decimal.</returns>
        /// <exception cref="NoSuchElementException">Thrown when the price element is not found or empty.</exception>
        public decimal SaveBundlePrice(ProductBundle bundle)
        {
            try
            {
                Log("Saving bundle price", bundle.ToString());
                var priceElement = WaitAndFindElement(_bundlePriceTags[bundle], $"{bundle} price");
                var priceText = priceElement.GetAttribute("data-price-without-addon");

                if (string.IsNullOrEmpty(priceText))
                {
                    throw new NoSuchElementException($"Price data attribute is empty for {bundle} bundle");
                }

                _savedBundlePrice = PriceUtils.NormalizePriceString(priceText);
                var price = PriceUtils.ParsePrice(_savedBundlePrice);
                LogSuccess("Saved bundle price", $"{bundle} - {price:C}");
                return price;
            }
            catch (Exception ex)
            {
                LogError($"Failed to save {bundle} price", ex);
                throw;
            }
        }

        /// <summary>
        /// Selects the specified product bundle by clicking its "Buy Now" button.
        /// </summary>
        /// <param name="bundle">The product bundle to select.</param>
        /// <exception cref="Exception">Thrown when the bundle cannot be selected.</exception>
        public void SelectBundle(ProductBundle bundle)
        {
            try
            {
                _selectedBundle = bundle;
                Log("Selecting bundle", bundle.ToString());
                ScrollToBundleSection();
                WaitAndClick(_bundleBuyButtons[bundle], $"Buy Now button for {bundle}");
                WaitForNetworkIdle();
                LogSuccess("Selected bundle", bundle.ToString());
            }
            catch (Exception ex)
            {
                LogError($"Failed to select {bundle}", ex);
                throw;
            }
        }

        /// <summary>
        /// Navigates to the shopping cart page from the purchase page.
        /// </summary>
        /// <exception cref="Exception">Thrown when navigation to the cart fails.</exception>
        public void GoToCart()
        {
            try
            {
                WaitAndClick(_cartButton, "Go to Cart button");
                WaitForNetworkIdle();
                TakeScreenshot("Navigated to cart page");
                LogSuccess("Navigated to cart");
            }
            catch (Exception ex)
            {
                LogError("Cart navigation failed", ex);
                throw;
            }
        }

        /// <summary>
        /// Verifies that the support level for the specified bundle matches the expected plan.
        /// </summary>
        /// <param name="bundle">The product bundle to verify.</param>
        /// <param name="plan">The expected support plan.</param>
        /// <returns><c>true</c> if the support level matches; otherwise, <c>false</c>.</returns>
        public bool VerifySupportLevel(ProductBundle bundle, string plan)
        {
            var actualText = GetSupportText(bundle);
            var isMatch = actualText.Contains(plan, StringComparison.OrdinalIgnoreCase);

            Log("Verifying support level", $"{bundle} - Expected: '{plan}', Actual: '{actualText}'");

            return isMatch;
        }

        /// <summary>
        /// Verifies that the response time for the specified bundle matches the expected response time.
        /// </summary>
        /// <param name="bundle">The product bundle to verify.</param>
        /// <param name="responseTime">The expected response time.</param>
        /// <returns><c>true</c> if the response time matches; otherwise, <c>false</c>.</returns>
        public bool VerifyResponseTime(ProductBundle bundle, string responseTime)
        {
            var actualText = GetSupportText(bundle);
            var isMatch = actualText.Contains(responseTime, StringComparison.OrdinalIgnoreCase);

            Log("Verifying response time", $"{bundle} - Expected: '{responseTime}', Actual: '{actualText}'");

            return isMatch;
        }

        /// <summary>
        /// Verifies that the incident limit or features for the specified bundle match the expected incidents.
        /// </summary>
        /// <param name="bundle">The product bundle to verify.</param>
        /// <param name="incidents">The expected incident limit or features.</param>
        /// <returns><c>true</c> if the incidents match; otherwise, <c>false</c>.</returns>
        public bool VerifyIncidentLimit(ProductBundle bundle, string incidents)
        {
            var actualText = GetSupportText(bundle);
            var isMatch = actualText.Contains(incidents, StringComparison.OrdinalIgnoreCase);

            Log("Verifying incident limit", $"{bundle} - Expected: '{incidents}', Actual: '{actualText}'");

            return isMatch;
        }

        /// <summary>
        /// Retrieves the support information text for the specified product bundle.
        /// </summary>
        /// <param name="bundle">The product bundle to get support text for.</param>
        /// <returns>The support information text.</returns>
        public string GetSupportText(ProductBundle bundle)
        {
            var supportElement = WaitAndFindElement(_bundleSupportLocators[bundle],
                $"support info for {bundle}");

            var text = supportElement.Text.Trim();
            LogSuccess("Got support text", $"{bundle} - '{text}'");

            return text;
        }

        /// <summary>
        /// Waits for the page to load completely and network activity to be idle.
        /// </summary>
        /// <exception cref="WebDriverTimeoutException">Thrown when the page load times out.</exception>
        private void WaitForPageLoad()
        {
            try
            {
                Wait.Until(driver => ((IJavaScriptExecutor)driver)
                    .ExecuteScript("return document.readyState").ToString() == "complete");
                WaitForNetworkIdle();
                LogSuccess("Page loaded");
            }
            catch (WebDriverTimeoutException ex)
            {
                LogError("Page load timeout", ex);
                throw;
            }
        }

        /// <summary>
        /// Scrolls the browser viewport to the bundle section on the purchase page.
        /// </summary>
        private void ScrollToBundleSection()
        {
            try
            {
                Log("Scrolling to bundle section");
                var element = Driver.FindElement(By.CssSelector("tr.Pricings-button"));
                ((IJavaScriptExecutor)Driver).ExecuteScript(
                    "arguments[0].scrollIntoView({behavior: 'smooth', block: 'center'});", element);
                Thread.Sleep(500);
            }
            catch (Exception ex)
            {
                LogWarning("Bundle section scroll failed", ex.Message);
            }
        }
    }
}
