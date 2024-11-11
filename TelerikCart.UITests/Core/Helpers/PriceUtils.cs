using System.Text.RegularExpressions;

namespace TelerikCart.UITests.Core.Helpers
{
    /// <summary>
    /// Provides utility methods for handling price strings.
    /// </summary>
    public static class PriceUtils
    {
        /// <summary>
        /// Removes all non-numeric and non-period characters from a price string.
        /// </summary>
        /// <param name="price">The price string to normalize.</param>
        /// <returns>A normalized price string containing only numbers and periods.</returns>
        public static string NormalizePriceString(string price)
        {
            return Regex.Replace(price, @"[^0-9.]", "").Trim();
        }

        /// <summary>
        /// Parses a normalized price string into a decimal value.
        /// </summary>
        /// <param name="price">The price string to parse.</param>
        /// <returns>The parsed decimal value of the price.</returns>
        /// <exception cref="FormatException">Thrown when the price string cannot be parsed.</exception>
        public static decimal ParsePrice(string price)
        {
            var normalizedPrice = NormalizePriceString(price);
            if (!decimal.TryParse(normalizedPrice, out var result))
            {
                throw new FormatException($"Unable to parse price: {price}");
            }
            return result;
        }
    }
}