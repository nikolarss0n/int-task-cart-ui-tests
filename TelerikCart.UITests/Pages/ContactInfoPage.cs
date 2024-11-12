using OpenQA.Selenium;
using TelerikCart.UITests.Core.Base;

namespace TelerikCart.UITests.Pages
{
    /// <summary>
    /// Provides methods for interacting with the contact information page, including navigation,
    /// filling out personal and billing details, selecting the billing country, submitting the form,
    /// and verifying the visibility and state of page elements.
    /// </summary>
    public class ContactInfoPage : BasePage
    {
        private const string PageUrl = "https://store.progress.com/contact-info";
        private readonly CommonComponents _commonComponents;

        // Locators
        private readonly By _firstName = By.Id("biFirstName");
        private readonly By _siFirstName = By.Id("siFirstName");
        private readonly By _lastName = By.Id("biLastName");
        private readonly By _email = By.Id("biEmail");
        private readonly By _company = By.Id("biCompany");
        private readonly By _phone = By.Id("biPhone");
        private readonly By _address = By.Id("biAddress");
        private readonly By _city = By.Id("biCity");
        private readonly By _country = By.Id("biCountry");
        private readonly By _zip = By.Id("biZipCode");
        private readonly By _countryTax = By.Id("biCountryTaxIdentificationNumber");
        private readonly By _continueButton = By.ClassName("e2e-continue");
        private readonly By _sameAsBillingCheckbox = By.Id("siSameAsBilling");

        /// <summary>
        /// Initializes a new instance of the <see cref="ContactInfoPage"/> class.
        /// </summary>
        /// <param name="driver">The WebDriver instance to interact with the browser.</param>
        public ContactInfoPage(IWebDriver driver) : base(driver, "Contact Info Page")
        {
            _commonComponents = new CommonComponents(driver);
        }

        /// <summary>
        /// Submits the contact information form by clicking the continue button.
        /// </summary>
        public void SubmitContactInfo()
        {
            WaitAndClick(_continueButton, "Submit Contact Info");
        }

        /// <summary>
        /// Navigates to the contact information page and waits for it to load.
        /// </summary>
        public void NavigateTo()
        {
            NavigateToUrl(PageUrl);
            WaitForPageLoad();
        }

        /// <summary>
        /// Unchecks the "Same as Billing" checkbox to allow entering different shipping information.
        /// </summary>
        public void UncheckSameAsBillingCheckbox()
        {
            WaitAndClick(_sameAsBillingCheckbox, "Uncheck Same as Billing Checkbox");
        }

        /// <summary>
        /// Determines whether the "Same as Billing" first name field is visible.
        /// </summary>
        /// <returns><c>true</c> if the field is visible; otherwise, <c>false</c>.</returns>
        public bool IsSameAsBillingFirstNameFieldVisible()
        {
            return IsElementVisible(_siFirstName, "Same as billing first name", 15);
        }

        /// <summary>
        /// Fills in the First Name field with the specified value.
        /// </summary>
        /// <param name="firstName">The first name to enter.</param>
        public void FillFirstName(string firstName)
        {
            SetInputValue(_firstName, firstName, "First Name");
        }

        /// <summary>
        /// Fills in the Last Name field with the specified value.
        /// </summary>
        /// <param name="lastName">The last name to enter.</param>
        public void FillLastName(string lastName)
        {
            SetInputValue(_lastName, lastName, "Last Name");
        }

        /// <summary>
        /// Fills in the Email field with the specified value.
        /// </summary>
        /// <param name="email">The email address to enter.</param>
        public void FillEmail(string email)
        {
            SetInputValue(_email, email, "Email");
        }

        /// <summary>
        /// Fills in the Company field with the specified value.
        /// </summary>
        /// <param name="company">The company name to enter.</param>
        public void FillCompany(string company)
        {
            SetInputValue(_company, company, "Company");
        }

        /// <summary>
        /// Fills in the Phone field with the specified value.
        /// </summary>
        /// <param name="phone">The phone number to enter.</param>
        public void FillPhone(string phone)
        {
            SetInputValue(_phone, phone, "Phone");
        }

        /// <summary>
        /// Fills in the Address field with the specified value.
        /// </summary>
        /// <param name="address">The address to enter.</param>
        public void FillAddress(string address)
        {
            SetInputValue(_address, address, "Address");
        }

        /// <summary>
        /// Fills in the City field with the specified value.
        /// </summary>
        /// <param name="city">The city to enter.</param>
        public void FillCity(string city)
        {
            SetInputValue(_city, city, "City");
        }

        /// <summary>
        /// Selects the billing country from the country dropdown.
        /// </summary>
        /// <param name="country">The country to select.</param>
        public void SelectBillingCountry(string country)
        {
            SelectKendoComboBoxOption(_country, country);
        }

        /// <summary>
        /// Fills in the Zip Code field with the specified value.
        /// </summary>
        /// <param name="zipCode">The zip code to enter.</param>
        public void FillZipCode(string zipCode)
        {
            SetInputValue(_zip, zipCode, "Zip Code");
        }

        /// <summary>
        /// Fills in the Country Tax Identification Number field with the specified value.
        /// </summary>
        /// <param name="countryTaxIdentificationNumber">The country tax identification number to enter.</param>
        public void FillCountryTax(string countryTaxIdentificationNumber)
        {
            SetInputValue(_countryTax, countryTaxIdentificationNumber, "Tax Identification Number");
        }
        
        /// <summary>
        /// Determines whether the VAT ID field is visible.
        /// </summary>
        /// <returns><c>true</c> if the VAT ID field is visible; otherwise, <c>false</c>.</returns>
        public bool IsVatIdFieldVisible()
        {
            return IsElementVisible(_countryTax, "VAT ID field", 15);
        }

        /// <summary>
        /// Verifies whether the continue button is enabled.
        /// </summary>
        /// <returns><c>true</c> if the continue button is enabled; otherwise, <c>false</c>.</returns>
        public bool VerifyButtonIsEnabled()
        {
            return GetElementState(_continueButton, "Continue button");
        }

        /// <summary>
        /// Fills in the contact information form using the provided data.
        /// </summary>
        /// <param name="data">A <see cref="ContactFormData"/> object containing the contact information.</param>
        public void FillContactInfo(ContactFormData data)
        {
            if (data.Country != null)
                SelectBillingCountry(data.Country);

            if (data.FirstName != null)
                FillFirstName(data.FirstName);

            if (data.LastName != null)
                FillLastName(data.LastName);

            if (data.Email != null)
                FillEmail(data.Email);

            if (data.Phone != null)
                FillPhone(data.Phone);

            if (data.Address != null)
                FillAddress(data.Address);

            if (data.Company != null)
                FillCompany(data.Company);

            if (data.City != null)
                FillCity(data.City);
        }
    }
}
