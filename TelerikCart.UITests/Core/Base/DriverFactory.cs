using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;

namespace TelerikCart.UITests.Core.Base;

public class DriverFactory
{
    private static IWebDriver? _driver;

    public static IWebDriver CreateDriver()
    {
        if (_driver != null) return _driver;

        // Setup WebDriver manager
        new DriverManager().SetUpDriver(new ChromeConfig());

        var options = new ChromeOptions();
        
        // Performance and security optimizations
        options.AddArgument("--start-maximized");
        options.AddArgument("--disable-gpu");
        options.AddArgument("--disable-dev-shm-usage");
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-extensions");
        options.AddArgument("--disable-notifications");
        options.AddArgument("--disable-popup-blocking");
        options.AddArgument("--disable-infobars");
        options.AddArgument("--disable-blink-features=AutomationControlled");
        options.AddArgument("--disable-site-isolation-trials");
        
        // Block unwanted resources and improve performance
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
        
        // Create driver
        _driver = new ChromeDriver(options);
        
        // Set timeouts
        _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
        _driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(30);
        _driver.Manage().Timeouts().AsynchronousJavaScript = TimeSpan.FromSeconds(10);

        return _driver;
    }

    public static void QuitDriver()
    {
        if (_driver == null) return;
        
        try
        {
            _driver.Quit();
        }
        catch (Exception)
        {
            // Ignore errors during quit
        }
        finally
        {
            _driver?.Dispose();
            _driver = null;
        }
    }
}