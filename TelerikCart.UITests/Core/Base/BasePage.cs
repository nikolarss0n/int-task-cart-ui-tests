using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using TelerikCart.UITests.Core.Reporting;

namespace TelerikCart.UITests.Core.Base;

public abstract class BasePage
{
    protected readonly IWebDriver Driver;
    protected readonly WebDriverWait Wait;
    protected readonly string PageName;

    protected BasePage(IWebDriver driver, string pageName)
    {
        Driver = driver;
        Wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        PageName = pageName;
    }

    protected void Log(string message)
    {
        ExtentTestManager.LogInfo($"[{PageName}] {message}");
    }

    protected void LogSuccess(string message)
    {
        ExtentTestManager.LogPass($"[{PageName}] {message}");
    }

    protected void LogFailure(string message, Exception ex = null)
    {
        ExtentTestManager.LogFail($"[{PageName}] {message}", ex);
    }
    
    protected void LogWarning(string message)
    {
        ExtentTestManager.LogWarning($"[{PageName}] {message}");
    }

    protected void TakeScreenshot(string message)
    {
        ExtentTestManager.LogScreenshot(Driver, $"[{PageName}] {message}");
    }

    protected IWebElement WaitAndFindElement(By by, string elementDescription)
    {
        try
        {
            Log($"Waiting for {elementDescription}");
            var element = Wait.Until(ExpectedConditions.ElementExists(by));
            LogSuccess($"Found {elementDescription}");
            return element;
        }
        catch (WebDriverTimeoutException ex)
        {
            LogFailure($"Failed to find {elementDescription}", ex);
            TakeScreenshot($"Element not found: {elementDescription}");
            throw;
        }
    }

    protected void WaitAndClick(By by, string elementDescription, bool waitForDisappear = false)
    {
        try
        {
            Log($"Attempting to click {elementDescription}");
        
            // Wait for visibility
            Wait.Until(ExpectedConditions.ElementIsVisible(by));
        
            // Wait for element to be clickable
            var element = Wait.Until(ExpectedConditions.ElementToBeClickable(by));
        
            // Scroll into view
            ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].scrollIntoView(true);", element);
            Thread.Sleep(500); // Pause for scroll to complete
        
            element.Click();
            LogSuccess($"Successfully clicked {elementDescription}");

            if (waitForDisappear)
            {
                Wait.Until(ExpectedConditions.InvisibilityOfElementLocated(by));
                Log($"{elementDescription} disappeared after click");
            }
        }
        catch (ElementClickInterceptedException ex)
        {
            Log($"Standard click failed for {elementDescription}, attempting JavaScript click");
            try
            {
                // JavaScript click
                var element = Driver.FindElement(by);
                ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].click();", element);
                LogSuccess($"Successfully clicked {elementDescription} using JavaScript");
            }
            catch (Exception jsEx)
            {
                LogFailure($"Failed to click {elementDescription} using both standard and JavaScript methods", jsEx);
                TakeScreenshot($"Failed to click: {elementDescription}");
                throw;
            }
        }
        catch (WebDriverTimeoutException ex)
        {
            LogFailure($"Element {elementDescription} was not clickable", ex);
            TakeScreenshot($"Element not clickable: {elementDescription}");
            throw;
        }
    }
    
    protected bool WaitForElementToBeVisible(By by, string elementDescription)
    {
        try
        {
            Wait.Until(ExpectedConditions.ElementIsVisible(by));
            LogSuccess($"{elementDescription} is visible");
            return true;
        }
        catch (WebDriverTimeoutException ex)
        {
            LogFailure($"{elementDescription} did not become visible", ex);
            TakeScreenshot($"Element not visible: {elementDescription}");
            return false;
        }
    }

    protected IReadOnlyCollection<IWebElement> WaitAndFindElements(By by, string elementsDescription)
    {
        try
        {
            Log($"Waiting for {elementsDescription}");
            var elements = Wait.Until(driver => {
                var foundElements = driver.FindElements(by);
                return foundElements.Count > 0 ? foundElements : null;
            });
            LogSuccess($"Found {elements.Count} {elementsDescription}");
            return elements;
        }
        catch (WebDriverTimeoutException ex)
        {
            LogFailure($"Failed to find any {elementsDescription}", ex);
            TakeScreenshot($"Elements not found: {elementsDescription}");
            throw;
        }
    }

    protected void NavigateToUrl(string url)
    {
        try
        {
            Log($"Navigating to {url}");
            Driver.Navigate().GoToUrl(url);
            LogSuccess($"Successfully navigated to {url}");
        }
        catch (Exception ex)
        {
            LogFailure($"Failed to navigate to {url}", ex);
            TakeScreenshot($"Navigation failed to: {url}");
            throw;
        }
    }

    protected void WaitForNetworkIdle(int timeoutSeconds = 30)
    {
        try
        {
            Log("Waiting for network to become idle");
            var start = DateTime.Now;
            
            while ((DateTime.Now - start).TotalSeconds < timeoutSeconds)
            {
                var readyState = ((IJavaScriptExecutor)Driver)
                    .ExecuteScript("return document.readyState")?.ToString();
                
                if (readyState == "complete")
                {
                    LogSuccess("Network is idle");
                    return;
                }
                Thread.Sleep(500);
            }
            
            LogFailure("Network did not become idle within timeout");
        }
        catch (Exception ex)
        {
            LogFailure("Error while waiting for network idle", ex);
            throw;
        }
    }
}