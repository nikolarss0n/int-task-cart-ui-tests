using NUnit.Framework;
using TelerikCart.UITests.Core.Base;
using TelerikCart.UITests.Core.Reporting;
using TelerikCart.UITests.Pages;
using System.Diagnostics;
using static TelerikCart.UITests.Pages.PurchasePage;
using YourProject.UITests.Core.Reporting.Documentation.Extensions;

namespace TelerikCart.UITests.Tests
{
    /// <summary>
    /// Contains test cases for validating the Contact Information page functionalities,
    /// including form validation, field visibility, and data persistence.
    /// </summary>
    [TestFixture]
    [Category("ContactInfo")]
    public class ContactInfoTests : BaseTest
    {
        private CommonComponents _commonComponents = null!;
        private PurchasePage _purchasePage = null!;
        private CartPage _cartPage = null!;
        private ContactInfoPage _contactInfoPage = null!;
        private ReviewOrderPage _reviewOrderPage = null!;
        private readonly Stopwatch _testStopwatch = new();
        private const decimal PriceTolerance = 0.01M;
        private const string CartUrl = "https://store.progress.com/your-order";
        private const string ContactInfoUrl = "https://store.progress.com/contact-info";
        private const string ReviewOrderUrl = "https://store.progress.com/review-order";

        /// <summary>
        /// Sets up the necessary page objects before each test.
        /// </summary>
        [SetUp]
        public void TestSetup()
        {
            _commonComponents = new CommonComponents(Driver);
            _purchasePage = new PurchasePage(Driver);
            _cartPage = new CartPage(Driver);
            _contactInfoPage = new ContactInfoPage(Driver);
            _reviewOrderPage = new ReviewOrderPage(Driver);
        }

        /// <summary>
        /// Verifies that the submit button becomes active only when all mandatory fields are filled.
        /// </summary>
        [Test]
        [Description("Verify submit button becomes active only when all mandatory fields are filled")]
        [Category("Contact Info")]
        [Retry(2)]
        public void VerifySubmitButtonIsEnabled()
        {
            // Document the test case details
            ExtentTestManager.GetTest().DocumentTest(
                objective: "Validate that the submit button is enabled only when all mandatory fields are filled",
                steps: new[]
                {
                    "Navigate to purchase page and accept cookies",
                    "Select DevCraft Complete bundle",
                    "Add bundle to cart and verify navigation to cart page",
                    "Proceed as guest to contact info page",
                    "Fill in mandatory fields except for country",
                    "Verify submit button is disabled",
                    "Select billing country",
                    "Verify submit button is enabled"
                },
                criteria: new[]
                {
                    "Submit button remains disabled until all mandatory fields are filled",
                    "Submit button is enabled after selecting country",
                    "Form validation works as expected"
                }
            );

            const ProductBundle selectedBundle = ProductBundle.DevCraftComplete;

            // Navigate to the purchase page and accept cookies
            _purchasePage.NavigateTo();
            _commonComponents.AcceptCookies();

            // Save the expected price and select the bundle
            var expectedPrice = _purchasePage.SaveBundlePrice(selectedBundle);
            _purchasePage.SelectBundle(selectedBundle);

            // Verify navigation to cart page and quantity
            Assert.Multiple(() =>
            {
                Assert.That(_purchasePage.VerifyNavigation(CartUrl), Is.True,
                    $"Failed to navigate to cart page. Current URL: {Driver.Url}");
                Assert.That(_cartPage.GetTotalQuantity(), Is.EqualTo(1));
            });

            // Proceed as a guest to the contact info page
            _cartPage.ContinueAsGuest();
            Assert.That(_purchasePage.VerifyNavigation(ContactInfoUrl), Is.True,
                $"Failed to navigate to contact info page. Current URL: {Driver.Url}");
            _commonComponents.AcceptCookies();

            // Fill in mandatory fields except for the country
            _contactInfoPage.FillFirstName("FirstName");
            _contactInfoPage.FillLastName("LastName");
            _contactInfoPage.FillEmail("Test@test.com");
            _contactInfoPage.FillPhone("+1234567890");
            _contactInfoPage.FillAddress("Address");
            _contactInfoPage.FillCompany("Company");
            _contactInfoPage.FillCity("Sofia");

            // Verify that the submit button is disabled
            Assert.That(_contactInfoPage.VerifyButtonIsEnabled(), Is.False,
                "Submit button should be disabled without country selected");

            // Select the billing country
            _contactInfoPage.SelectBillingCountry("Bulgaria");

            // Verify that the submit button is now enabled
            Assert.That(_contactInfoPage.VerifyButtonIsEnabled(), Is.True,
                "Submit button should be enabled after selecting country");

            // Log the results of the test
            ExtentTestManager.GetTest().LogResults(true, new()
            {
                ["Submit Button State"] = _contactInfoPage.VerifyButtonIsEnabled() ? "Enabled" : "Disabled",
                ["Mandatory Fields Filled"] = "All",
                ["Country Selected"] = "Yes"
            });
        }

        /// <summary>
        /// Verifies that additional billing fields appear when the "Same as Billing" checkbox is unchecked.
        /// </summary>
        [Test]
        [Description("Verify additional billing fields appear when License Holder checkbox is unchecked")]
        [Category("Contact Info")]
        [Category("Smoke")]
        [Retry(2)]
        public void VerifyLicenseHolderInformation()
        {
            // Document the test case details
            ExtentTestManager.GetTest().DocumentTest(
                objective: "Validate that additional billing fields appear when 'Same as Billing' checkbox is unchecked",
                steps: new[]
                {
                    "Navigate to purchase page and accept cookies",
                    "Select DevCraft Complete bundle",
                    "Add bundle to cart and verify navigation to cart page",
                    "Proceed as guest to contact info page",
                    "Verify 'Same as Billing' fields are not visible",
                    "Uncheck 'Same as Billing' checkbox",
                    "Verify additional fields become visible"
                },
                criteria: new[]
                {
                    "Additional fields are hidden by default",
                    "Unchecking the checkbox reveals additional fields",
                    "Form adjusts to collect license holder information"
                }
            );

            const ProductBundle selectedBundle = ProductBundle.DevCraftComplete;

            // Navigate to the purchase page and accept cookies
            _purchasePage.NavigateTo();
            _commonComponents.AcceptCookies();

            // Save the expected price and select the bundle
            var expectedPrice = _purchasePage.SaveBundlePrice(selectedBundle);
            _purchasePage.SelectBundle(selectedBundle);

            // Verify navigation to cart page and quantity
            Assert.Multiple(() =>
            {
                Assert.That(_purchasePage.VerifyNavigation(CartUrl), Is.True,
                    $"Failed to navigate to cart page. Current URL: {Driver.Url}");
                Assert.That(_cartPage.GetTotalQuantity(), Is.EqualTo(1));
            });

            // Proceed as a guest to the contact info page
            _cartPage.ContinueAsGuest();
            Assert.That(_purchasePage.VerifyNavigation(ContactInfoUrl), Is.True,
                $"Failed to navigate to contact info page. Current URL: {Driver.Url}");
            _commonComponents.AcceptCookies();

            // Verify that 'Same as Billing' fields are not visible by default
            var initialVisibility = _contactInfoPage.IsSameAsBillingFirstNameFieldVisible();
            Assert.That(initialVisibility, Is.False,
                "'Same as Billing' fields should not be visible by default");

            // Uncheck the 'Same as Billing' checkbox
            _contactInfoPage.UncheckSameAsBillingCheckbox();

            // Verify that additional fields become visible
            var afterUncheckVisibility = _contactInfoPage.IsSameAsBillingFirstNameFieldVisible();
            Assert.That(afterUncheckVisibility, Is.True,
                "Additional fields should be visible after unchecking 'Same as Billing' checkbox");

            // Log the results of the test
            ExtentTestManager.GetTest().LogResults(true, new Dictionary<string, string>
            {
                ["Initial 'Same as Billing' Field Visibility"] = initialVisibility ? "Visible" : "Hidden",
                ["After Unchecking Visibility"] = afterUncheckVisibility ? "Visible" : "Hidden",
                ["Checkbox State"] = "Unchecked"
            });
        }

        /// <summary>
        /// Verifies real-time validation of mandatory fields affecting the submit button state.
        /// </summary>
        [Test]
        [Description("Verify real-time validation of mandatory fields affecting submit button state")]
        [Category("Contact Info")]
        [Retry(2)]
        public void ValidatesCompanyField()
        {
            // Document the test case details
            ExtentTestManager.GetTest().DocumentTest(
                objective: "Validate real-time field validation and its effect on the submit button state",
                steps: new[]
                {
                    "Navigate to purchase page and accept cookies",
                    "Select DevCraft Complete bundle",
                    "Add bundle to cart and verify navigation to cart page",
                    "Proceed as guest to contact info page",
                    "Attempt to fill form with invalid email",
                    "Verify submit button remains disabled",
                    "Correct the email field",
                    "Verify submit button becomes enabled"
                },
                criteria: new[]
                {
                    "Submit button is disabled when mandatory fields are invalid",
                    "Real-time validation updates submit button state",
                    "Error messages appear appropriately for invalid inputs"
                }
            );

            const ProductBundle selectedBundle = ProductBundle.DevCraftComplete;

            // Navigate to the contact info page
            _purchasePage.NavigateTo();
            _commonComponents.AcceptCookies();

            // Save the expected price and select the bundle
            var expectedPrice = _purchasePage.SaveBundlePrice(selectedBundle);
            _purchasePage.SelectBundle(selectedBundle);

            // Verify navigation to cart page and quantity
            Assert.Multiple(() =>
            {
                Assert.That(_purchasePage.VerifyNavigation(CartUrl), Is.True,
                    $"Failed to navigate to cart page. Current URL: {Driver.Url}");
                Assert.That(_cartPage.GetTotalQuantity(), Is.EqualTo(1));
            });

            // Proceed as a guest to the contact info page
            _cartPage.ContinueAsGuest();
            Assert.That(_purchasePage.VerifyNavigation(ContactInfoUrl), Is.True,
                $"Failed to navigate to contact info page. Current URL: {Driver.Url}");
            _commonComponents.AcceptCookies();

            // Verify that the submit button is disabled initially
            Assert.That(_contactInfoPage.VerifyButtonIsEnabled(), Is.False,
                "Submit button should be disabled initially");

            // Fill contact info with an invalid email
            var contactData = ContactFormData.Default.Clone();
            contactData.Email = "InvalidEmail";
            _contactInfoPage.FillContactInfo(contactData);

            // Verify that the submit button remains disabled
            var buttonStateWithInvalidEmail = _contactInfoPage.VerifyButtonIsEnabled();
            Assert.That(buttonStateWithInvalidEmail, Is.False,
                "Submit button should be disabled with invalid email");

            // Correct the email field
            var validEmailData = new ContactFormData { Email = "Test@test.com" };
            _contactInfoPage.FillContactInfo(validEmailData);

            // Verify that the submit button becomes enabled
            var buttonStateWithValidEmail = _contactInfoPage.VerifyButtonIsEnabled();
            Assert.That(buttonStateWithValidEmail, Is.True,
                "Submit button should be enabled with valid email");

            // Log the results of the test
            ExtentTestManager.GetTest().LogResults(true, new Dictionary<string, string>
            {
                ["Submit Button State with Invalid Email"] = buttonStateWithInvalidEmail ? "Enabled" : "Disabled",
                ["Submit Button State with Valid Email"] = buttonStateWithValidEmail ? "Enabled" : "Disabled",
                ["Email Entered"] = contactData.Email
            });
        }

        /// <summary>
        /// Verifies that contact information persists correctly after proceeding to the next page.
        /// </summary>
        [Test]
        [Description("Verify contact information persists correctly after proceeding to next page")]
        [Category("Contact Info")]
        [Retry(2)]
        public void VerifyContactDataPersistence()
        {
            // Document the test case details
            ExtentTestManager.GetTest().DocumentTest(
                objective: "Validate that contact information persists correctly after navigating to the next page",
                steps: new[]
                {
                    "Navigate to purchase page and accept cookies",
                    "Select DevCraft Complete bundle",
                    "Add bundle to cart and verify navigation to cart page",
                    "Proceed as guest to contact info page",
                    "Fill in contact information form",
                    "Submit the form and proceed to review order page",
                    "Verify contact information on review order page matches input"
                },
                criteria: new[]
                {
                    "Contact information is correctly saved",
                    "Data persists across page navigation",
                    "All displayed contact details match the entered data"
                }
            );

            const ProductBundle selectedBundle = ProductBundle.DevCraftComplete;

            // Navigate to the contact info page
            _purchasePage.NavigateTo();
            _commonComponents.AcceptCookies();

            // Save the expected price and select the bundle
            var expectedPrice = _purchasePage.SaveBundlePrice(selectedBundle);
            _purchasePage.SelectBundle(selectedBundle);

            // Verify navigation to cart page and quantity
            Assert.Multiple(() =>
            {
                Assert.That(_cartPage.VerifyNavigation(CartUrl), Is.True,
                    $"Failed to navigate to cart page. Current URL: {Driver.Url}");
                Assert.That(_cartPage.GetTotalQuantity(), Is.EqualTo(1));
            });

            // Proceed as a guest to the contact info page
            _cartPage.ContinueAsGuest();
            Assert.That(_contactInfoPage.VerifyNavigation(ContactInfoUrl), Is.True,
                $"Failed to navigate to contact info page. Current URL: {Driver.Url}");
            _commonComponents.AcceptCookies();

            // Fill in the contact information form
            var expectedContactData = ContactFormData.Default.Clone();
            _contactInfoPage.FillContactInfo(expectedContactData);

            // Verify that the submit button is enabled and submit the form
            Assert.That(_contactInfoPage.VerifyButtonIsEnabled(), Is.True,
                "Submit button should be enabled");
            _contactInfoPage.SubmitContactInfo();

            // Verify navigation to the review order page
            Assert.That(_reviewOrderPage.VerifyNavigation(ReviewOrderUrl), Is.True,
                $"Failed to navigate to review order page. Current URL: {Driver.Url}");

            // Retrieve the actual contact information displayed
            var actualFullName = _reviewOrderPage.GetFullName();
            var actualEmail = _reviewOrderPage.GetEmail();
            var actualAddress = _reviewOrderPage.GetAddress();
            var actualCompany = _reviewOrderPage.GetCompany();
            var actualCity = _reviewOrderPage.GetCity();
            var actualCountry = _reviewOrderPage.GetCountry();

            // Verify that the contact information matches the entered data
            Assert.Multiple(() =>
            {
                Assert.That(actualFullName,
                    Is.EqualTo($"{expectedContactData.FirstName} {expectedContactData.LastName}"),
                    "Full name doesn't match entered values");
                Assert.That(actualEmail, Is.EqualTo(expectedContactData.Email),
                    "Email doesn't match entered value");
                Assert.That(actualAddress, Is.EqualTo(expectedContactData.Address),
                    "Address doesn't match entered value");
                Assert.That(actualCompany, Is.EqualTo(expectedContactData.Company),
                    "Company doesn't match entered value");
                Assert.That(actualCity, Is.EqualTo(expectedContactData.City),
                    "City doesn't match entered value");
                Assert.That(actualCountry, Is.EqualTo(expectedContactData.Country),
                    "Country doesn't match entered value");
            });

            // Log the results of the test
            ExtentTestManager.GetTest().LogResults(true, new()
            {
                ["Entered Full Name"] = $"{expectedContactData.FirstName} {expectedContactData.LastName}",
                ["Displayed Full Name"] = actualFullName,
                ["Entered Email"] = expectedContactData.Email,
                ["Displayed Email"] = actualEmail,
                ["Entered Address"] = expectedContactData.Address,
                ["Displayed Address"] = actualAddress,
                ["Entered Company"] = expectedContactData.Company,
                ["Displayed Company"] = actualCompany,
                ["Entered City"] = expectedContactData.City,
                ["Displayed City"] = actualCity,
                ["Entered Country"] = expectedContactData.Country,
                ["Displayed Country"] = actualCountry
            });
        }

        /// <summary>
        /// Cleans up after each test execution.
        /// </summary>
        [TearDown]
        public void TestCleanup()
        {
            try
            {
                if (_testStopwatch.IsRunning)
                {
                    ExtentTestManager.LogInfo($"""
                        Test Execution Summary:
                        • Duration: {_testStopwatch.ElapsedMilliseconds}ms
                        • Status: Completed
                        • Components: {("Initialized")}
                        """);
                }
            }
            catch (Exception ex)
            {
                ExtentTestManager.LogInfo($"""
                    Cleanup Operation Failed:
                    • Error: {ex.Message}
                    • Stack: {ex.StackTrace}
                    """);
            }
            finally
            {
                _testStopwatch.Reset();
            }
        }

        /// <summary>
        /// Performs any necessary cleanup after all tests in this class have run.
        /// </summary>
        [OneTimeTearDown]
        public void ClassCleanup()
        {
            ExtentTestManager.LogInfo("""
                =================================
                Contact Info Test Suite Execution Report
                =================================
                • All test cases completed
                • Resources released
                • Report generation triggered
                =================================
                """);
        }
    }
}
