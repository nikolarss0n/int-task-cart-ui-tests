using OpenQA.Selenium;
using System.Text.RegularExpressions;
using TelerikCart.UITests.Core.Base;

namespace TelerikCart.UITests.Pages
{
    /// <summary>
    /// Provides methods for interacting with the shopping cart page, including navigation,
    /// item quantity updates, support period selection, price retrieval, product removal,
    /// and verification of cart messages and state.
    /// </summary>
    public class CartPage : BasePage
    {
        private const string PageUrl = "https://store.progress.com/your-order";
        private readonly CommonComponents _commonComponents;
        private decimal _lastPrice;

        // Locators
        private readonly By _pricePerLicense = By.CssSelector(".e2e-price-per-license");
        private readonly By _licenseTotalPrice = By.CssSelector(".e2e-total-price");
        private readonly By _quantityValue = By.CssSelector("td[data-label='Licenses'] .k-input-value-text");
        private readonly By _periodDropdownButton = By.CssSelector("period-select kendo-dropdownlist .k-input-button");
        private readonly By _removeProductButton = By.CssSelector(".e2e-delete-item");
        private readonly By _emptyCartHeading = By.CssSelector(".e2e-empty-shopping-cart-heading");
        private readonly By _continueAsGuestButton = By.CssSelector(".e2e-continue");
        private readonly By _updateLicenseQuantity = By.CssSelector("kendo-dropdownlist.dropdown--small");

        /// <summary>
        /// Specifies the support period options available, along with their associated discounts.
        /// </summary>
        public enum PeriodOption
        {
            /// <summary>1 year included with no additional discount.</summary>
            OneYearIncluded = 0, 
            /// <summary>Plus 1 year with a 5% discount.</summary>
            PlusOneYear = 5, 
            /// <summary>Plus 2 years with an 8% discount.</summary>
            PlusTwoYears = 8,
            /// <summary>Plus 3 years with an 11% discount.</summary>
            PlusThreeYears = 11,
            /// <summary>Plus 4 years with a 14% discount.</summary>
            PlusFourYears = 14
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CartPage"/> class.
        /// </summary>
        /// <param name="driver">The WebDriver instance to interact with the browser.</param>
        public CartPage(IWebDriver driver) : base(driver, "Cart Page")
        {
            _commonComponents = new CommonComponents(driver);
        }

        /// <summary>
        /// Navigates to the Cart page and waits for the page to load.
        /// </summary>
        public void NavigateTo()
        {
            NavigateToUrl(PageUrl);
            WaitForPageLoad();
        }

        /// <summary>
        /// Continues the checkout process as a guest user.
        /// </summary>
        public void ContinueAsGuest()
        {
            WaitAndClick(_continueAsGuestButton, "Continue as guest");
        }

        /// <summary>
        /// Gets the current price per license displayed on the cart page.
        /// </summary>
        /// <returns>The price per license as a decimal.</returns>
        /// <exception cref="FormatException">Thrown when the price cannot be parsed.</exception>
        public decimal GetCurrentPrice()
        {
            try
            {
                var priceElement = WaitAndFindElement(_pricePerLicense, "Price per license");
                var priceText = priceElement.Text;
                var price = ParsePrice(priceText);
                LogSuccess("Got current price", $"{price:C}");
                return price;
            }
            catch (Exception ex)
            {
                LogError("Failed to get current price", ex);
                throw;
            }
        }

        /// <summary>
        /// Gets the total price displayed on the cart page.
        /// </summary>
        /// <returns>The total price as a decimal.</returns>
        /// <exception cref="FormatException">Thrown when the total price cannot be parsed.</exception>
        public decimal GetTotalPrice()
        {
            try
            {
                var priceElement = WaitAndFindElement(_licenseTotalPrice, "Total price");
                var priceText = priceElement.Text;
                var price = ParsePrice(priceText);
                LogSuccess("Got total price", $"{price:C}");
                return price;
            }
            catch (Exception ex)
            {
                LogError("Failed to get total price", ex);
                throw;
            }
        }

        /// <summary>
        /// Gets the total quantity of licenses in the cart.
        /// </summary>
        /// <returns>The total quantity as an integer.</returns>
        /// <exception cref="FormatException">Thrown when the quantity cannot be parsed.</exception>
        public int GetTotalQuantity()
        {
            try
            {
                var quantityElements = WaitAndFindElements(_quantityValue, "License quantity values");
                var total = quantityElements.Sum(element => int.Parse(element.Text));
                LogSuccess("Got total quantity", total.ToString());
                return total;
            }
            catch (Exception ex)
            {
                LogError("Failed to get total quantity", ex);
                throw;
            }
        }

        /// <summary>
        /// Selects the support period option from the dropdown.
        /// </summary>
        /// <param name="period">The <see cref="PeriodOption"/> to select.</param>
        /// <exception cref="ArgumentException">Thrown when an unsupported period option is provided.</exception>
        public void SelectPeriod(PeriodOption period)
        {
            var (displayText, searchText) = GetDisplayAndSearchText(period);
            SelectKendoDropDownListOption(_periodDropdownButton, searchText);
        }

        /// <summary>
        /// Updates the quantity of licenses in the cart.
        /// </summary>
        /// <param name="quantity">The desired quantity to set.</param>
        /// <exception cref="Exception">Thrown when the quantity update fails.</exception>
        public void UpdateQuantity(int quantity)
        {
            try
            {
                Log("Updating quantity", quantity.ToString());

                // Use the common SelectKendoDropDownListOption method
                SelectKendoDropDownListOption(
                    _updateLicenseQuantity,
                    quantity.ToString()
                );

                // Use the common retry mechanism for price stabilization
                _commonComponents.RetryUntilSuccess(
                    CheckPriceStability,
                    isStable => isStable,
                    "Wait for price stabilization"
                );

                LogSuccess("Updated quantity", quantity.ToString());
            }
            catch (Exception ex)
            {
                LogError($"Failed to update quantity to {quantity}", ex);
                TakeScreenshot("QuantityUpdateFailure");
                throw;
            }
        }

        /// <summary>
        /// Removes the product from the cart.
        /// </summary>
        /// <exception cref="Exception">Thrown when the product removal fails.</exception>
        public void RemoveProduct()
        {
            try
            {
                WaitAndClick(_removeProductButton, "Remove product");
                LogSuccess("Removed product");
            }
            catch (Exception ex)
            {
                LogError("Failed to remove product", ex);
                throw;
            }
        }

        /// <summary>
        /// Verifies that the empty cart message matches the expected message.
        /// </summary>
        /// <param name="expectedMessage">The expected empty cart message.</param>
        /// <returns><c>true</c> if the message matches; otherwise, <c>false</c>.</returns>
        public bool VerifyEmptyCartMessage(string expectedMessage)
        {
            bool match = VerifyElementText(_emptyCartHeading, expectedMessage, "empty cart heading", exact: true);

            if (match)
            {
                LogSuccess("Empty cart message verified", $"'{expectedMessage}'");
            }
            else
            {
                LogWarning("Empty cart message mismatch", $"Expected: '{expectedMessage}', Actual: '{GetEmptyCartMessage()}'");
            }

            return match;
        }

        /// <summary>
        /// Gets the empty cart message displayed on the cart page.
        /// </summary>
        /// <returns>The empty cart message as a string.</returns>
        public string GetEmptyCartMessage()
        {
            var text = GetElementText(_emptyCartHeading, "empty cart heading");
            LogSuccess("Got empty cart message", text);
            return text;
        }

        /// <summary>
        /// Normalizes a price string by removing any non-numeric characters except for the decimal point.
        /// </summary>
        /// <param name="price">The price string to normalize.</param>
        /// <returns>The normalized price string.</returns>
        private static string NormalizePriceString(string price)
        {
            return Regex.Replace(price, @"[^0-9.]", "").Trim();
        }

        /// <summary>
        /// Parses a price string into a decimal value.
        /// </summary>
        /// <param name="price">The price string to parse.</param>
        /// <returns>The parsed price as a decimal.</returns>
        /// <exception cref="FormatException">Thrown when the price cannot be parsed.</exception>
        private static decimal ParsePrice(string price)
        {
            var normalizedPrice = NormalizePriceString(price);
            if (!decimal.TryParse(normalizedPrice, out var result))
            {
                throw new FormatException($"Unable to parse price: {price}");
            }
            return result;
        }

        /// <summary>
        /// Gets the display and search text for the specified support period option.
        /// </summary>
        /// <param name="period">The <see cref="PeriodOption"/> to get text for.</param>
        /// <returns>A tuple containing the display text and search text.</returns>
        /// <exception cref="ArgumentException">Thrown when an unsupported period option is provided.</exception>
        private (string displayText, string searchText) GetDisplayAndSearchText(PeriodOption period)
        {
            return period switch
            {
                PeriodOption.OneYearIncluded => ("1 year included", "1 year"),
                PeriodOption.PlusOneYear => ("+1 year", "+1 year"),
                PeriodOption.PlusTwoYears => ("+2 years", "+2 years"),
                PeriodOption.PlusThreeYears => ("+3 years", "+3 years"),
                PeriodOption.PlusFourYears => ("+4 years", "+4 years"),
                _ => throw new ArgumentException($"Unsupported period option: {period}", nameof(period))
            };
        }

        /// <summary>
        /// Checks whether the total price has stabilized after updating the quantity or period.
        /// </summary>
        /// <returns><c>true</c> if the price has stabilized; otherwise, <c>false</c>.</returns>
        private bool CheckPriceStability()
        {
            var currentPrice = GetTotalPrice();
            if (_lastPrice == 0)
            {
                _lastPrice = currentPrice;
                return false;
            }

            if (currentPrice == _lastPrice)
            {
                Thread.Sleep(300);
                var verificationPrice = GetTotalPrice();
                return verificationPrice == currentPrice;
            }

            _lastPrice = currentPrice;
            return false;
        }
    }
}
