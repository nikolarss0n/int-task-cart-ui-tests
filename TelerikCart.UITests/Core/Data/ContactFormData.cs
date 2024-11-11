public class ContactFormData
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? Company { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? CountryTaxIdentificationNumber { get; set; }

    public static ContactFormData Default => new()
    {
        FirstName = "FirstName",
        LastName = "LastName",
        Email = "Test@test.com",
        Phone = "+1234567890",
        Address = "Address",
        Company = "Company",
        City = "Sofia",
        Country = "Bulgaria",
        CountryTaxIdentificationNumber = "123456789"
    };

    // Add a clone method
    public ContactFormData Clone()
    {
        return new ContactFormData
        {
            FirstName = this.FirstName,
            LastName = this.LastName,
            Email = this.Email,
            Phone = this.Phone,
            Address = this.Address,
            Company = this.Company,
            City = this.City,
            Country = this.Country,
            CountryTaxIdentificationNumber = this.CountryTaxIdentificationNumber
        };
    }
}