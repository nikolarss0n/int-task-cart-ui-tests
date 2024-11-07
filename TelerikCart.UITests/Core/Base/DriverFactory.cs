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
        
        new DriverManager().SetUpDriver(new ChromeConfig());

        var options = new ChromeOptions();
        options.AddArgument("--start-maximized");
        
        _driver = new ChromeDriver(options);
        
        _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
        _driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(30);

        return _driver;
    }

    public static void QuitDriver()
    {
        if (_driver == null) return;
        
        _driver.Quit();
        _driver.Dispose();
        _driver = null;
    }
}