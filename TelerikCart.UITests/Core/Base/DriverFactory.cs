using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;

namespace TelerikCart.UITests.Core.Base
{
    /// <summary>
    /// Factory for creating and managing WebDriver instances.
    /// </summary>
    public class DriverFactory
    {
        private static IWebDriver? _driver;
        private static readonly object Lock = new object();

        /// <summary>
        /// Test configuration settings.
        /// </summary>
        public static class TestSettings
        {
            /// <summary>
            /// Determines if the browser should run in headless mode based on environment variable.
            /// Defaults to true if not set.
            /// </summary>
            public static bool IsHeadless
            {
                get
                {
                    var envVar = Environment.GetEnvironmentVariable("TEST_HEADLESS");
                    return string.IsNullOrEmpty(envVar) ? true : bool.Parse(envVar);
                }
            }
        }

        /// <summary>
        /// Creates and configures the WebDriver instance.
        /// </summary>
        /// <returns>Configured IWebDriver instance.</returns>
        public static IWebDriver CreateDriver()
        {
            if (_driver != null) return _driver;

            lock (Lock)
            {
                if (_driver != null) return _driver;

                // Setup WebDriver using WebDriverManager
                new DriverManager().SetUpDriver(new ChromeConfig());

                var options = new ChromeOptions();

                // Configure headless mode if enabled
                if (TestSettings.IsHeadless)
                {
                    options.AddArguments(
                        "--headless=new",
                        "--window-size=1920,1080"
                    );
                }

                // Add performance and security optimizations
                options.AddArguments(
                    "--start-maximized",
                    "--disable-gpu",
                    "--disable-dev-shm-usage",
                    "--no-sandbox",
                    "--disable-extensions",
                    "--disable-notifications",
                    "--disable-popup-blocking",
                    "--disable-infobars",
                    "--disable-blink-features=AutomationControlled",
                    "--disable-site-isolation-trials"
                );

                // Block unwanted resources to improve performance
                options.AddUserProfilePreference("profile.default_content_settings.images", 2);
                options.AddUserProfilePreference("profile.default_content_settings.cookies", 2);
                options.AddUserProfilePreference("profile.default_content_settings.popups", 2);
                options.AddUserProfilePreference("profile.default_content_settings.geolocation", 2);
                options.AddUserProfilePreference("profile.default_content_settings.notifications", 2);
                options.AddUserProfilePreference("profile.default_content_settings.plugins", 2);
                options.AddUserProfilePreference("profile.password_manager_enabled", false);
                options.AddUserProfilePreference("credentials_enable_service", false);
                options.AddUserProfilePreference("profile.default_content_setting_values.media_stream", 2);
                options.AddUserProfilePreference("profile.default_content_setting_values.media_stream_mic", 2);
                options.AddUserProfilePreference("profile.default_content_setting_values.media_stream_camera", 2);

                // Initialize ChromeDriver with configured options
                _driver = new ChromeDriver(options);

                // Set WebDriver timeouts
                _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
                _driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(30);
                _driver.Manage().Timeouts().AsynchronousJavaScript = TimeSpan.FromSeconds(10);

                return _driver;
            }
        }

        /// <summary>
        /// Quits and disposes the WebDriver instance.
        /// </summary>
        public static void QuitDriver()
        {
            if (_driver == null) return;

            lock (Lock)
            {
                if (_driver == null) return;

                try
                {
                    _driver.Quit();
                }
                catch (Exception)
                {
                    // Suppress any exceptions during quit
                }
                finally
                {
                    _driver?.Dispose();
                    _driver = null;
                }
            }
        }
    }
}
