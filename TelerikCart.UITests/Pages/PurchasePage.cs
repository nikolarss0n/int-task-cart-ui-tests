using OpenQA.Selenium;
using System.Text.RegularExpressions;
using NUnit.Framework;
using TelerikCart.UITests.Core.Base;

namespace TelerikCart.UITests.Pages;

public class PurchasePage : BasePage
{
    private const string PageUrl = "https://www.telerik.com/purchase";
    private string? _savedBundlePrice;

    // Locators
    private readonly By _cartButton = By.CssSelector("[href*='/shopping-cart']");
    private readonly By _acceptCookiesButton = By.Id("onetrust-accept-btn-handler");
    private readonly By _pricePerLicense = By.CssSelector(".e2e-price-per-license");
    private readonly By _quantityValue = By.CssSelector(".k-input-value-text");

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

    public void AcceptCookies()
    {
        try
        {
            WaitAndClick(_acceptCookiesButton, "Accept Cookies button", waitForDisappear: true);
            WaitForNetworkIdle();
        }
        catch (WebDriverTimeoutException)
        {
            Log("Cookie banner was not found - it might have been already accepted");
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

            _savedBundlePrice = NormalizePriceString(priceText);
            var price = ParsePrice(_savedBundlePrice);
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

    public decimal GetCurrentPrice()
    {
        try
        {
            var priceElement = WaitAndFindElement(_pricePerLicense, "Price per license");
            var priceText = priceElement.Text;
            return ParsePrice(priceText);
        }
        catch (Exception ex)
        {
            LogFailure("Failed to get current price", ex);
            throw;
        }
    }

    public decimal GetSavedPrice()
    {
        if (string.IsNullOrEmpty(_savedBundlePrice))
        {
            throw new InvalidOperationException("No price has been saved. Call SaveBundlePrice first.");
        }
        return ParsePrice(_savedBundlePrice);
    }

    public int GetQuantity()
    {
        try
        {
            var quantityElement = WaitAndFindElement(_quantityValue, "Quantity value");
            return int.Parse(quantityElement.Text);
        }
        catch (Exception ex)
        {
            LogFailure("Failed to get quantity value", ex);
            throw;
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

    private static string NormalizePriceString(string price)
    {
        // Remove any currency symbols, commas, and trim whitespace
        return Regex.Replace(price, @"[^0-9.]", "").Trim();
    }

    private static decimal ParsePrice(string price)
    {
        var normalizedPrice = NormalizePriceString(price);
        if (!decimal.TryParse(normalizedPrice, out var result))
        {
            throw new FormatException($"Unable to parse price: {price}");
        }
        return result;
    }

    public bool VerifyPriceMatchesSaved(decimal tolerance = 0.01M)
    {
        var savedPrice = GetSavedPrice();
        var currentPrice = GetCurrentPrice();
        var difference = Math.Abs(savedPrice - currentPrice);
        
        if (difference <= tolerance)
        {
            LogSuccess($"Prices match within tolerance: Saved={savedPrice:C}, Current={currentPrice:C}");
            return true;
        }
        
        LogFailure($"Price mismatch: Saved={savedPrice:C}, Current={currentPrice:C}, Difference={difference:C}");
        return false;
    }

    public void AssertPriceMatchesSaved(decimal tolerance = 0.01M)
    {
        if (!VerifyPriceMatchesSaved(tolerance))
        {
            throw new AssertionException($"Price verification failed: Saved price ({GetSavedPrice():C}) " +
                                       $"does not match current price ({GetCurrentPrice():C})");
        }
    }
}