using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using TelerikCart.UITests.Core.Reporting;

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
}