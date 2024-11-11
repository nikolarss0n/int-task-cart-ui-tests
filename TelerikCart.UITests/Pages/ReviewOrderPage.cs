using OpenQA.Selenium;
using TelerikCart.UITests.Core.Base;

namespace TelerikCart.UITests.Pages
{
    /// <summary>
    /// Provides methods for interacting with the review order page, including navigation
    /// and retrieval of billing information such as full name, email, company, address,
    /// city, and country.
    /// </summary>
    public class ReviewOrderPage : BasePage
    {
        private const string PageUrl = "https://store.progress.com/review-order";
        private readonly CommonComponents _commonComponents;

        // Locators
        private readonly By _fullName = By.ClassName("e2e-billing-info-fullName");
        private readonly By _email = By.ClassName("e2e-billing-info-email");
        private readonly By _company = By.ClassName("e2e-billing-info-company");
        private readonly By _address = By.ClassName("e2e-billing-info-address");
        private readonly By _city = By.ClassName("e2e-billing-info-city");
        private readonly By _country = By.ClassName("e2e-billing-info-country");

        /// <summary>
        /// Initializes a new instance of the <see cref="ReviewOrderPage"/> class.
        /// </summary>
        /// <param name="driver">The WebDriver instance to interact with the browser.</param>
        public ReviewOrderPage(IWebDriver driver) : base(driver, "Review Order Page")
        {
            _commonComponents = new CommonComponents(driver);
        }

        /// <summary>
        /// Navigates to the review order page and waits for it to load.
        /// </summary>
        public void NavigateTo()
        {
            NavigateToUrl(PageUrl);
            WaitForPageLoad();
        }

        /// <summary>
        /// Retrieves the full name displayed on the review order page.
        /// </summary>
        /// <returns>The full name as a string.</returns>
        public string GetFullName() =>
            GetElementText(_fullName, "Full Name field");

        /// <summary>
        /// Retrieves the email displayed on the review order page.
        /// </summary>
        /// <returns>The email as a string.</returns>
        public string GetEmail() =>
            GetElementText(_email, "Email field");

        /// <summary>
        /// Retrieves the company name displayed on the review order page.
        /// </summary>
        /// <returns>The company name as a string.</returns>
        public string GetCompany() =>
            GetElementText(_company, "Company field");

        /// <summary>
        /// Retrieves the address displayed on the review order page.
        /// </summary>
        /// <returns>The address as a string.</returns>
        public string GetAddress() =>
            GetElementText(_address, "Address field");

        /// <summary>
        /// Retrieves the city displayed on the review order page.
        /// </summary>
        /// <returns>The city as a string.</returns>
        public string GetCity() =>
            GetElementText(_city, "City field");

        /// <summary>
        /// Retrieves the country displayed on the review order page.
        /// </summary>
        /// <returns>The country as a string.</returns>
        public string GetCountry() =>
            GetElementText(_country, "Country field");
    }
}
