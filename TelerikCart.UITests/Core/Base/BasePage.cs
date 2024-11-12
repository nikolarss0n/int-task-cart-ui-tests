using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using TelerikCart.UITests.Core.Reporting;

namespace TelerikCart.UITests.Core.Base;

public abstract class BasePage
{
    protected readonly IWebDriver Driver;
    protected readonly WebDriverWait Wait;
    protected readonly string PageName;
    private readonly IJavaScriptExecutor _js;

    protected BasePage(IWebDriver driver, string pageName)
    {
        Driver = driver;
        Wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        PageName = pageName;
        _js = (IJavaScriptExecutor)Driver;
    }

    #region Logging Methods
    protected void Log(string action, string? details = null) =>
        ExtentTestManager.LogInfo($"[{PageName}] {action}{(details == null ? "" : $" | {details}")}");

    protected void LogSuccess(string action, string? details = null) =>
        ExtentTestManager.LogPass($"[{PageName}] {action}{(details == null ? "" : $" | {details}")}");

    protected void LogWarning(string action, string? details = null) =>
        ExtentTestManager.LogWarning($"[{PageName}] {action}{(details == null ? "" : $" | {details}")}");

    protected void LogError(string action, Exception? ex = null, string? details = null)
    {
        var message = $"[{PageName}] {action}{(details == null ? "" : $" | {details}")}";
        ExtentTestManager.LogFail(message, ex);
        TakeScreenshot(action);
    }

    protected void TakeScreenshot(string description) =>
        ExtentTestManager.LogScreenshot(Driver, $"[{PageName}] {description}");
    #endregion

    #region Navigation Methods
    protected void NavigateToUrl(string url)
    {
        try
        {
            Log("Navigating", url);
            Driver.Navigate().GoToUrl(url);
            LogSuccess("Navigation complete", url);
        }
        catch (Exception ex)
        {
            LogError("Navigation failed", ex, url);
            throw;
        }
    }

    protected bool WaitForUrlContains(string expectedUrlPart, int timeoutSeconds = 10)
    {
        try
        {
            Log("Waiting for URL change", expectedUrlPart);
            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutSeconds));
            
            return wait.Until(driver =>
            {
                var currentUrl = driver.Url;
                if (currentUrl.Contains(expectedUrlPart))
                {
                    LogSuccess("URL changed", currentUrl);
                    return true;
                }
                Log("Checking URL", currentUrl);
                return false;
            });
        }
        catch (WebDriverTimeoutException ex)
        {
            LogError("URL change timeout", ex, expectedUrlPart);
            return false;
        }
    }

    protected void WaitForNetworkIdle(int timeoutSeconds = 30)
    {
        try
        {
            Log("Waiting for network idle");

            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutSeconds))
            {
                PollingInterval = TimeSpan.FromMilliseconds(500)
            };

            wait.Until(driver => _js.ExecuteScript("return document.readyState").ToString() == "complete");

            LogSuccess("Network is idle");
        }
        catch (WebDriverTimeoutException)
        {
            LogWarning("Network idle timeout reached");
        }
        catch (Exception ex)
        {
            LogError("Network idle check failed", ex);
            throw;
        }
    }

    protected void WaitForPageLoad()
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

    #endregion

    #region Element Interaction Methods
    
    protected void SetInputValue(By inputLocator, string value, string inputDescription)
    {
        try
        {
            Log("Setting input value", $"{inputDescription}: {value}");

            var input = Wait.Until(ExpectedConditions.ElementToBeClickable(inputLocator));
    
            // Scroll element into view
            _js.ExecuteScript("arguments[0].scrollIntoView(true);", input);

            // Wait until the input is clickable after scrolling
            input = Wait.Until(ExpectedConditions.ElementToBeClickable(inputLocator));

            // Clear existing value
            input.Clear();
    
            // If Clear() doesn't work, try alternative clearing methods
            if (!string.IsNullOrEmpty(input.GetAttribute("value")))
            {
                input.SendKeys(Keys.Control + "a" + Keys.Delete);
                // If still not cleared, try with JavaScript
                if (!string.IsNullOrEmpty(input.GetAttribute("value")))
                {
                    _js.ExecuteScript("arguments[0].value = '';", input);
                }
            }

            // Type the new value
            input.SendKeys(value);
    
            // Verify the input value
            Wait.Until(driver =>
            {
                var currentValue = input.GetAttribute("value");
                return currentValue.Equals(value, StringComparison.Ordinal);
            });

            LogSuccess("Input value set", $"{inputDescription}: {value}");
        }
        catch (Exception ex)
        {
            LogError("Failed to set input value", ex, $"{inputDescription}: {value}");
            throw;
        }
    }

    
    protected void SelectKendoComboBoxOption(By comboBoxLocator, string optionText)
    {
        // Click to open the dropdown
        var combobox = Wait.Until(ExpectedConditions.ElementToBeClickable(comboBoxLocator));
        combobox.Click();

        // Find and focus on the input field within the combobox
        var input = Wait.Until(ExpectedConditions.ElementToBeClickable(
            By.CssSelector($"#{combobox.GetAttribute("id")} input")));

        // Clear existing value if any
        var clearButton = Driver.FindElements(By.CssSelector(".k-clear-value"));
        if (clearButton.Any() && clearButton.First().Displayed)
        {
            clearButton.First().Click();
        }

        // Type the option text
        input.SendKeys(optionText);

        // Wait for the dropdown to filter and the desired option to appear
        var optionLocator = By.CssSelector("kendo-popup .k-list-item");
        var targetOption = Wait.Until(driver =>
        {
            var options = driver.FindElements(optionLocator);
            return options.FirstOrDefault(option =>
                option.Text.Trim().Equals(optionText, StringComparison.OrdinalIgnoreCase));
        });

        if (targetOption == null)
        {
            var options = Driver.FindElements(optionLocator);
            var availableOptions = string.Join(", ", options.Select(o => $"'{o.Text.Trim()}'"));
            throw new NoSuchElementException(
                $"Could not find option '{optionText}' in Kendo ComboBox. Available options: {availableOptions}");
        }

        targetOption.Click();

        // Verify selection
        Wait.Until(driver => {
            var selectedText = input.GetAttribute("value");
            return selectedText.Equals(optionText, StringComparison.OrdinalIgnoreCase);
        });
    }

    
    protected void SelectMaintanenceAndSupport(By dropdownLocator, string optionText)
    {
        // Wait for and click dropdown
        var dropdown = Wait.Until(ExpectedConditions.ElementToBeClickable(dropdownLocator));
        dropdown.Click();

        // Wait for dropdown to fully expand
        Wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".k-list-item")));

        // Wait for options to load
        var options = Wait.Until(d => {
            var items = Driver.FindElements(By.CssSelector(".k-list-item"));
            return items.Count > 0 ? items : null;
        });

        var targetOption = options.FirstOrDefault(option => 
            option.Text.Trim().StartsWith(optionText, StringComparison.OrdinalIgnoreCase));

        if (targetOption == null)
        {
            var availableOptions = string.Join(", ", options.Select(o => $"'{o.Text.Trim()}'"));
            throw new NoSuchElementException(
                $"Could not find option starting with '{optionText}' in DropDownList. Available options: {availableOptions}");
        }

        // Click and wait for option to be selected
        targetOption.Click();
        Wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.CssSelector(".k-list-item")));
    }


    protected void SelectBundleQuantity(By dropdownLocator, string optionText)
    {
        try
        {
            // Wait for the dropdown to be clickable
            var dropdownElement = Wait.Until(ExpectedConditions.ElementToBeClickable(dropdownLocator));
    
            // Click the dropdown to focus
            dropdownElement.Click();
    
            // Use Actions to send keys
            Actions actions = new Actions(Driver);
    
            // Type the option text
            actions.SendKeys(optionText).Perform();
    
            // Press Enter to select the highlighted option
            actions.SendKeys(Keys.Enter).Perform();
    
            // Wait for the value to be updated
            Wait.Until(driver =>
            {
                var selectedValueElement = driver.FindElement(dropdownLocator).FindElement(By.CssSelector(".k-input-inner .k-input-value-text"));
                var selectedValue = selectedValueElement.Text.Trim();
                return selectedValue.Equals(optionText, StringComparison.OrdinalIgnoreCase);
            });
        }
        catch (Exception ex)
        {
            LogError("Failed to select Kendo DropDownList option via typing", ex, optionText);
            throw;
        }
    }

    protected IWebElement WaitAndFindElement(By by, string elementDescription)
    {
        try
        {
            Log("Finding element", elementDescription);
            var element = Wait.Until(ExpectedConditions.ElementExists(by));
            LogSuccess("Found element", elementDescription);
            return element;
        }
        catch (WebDriverTimeoutException ex)
        {
            LogError("Element not found", ex, elementDescription);
            throw;
        }
    }
    protected void WaitAndClick(By by, string elementDescription, bool waitForDisappear = false)
    {
        try
        {
            Log("Clicking", elementDescription);
            var element = Wait.Until(ExpectedConditions.ElementToBeClickable(by));
        
            _js.ExecuteScript("arguments[0].scrollIntoView(true);", element);

            element = Wait.Until(ExpectedConditions.ElementToBeClickable(by));

            try
            {
                element.Click();
            }
            catch (ElementClickInterceptedException)
            {
                Log("Using JavaScript click", elementDescription);
                _js.ExecuteScript("arguments[0].click();", element);
            }

            if (waitForDisappear)
            {
                Wait.Until(ExpectedConditions.InvisibilityOfElementLocated(by));
                Log("Element disappeared", elementDescription);
            }

            LogSuccess("Clicked", elementDescription);
        }
        catch (Exception ex)
        {
            LogError("Click failed", ex, elementDescription);
            throw;
        }
    }


protected bool IsElementVisible(By by, string elementDescription, int timeoutSeconds = 10)
    {
        try
        {
            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutSeconds))
            {
                PollingInterval = TimeSpan.FromMilliseconds(500),
                Message = $"Timeout waiting for element: {elementDescription}"
            };

            return wait.Until(driver =>
            {
                try
                {
                    var element = driver.FindElement(by);
                    // Check if element is both present and visible
                    return element != null && element.Displayed && element.Enabled;
                }
                catch (NoSuchElementException)
                {
                    return false;
                }
                catch (StaleElementReferenceException)
                {
                    return false;
                }
                catch (ElementNotInteractableException)
                {
                    return false;
                }
            });
        }
        catch (WebDriverTimeoutException)
        {
            // Log the timeout for debugging purposes
            Console.WriteLine($"Timeout occurred while checking visibility of {elementDescription}");
            return false;
        }
        catch (Exception ex)
        {
            // Log any unexpected exceptions
            Console.WriteLine($"Unexpected error checking visibility of {elementDescription}: {ex.Message}");
            return false;
        }
    }

    // Helper method for explicit waits with custom timeout
    protected bool WaitForElementToBeVisible(By by, string elementDescription, int timeoutSeconds = 10)
    {
        try
        {
            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutSeconds));
            var element = wait.Until(ExpectedConditions.ElementIsVisible(by));
            return element != null;
        }
        catch (WebDriverTimeoutException)
        {
            return false;
        }
    }

    // Helper method to check if element exists in DOM
    protected bool IsElementPresent(By by)
    {
        try
        {
            Driver.FindElement(by);
            return true;
        }
        catch (NoSuchElementException)
        {
            return false;
        }
    }

    protected IReadOnlyCollection<IWebElement> WaitAndFindElements(By by, string elementsDescription)
    {
        try
        {
            Log("Finding elements", elementsDescription);
            var elements = Wait.Until(driver => {
                var found = driver.FindElements(by);
                return found.Count > 0 ? found : null;
            });
            LogSuccess("Found elements", $"{elements.Count} {elementsDescription}");
            return elements;
        }
        catch (WebDriverTimeoutException ex)
        {
            LogError("No elements found", ex, elementsDescription);
            throw;
        }
    }
    #endregion

    #region Element Verification Methods
    
    public bool VerifyNavigation(string expectedUrl, int timeoutSeconds = 10)
    {
        bool success = WaitForUrlContains(expectedUrl, timeoutSeconds);
        
        if (success)
        {
            LogSuccess("Navigation verified", $"URL contains '{expectedUrl}'");
        }
        else
        {
            LogWarning("Navigation not verified", $"URL does not contain '{expectedUrl}' after {timeoutSeconds}s");
        }

        return success;
    }
    
    protected bool GetElementState(By by, string elementDescription, int timeoutSeconds = 10)
    {
        try
        {
            Log($"Getting element state", elementDescription);
    
            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutSeconds));
            var element = wait.Until(driver => driver.FindElement(by));
        
            var isEnabled = element.Enabled && !element.GetAttribute("class").Contains("disabled");
        
            LogSuccess($"Element is {(isEnabled ? "enabled" : "disabled")}", elementDescription);
            return isEnabled;
        }
        catch (WebDriverTimeoutException ex)
        {
            LogError($"Failed to get element state", ex, elementDescription);
            throw;
        }
    }
    protected bool VerifyElementText(By by, string expectedText, string elementDescription, bool exact = true)
    {
        try
        {
            Log("Verifying text", elementDescription);
            var element = WaitAndFindElement(by, elementDescription);
            var actualText = element.Text.Trim();

            bool isMatch = exact 
                ? string.Equals(actualText, expectedText, StringComparison.OrdinalIgnoreCase)
                : actualText.Contains(expectedText, StringComparison.OrdinalIgnoreCase);

            if (isMatch)
            {
                LogSuccess("Text verified", $"'{actualText}' matches '{expectedText}'");
                return true;
            }

            LogWarning("Text mismatch", $"Expected: '{expectedText}', Found: '{actualText}'");
            return false;
        }
        catch (Exception ex)
        {
            LogError("Text verification failed", ex, elementDescription);
            return false;
        }
    }

    protected string GetElementText(By by, string elementDescription)
    {
        try
        {
            Log("Getting text", elementDescription);
            var text = WaitAndFindElement(by, elementDescription).Text.Trim();
            LogSuccess("Got text", $"'{text}'");
            return text;
        }
        catch (Exception ex)
        {
            LogError("Failed to get text", ex, elementDescription);
            throw;
        }
    }
    #endregion
}