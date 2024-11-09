using OpenQA.Selenium;
using System.Text.RegularExpressions;
using NUnit.Framework;
using TelerikCart.UITests.Core.Base;

namespace TelerikCart.UITests.Pages;

public class CartPage : BasePage
{
    private const string PageUrl = "https://www.telerik.com/purchase";
    private string? _savedBundlePrice;
    private readonly CommonComponents _commonComponents;

    // Locators
    private readonly By _pricePerLicense = By.CssSelector(".e2e-price-per-license");
    private readonly By _licenseTotalPrice = By.CssSelector(".e2e-total-price");
    private readonly By _quantityValue = By.CssSelector("td[data-label='Licenses'] .k-input-value-text");
    private readonly By _periodDropdownButton = By.CssSelector("period-select kendo-dropdownlist .k-input-button");
    private readonly By _periodDropdownItems = By.CssSelector("kendo-popup[class*='k-animation-container'] .k-list-item");
    private readonly By _selectedPeriodText = By.CssSelector("period-select kendo-dropdownlist .k-input-value-text");
    private readonly By _licenseCell = By.CssSelector("td[data-label='Licenses']");
    private readonly By _dropdownButton = By.CssSelector("button.k-input-button");
    private readonly By _listItems = By.CssSelector("ul.k-list-ul li.k-list-item");
    private decimal _lastPrice;
    
    public enum PeriodOption
    {
        OneYearIncluded = 0,   // 0% discount
        PlusOneYear = 5,       // 5% discount
        PlusTwoYears = 8,      // 8% discount
        PlusThreeYears = 11,   // 11% discount
        PlusFourYears = 14     // 14% discount
    }

    public CartPage(IWebDriver driver) : base(driver, "Purchase Page")
    {
        _commonComponents = new CommonComponents(driver);
    }

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
    
    public decimal GetTotalPrice()
    {
        try
        {
            var priceElement = WaitAndFindElement(_licenseTotalPrice, "Total price");
            var priceText = priceElement.Text;
            return ParsePrice(priceText);
        }
        catch (Exception ex)
        {
            LogFailure("Failed to get total price", ex);
            throw;
        }
    }
    
    public int GetTotalQuantity()
    {
        try
        {
            var quantityElements = WaitAndFindElements(_quantityValue, "License quantity values");
            var total = quantityElements.Sum(element => int.Parse(element.Text));
            LogSuccess($"Total license quantity in cart: {total}");
            return total;
        }
        catch (Exception ex)
        {
            LogFailure("Failed to get total quantity value", ex);
            throw;
        }
    }
    
    public int GetQuantityByIndex(int index)
    {
        try
        {
            var quantityElements = WaitAndFindElements(_quantityValue, "Quantity values");
            if (index >= quantityElements.Count)
            {
                throw new ArgumentException($"Index {index} is out of range. Only {quantityElements.Count} items in cart.");
            }
            return int.Parse(quantityElements.ElementAt(index).GetAttribute("value"));
        }
        catch (Exception ex)
        {
            LogFailure($"Failed to get quantity value for item at index {index}", ex);
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

    public void SelectPeriod(PeriodOption period)
    {
            Log($"Attempting to select period: {period}");
            
            // Open dropdown
            WaitAndClick(_periodDropdownButton, "Period dropdown button");
            
            // Wait for popup to be visible and get options
            var options = Wait.Until(driver => 
            {
                var items = driver.FindElements(_periodDropdownItems);
                return items.Count > 0 ? items : null;
            });

            // Log all options for debugging
            foreach (var option in options)
            {
                var rawText = option.Text;
                var rawHtml = option.GetAttribute("innerHTML");
                Log($"Found option - Text: '{rawText}', HTML: '{rawHtml}'");
            }

            // Map enum to expected text pattern
            var (displayText, searchText) = period switch
            {
                PeriodOption.OneYearIncluded => ("1 year included", "1 year"),
                PeriodOption.PlusOneYear => ("+1 year", "+1 year"),
                PeriodOption.PlusTwoYears => ("+2 years", "+2 year"),
                PeriodOption.PlusThreeYears => ("+3 years", "+3 year"),
                PeriodOption.PlusFourYears => ("+4 years", "+4 year"),
                _ => throw new ArgumentException($"Unsupported period option: {period}")
            };

            // Find matching option using more flexible matching
            var targetOption = options.FirstOrDefault(option => 
            {
                var optionText = option.Text.Trim();
                var optionHtml = option.GetAttribute("innerHTML");
                
                return optionText.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                       optionHtml.Contains(searchText, StringComparison.OrdinalIgnoreCase);
            });

            if (targetOption == null)
            {
                var availableOptions = string.Join(", ", 
                    options.Select(o => $"'{o.Text.Trim()}'"));
                    
                throw new NoSuchElementException(
                    $"Could not find period option containing text: '{searchText}'. " +
                    $"Available options: {availableOptions}");
            }

            // Scroll option into view if needed
            ((IJavaScriptExecutor)Driver).ExecuteScript(
                "arguments[0].scrollIntoView(true);", targetOption);
            
            // Click the option
            targetOption.Click();

            // Verify selection
            Wait.Until(driver =>
            {
                var selectedText = GetSelectedPeriod();
                return selectedText.Contains(searchText, StringComparison.OrdinalIgnoreCase);
            });

            LogSuccess($"Successfully selected period: {period} ({displayText})");
    }

    public string GetSelectedPeriod()
    {
        return WaitAndFindElement(_selectedPeriodText, "Selected period text")
            .Text.Trim();
    }
    
    public void UpdateQuantity(int quantity)
    {
        try
        {
            Log($"Attempting to update quantity to: {quantity}");
            
            var licenseCell = WaitAndFindElement(_licenseCell, "licenses cell");
            var dropdownButton = licenseCell.FindElement(_dropdownButton);
            WaitAndClick(By.CssSelector("button.k-input-button"), "quantity dropdown button");
            
            Thread.Sleep(500); //dropdown animation
            
            var quantityOption = Wait.Until(driver => {
                var items = driver.FindElements(_listItems);
                return items.FirstOrDefault(item => item.Text.Trim() == quantity.ToString());
            });

            if (quantityOption == null)
            {
                throw new NoSuchElementException($"Could not find quantity option: {quantity}");
            }

            ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].click();", quantityOption);

            // Use the common retry mechanism for price stabilization
            _commonComponents.RetryUntilSuccess(
                CheckPriceStability,
                isStable => isStable,
                "Wait for price stabilization"
            );

            LogSuccess($"Successfully updated quantity to: {quantity}");
        }
        catch (Exception ex)
        {
            LogFailure($"Failed to update quantity to {quantity}", ex);
            TakeScreenshot("QuantityUpdateFailure");
            throw;
        }
    }

    private bool CheckPriceStability()
    {
        var currentPrice = GetTotalPrice();
        if (_lastPrice == 0)
        {
            _lastPrice = currentPrice;
            return false;
        }

        if (currentPrice == _lastPrice)
        {
            Thread.Sleep(300); 
            var verificationPrice = GetTotalPrice();
            return verificationPrice == currentPrice;
        }

        _lastPrice = currentPrice;
        return false;
    }
}