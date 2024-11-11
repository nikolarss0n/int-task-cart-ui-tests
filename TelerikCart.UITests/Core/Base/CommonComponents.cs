using System.Diagnostics;
using OpenQA.Selenium;
using TelerikCart.UITests.Core.Base;

namespace TelerikCart.UITests.Pages
{
    /// <summary>
    /// Represents common UI components and actions.
    /// </summary>
    public class CommonComponents : BasePage
    {
        private readonly By _acceptCookiesButton = By.Id("onetrust-accept-btn-handler");

        /// <summary>
        /// Initializes a new instance of the <see cref="CommonComponents"/> class.
        /// </summary>
        /// <param name="driver">The WebDriver instance.</param>
        public CommonComponents(IWebDriver driver) : base(driver, "Common Components") { }

        /// <summary>
        /// Accepts cookies by clicking the accept button.
        /// </summary>
        public void AcceptCookies()
        {
            try
            {
                WaitAndClick(_acceptCookiesButton, "Accept Cookies button", waitForDisappear: true);
                WaitForNetworkIdle();
                LogSuccess("Accepted cookies");
            }
            catch (WebDriverTimeoutException)
            {
                LogWarning("Cookie banner not found - may have been already accepted");
            }
        }
        
        /// <summary>
        /// Retries an action until it succeeds or a timeout is reached.
        /// </summary>
        /// <typeparam name="T">The return type of the action.</typeparam>
        /// <param name="action">The action to execute.</param>
        /// <param name="validateResult">Function to validate the action's result.</param>
        /// <param name="operationName">Name of the operation for logging.</param>
        /// <param name="timeout">Maximum time to retry.</param>
        /// <param name="interval">Interval between retries.</param>
        /// <returns>The result of the successful action.</returns>
        /// <exception cref="WebDriverTimeoutException">Thrown if the action fails after retries.</exception>
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
                    Log($"Attempting {operationName}", $"Attempt {attempts}");
                    var result = action();
                    
                    if (validateResult(result))
                    {
                        LogSuccess($"{operationName} succeeded", $"Attempt {attempts}, Duration: {stopwatch.ElapsedMilliseconds}ms");
                        return result;
                    }
                    
                    Log($"Validation failed for {operationName}", $"Attempt {attempts}");
                }
                catch (Exception ex) when (ex is StaleElementReferenceException 
                                          || ex is NoSuchElementException
                                          || ex is ElementNotInteractableException)
                {
                    lastException = ex;
                    LogWarning($"{operationName} failed", $"Attempt {attempts}, Error: {ex.Message}");
                }

                if (stopwatch.Elapsed + interval.Value < timeout.Value)
                {
                    Thread.Sleep(interval.Value);
                }
            }

            var errorMessage = $"{operationName} failed after {attempts} attempts ({stopwatch.ElapsedMilliseconds}ms)";
            LogError(errorMessage, lastException);
            throw new WebDriverTimeoutException(errorMessage, lastException);
        }
    }
}
