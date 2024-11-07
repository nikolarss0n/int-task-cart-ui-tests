using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace TelerikCart.UITests.Core.Base;

public class WebElementHandler
{
    private readonly IWebDriver _driver;
    private readonly WebDriverWait _wait;
    private const int DefaultTimeout = 10;

    public WebElementHandler(IWebDriver driver)
    {
        _driver = driver;
        _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(DefaultTimeout));
    }

    public void ClickElement(By locator, bool waitForDisappear = false)
    {
        try
        {
            var element = WaitForElementToBeClickable(locator);
            EnsureElementIsInView(element);
            element.Click();

            if (waitForDisappear)
            {
                WaitForElementToDisappear(locator);
            }
        }
        catch (Exception ex) when (ex is ElementClickInterceptedException or WebDriverTimeoutException)
        {
            HandleClickException(locator, ex);
        }
    }

    private IWebElement WaitForElementToBeClickable(By locator)
    {
        return _wait.Until(ExpectedConditions.ElementToBeClickable(locator));
    }

    private void WaitForElementToDisappear(By locator)
    {
        _wait.Until(ExpectedConditions.InvisibilityOfElementLocated(locator));
    }

    private void EnsureElementIsInView(IWebElement element)
    {
        ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView(true);", element);
        Thread.Sleep(200); // Pause for scroll to complete
    }

    private void HandleClickException(By locator, Exception exception)
    {
        TestContext.WriteLine($"Failed to click element {locator}: {exception.Message}");
        
        try
        {
            var element = _driver.FindElement(locator);
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", element);
        }
        catch (Exception fallbackEx)
        {
            throw new WebDriverException($"Failed to click element {locator} using both standard and JavaScript methods.", fallbackEx);
        }
    }
}