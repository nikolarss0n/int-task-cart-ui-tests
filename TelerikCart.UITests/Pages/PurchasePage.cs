using OpenQA.Selenium;
using TelerikCart.UITests.Core.Helpers;
using NUnit.Framework;
using TelerikCart.UITests.Core.Base;
using TelerikCart.UITests.Core.Reporting;

namespace TelerikCart.UITests.Pages;

public class PurchasePage : BasePage
{
    private const string PageUrl = "https://www.telerik.com/purchase";
    private string? _savedBundlePrice;
    ProductBundle? _selectedBundle;

    // Locators
    private readonly By _cartButton = By.CssSelector("[href*='/shopping-cart']");
    

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

    public PurchasePage(IWebDriver driver) : base(driver, "Purchase Page") { }

    public void NavigateTo()
    {
        NavigateToUrl(PageUrl);
        WaitForPageLoad();
    }

    private void WaitForPageLoad()
    {
        try
        {
            Wait.Until(driver => ((IJavaScriptExecutor)driver)
                .ExecuteScript("return document.readyState").ToString() == "complete");
            WaitForNetworkIdle();
        }
        catch (WebDriverTimeoutException ex)
        {
            LogFailure("Page failed to load completely", ex);
            throw;
        }
    }

    public decimal SaveBundlePrice(ProductBundle bundle)
    {
        try
        {
            Log($"Saving price for {bundle} bundle");
            var priceElement = WaitAndFindElement(_bundlePriceTags[bundle], $"{bundle} price");
            var priceText = priceElement.GetAttribute("data-price-without-addon");
            
            if (string.IsNullOrEmpty(priceText))
            {
                throw new NoSuchElementException($"Price data attribute is empty for {bundle} bundle");
            }

            _savedBundlePrice = PriceUtils.NormalizePriceString(priceText);
            var price = PriceUtils.ParsePrice(_savedBundlePrice);
            LogSuccess($"Saved {bundle} price: {price:C}");
            return price;
        }
        catch (Exception ex)
        {
            LogFailure($"Failed to save bundle price for {bundle}", ex);
            throw;
        }
    }

    public void SelectBundle(ProductBundle bundle)
    {
        try
        {
            _selectedBundle = bundle;
            Log($"Selecting {bundle} bundle");
            ScrollToBundleSection();
            WaitAndClick(_bundleBuyButtons[bundle], $"Buy Now button for {bundle}");
            WaitForNetworkIdle();
            LogSuccess($"Successfully selected {bundle} bundle");
        }
        catch (Exception ex)
        {
            LogFailure($"Failed to select bundle {bundle}", ex);
            throw;
        }
    }

    private void ScrollToBundleSection()
    {
        try
        {
            var element = Driver.FindElement(By.CssSelector("tr.Pricings-button"));
            ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].scrollIntoView({behavior: 'smooth', block: 'center'});", element);
            Thread.Sleep(500); // Wait for smooth scroll
        }
        catch (Exception ex)
        {
            LogWarning($"Failed to scroll to bundle section: {ex.Message}");
        }
    }

    public void GoToCart()
    {
        try
        {
            WaitAndClick(_cartButton, "Go to Cart button");
            WaitForNetworkIdle();
            TakeScreenshot("Navigated to cart page");
            LogSuccess("Successfully navigated to cart");
        }
        catch (Exception ex)
        {
            LogFailure("Failed to navigate to cart", ex);
            throw;
        }
    }
    
    public bool VerifySupportLevel(ProductBundle bundle, string plan)
    {
        var actualText = GetSupportText(bundle);
        var isMatch = actualText.Contains(plan, StringComparison.OrdinalIgnoreCase);
        
        ExtentTestManager.LogInfo($"Verifying support level for {bundle}:");
        ExtentTestManager.LogInfo($"Expected to contain: '{plan}'");
        ExtentTestManager.LogInfo($"Actual text: '{actualText}'");
        
        return isMatch;
    }

    public bool VerifyResponseTime(ProductBundle bundle, string responseTime)
    {
        var actualText = GetSupportText(bundle);
        var isMatch = actualText.Contains(responseTime, StringComparison.OrdinalIgnoreCase);
        
        ExtentTestManager.LogInfo($"Verifying response time for {bundle}:");
        ExtentTestManager.LogInfo($"Expected to contain: '{responseTime}'");
        ExtentTestManager.LogInfo($"Actual text: '{actualText}'");
        
        return isMatch;
    }

    public bool VerifyIncidentLimit(ProductBundle bundle, string incidents)
    {
        var actualText = GetSupportText(bundle);
        var isMatch = actualText.Contains(incidents, StringComparison.OrdinalIgnoreCase);
        
        ExtentTestManager.LogInfo($"Verifying incident limit for {bundle}:");
        ExtentTestManager.LogInfo($"Expected to contain: '{incidents}'");
        ExtentTestManager.LogInfo($"Actual text: '{actualText}'");
        
        return isMatch;
    }

    public string GetSupportText(ProductBundle bundle)
    {
        var supportElement = WaitAndFindElement(_bundleSupportLocators[bundle], 
            $"support info for {bundle}");
        return supportElement.Text.Trim();
    }
    
    public bool VerifyNavigation(string expectedUrl, int timeoutSeconds = 10)
    {
        return WaitForUrlContains(expectedUrl, timeoutSeconds);
    }
}