using System.Diagnostics;
using OpenQA.Selenium;

namespace TelerikCart.UITests.Core.Base;

public class CommonComponents : BasePage
{
    private readonly By _acceptCookiesButton = By.Id("onetrust-accept-btn-handler");

    public CommonComponents(IWebDriver driver) : base(driver, "Common Components") { }

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
    
    public T RetryUntilSuccess<T>(
        Func<T> action,
        Func<T, bool> validateResult,
        string operationName,
        TimeSpan? timeout = null,
        TimeSpan? interval = null)
    {
        timeout ??= TimeSpan.FromSeconds(10);
        interval ??= TimeSpan.FromMilliseconds(500);
        
        var stopwatch = Stopwatch.StartNew();
        var attempts = 0;
        Exception? lastException = null;

        while (stopwatch.Elapsed < timeout)
        {
            attempts++;
            try
            {
                Log($"Attempt {attempts} for {operationName}");
                var result = action();
                
                if (validateResult(result))
                {
                    LogSuccess($"{operationName} succeeded after {attempts} attempts ({stopwatch.ElapsedMilliseconds}ms)");
                    return result;
                }
                
                Log($"{operationName} attempt {attempts} did not meet validation criteria");
            }
            catch (Exception ex) when (ex is StaleElementReferenceException 
                                     || ex is NoSuchElementException
                                     || ex is ElementNotInteractableException)
            {
                lastException = ex;
                Log($"{operationName} attempt {attempts} failed: {ex.Message}");
            }

            if (stopwatch.Elapsed + interval.Value < timeout.Value)
            {
                Thread.Sleep(interval.Value);
            }
        }

        var errorMessage = $"{operationName} failed after {attempts} attempts ({stopwatch.ElapsedMilliseconds}ms)";
        LogFailure(errorMessage, lastException);
        throw new WebDriverTimeoutException(errorMessage, lastException);
    }

    public bool RetryUntilSuccess(
        Action action,
        Func<bool> validate,
        string operationName,
        TimeSpan? timeout = null,
        TimeSpan? interval = null)
    {
        return RetryUntilSuccess(
            () => {
                action();
                return true;
            },
            _ => validate(),
            operationName,
            timeout,
            interval);
    }
}