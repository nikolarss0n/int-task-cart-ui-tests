# TelerikCart UI Tests

UI test automation solution for the Telerik Cart system, built with Selenium WebDriver and C#. This framework uses modern test automation practices, including the Page Object Model (POM) pattern, extensible reporting, and a modular test architecture.

## Architecture Overview

The test suite structure:

- **Page Objects**: Encapsulate page-specific interactions and validations.
- **Core Framework**: Contains base classes, utilities, and test infrastructure.
- **Test Cases**: Organized by functional areas with categorization.
- **Reporting**: Using ExtentReports for detailed test execution documentation.

## Technical Stack

- .NET 8.0+
- Selenium WebDriver
- NUnit Test Framework
- ExtentReports
- WebDriverManager

## Project Setup

### Prerequisites
- .NET 8.0 SDK or higher
- Chrome browser (latest version)
- IDE with .NET support (e.g., Visual Studio or JetBrains Rider)

### Installation
```bash
git clone https://github.com/nikolarss0n/int-task-cart-ui-tests.git
cd int-task-cart-ui-tests
dotnet restore
```

## Test Execution

Multiple options are available for executing different testing scenarios.

### Command Line Execution
Execute specific test categories:

```bash
# Run all tests
dotnet test --logger "console;verbosity=detailed"

# Smoke tests (tests categorized as 'Smoke')
dotnet test --filter "TestCategory~Smoke" --logger "console;verbosity=detailed"

# Bundle Purchase tests
dotnet test --filter "TestCategory~'Bundle Purchase'" --logger "console;verbosity=detailed"

# Contact Info tests
dotnet test --filter "TestCategory~'Contact Info'" --logger "console;verbosity=detailed"
```

Note: Use `TestCategory~` for partial matching of test categories, especially when tests have multiple categories.

### Configuration Options
Test behavior can be controlled via environment variables or command-line parameters:

```bash
# Run tests with the browser UI visible
TEST_HEADLESS=false dotnet test
```

## Test Reports

The framework generates detailed HTML reports using ExtentReports.

### Report Location
- **Report Path**: The reports are saved in the `TestResults/Report_[DATE_TIME]/` directory.
- **Accessing Reports**: After test execution, you can find the report at the path specified by `ExtentService.ReportPath`. This path is also logged in the test execution summary.

### Report Contents
Reports include:
- Execution summary
- Detailed test steps and logs
- Failure screenshots
- Execution metrics

## Custom Retry Mechanisms

The test framework includes a custom retry mechanism to handle transient failures and ensure test reliability.

### RetryUntilSuccess<T> Method

#### Overview
The `RetryUntilSuccess<T>` method is a generic utility that attempts to execute a specified action until it succeeds or a timeout is reached. It's particularly useful for handling flakiness due to asynchronous page updates or transient issues in UI tests.

#### Method Signature
```csharp
public T RetryUntilSuccess<T>(
    Func<T> action,
    Func<T, bool> validateResult,
    string operationName,
    TimeSpan? timeout = null,
    TimeSpan? interval = null)
```
- `action`: The operation to perform.
- `validateResult`: A function to validate the result of the action.
- `operationName`: A descriptive name of the operation for logging purposes.
- `timeout`: The maximum duration to keep retrying.
- `interval`: The wait time between retries.

#### Implementation
```csharp
public T RetryUntilSuccess<T>(
    Func<T> action,
    Func<T, bool> validateResult,
    string operationName,
    TimeSpan? timeout = null,
    TimeSpan? interval = null)
{
    timeout ??= TimeSpan.FromSeconds(10);
    interval ??= TimeSpan.FromMilliseconds(500);

    var stopwatch = Stopwatch.StartNew();
    var attempts = 0;
    Exception? lastException = null;

    while (stopwatch.Elapsed < timeout)
    {
        attempts++;
        try
        {
            Log($"Attempting {operationName}", $"Attempt {attempts}");
            var result = action();
            
            if (validateResult(result))
            {
                LogSuccess($"{operationName} succeeded", $"Attempt {attempts}, Duration: {stopwatch.ElapsedMilliseconds}ms");
                return result;
            }
            
            Log($"Validation failed for {operationName}", $"Attempt {attempts}");
        }
        catch (Exception ex) when (ex is StaleElementReferenceException
                                  || ex is NoSuchElementException
                                  || ex is ElementNotInteractableException)
        {
            lastException = ex;
            LogWarning($"{operationName} failed", $"Attempt {attempts}, Error: {ex.Message}");
        }

        if (stopwatch.Elapsed + interval.Value < timeout.Value)
        {
            Thread.Sleep(interval.Value);
        }
    }

    var errorMessage = $"{operationName} failed after {attempts} attempts ({stopwatch.ElapsedMilliseconds}ms)";
    LogError(errorMessage, lastException);
    throw new WebDriverTimeoutException(errorMessage, lastException);
}
```

### Usage Example
In the `CartPage` class, the `UpdateQuantity` method uses `RetryUntilSuccess` to ensure the price stabilizes after updating the quantity.

```csharp
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
```
## Project Structure

```
├── Core/
│   ├── Base/               # Framework foundation classes
│   │   ├── BasePage.cs
│   │   ├── BaseTest.cs
│   │   ├── CommonComponents.cs
│   │   └── DriverFactory.cs
│   ├── Data/               # Test data models and generators
│   │   └── ContactFormData.cs
│   ├── Helpers/            # Utility functions and extensions
│   │   ├── PriceUtils.cs
│   │   └── RetryHelper.cs  # Contains RetryUntilSuccess<T> method
│   └── Reporting/          # Reporting infrastructure
│       ├── Documentation/
│       │   ├── Extensions/
│       │   │   ├── DocumentationExtensions.cs
│       │   │   └── TechnicalLoggingExtension.cs
│       │   ├── Models/
│       │   │   ├── TestDoc.cs
│       │   │   └── TestResult.cs
│       │   ├── Templates/
│       │   │   ├── DocumentationTemplates.cs
│       │   │   └── ResultsTemplates.cs
│       │   └── TestDocumentation.cs
│       ├── ExtentService.cs
│       └── ExtentTestManager.cs
├── Pages/
│   ├── CartPage.cs
│   ├── ContactInfoPage.cs
│   ├── PurchasePage.cs
│   └── ReviewOrderPage.cs
├── Tests/
│   ├── BundleTests.cs
│   └── ContactInfoTests.cs
└── TestResults/
    └── Report_[DATE_TIME]/  # Generated test reports
```

### Core Directory
- **Helpers/**:
    - `PriceUtils.cs`: Contains methods that helps with price conversion.
- **Base/**: Contains base classes for pages and tests (`BasePage.cs`, `BaseTest.cs`, etc.).
- **Data/**: Houses data models and test data management (`ContactFormData.cs`).
- **Reporting/**: Implements ExtentReports configuration and custom reporters.
    - **Documentation/**: Extensions and templates for test documentation.

### Pages Directory
Page Object Model (POM) classes for the application under test:
- **CartPage.cs**
- **ContactInfoPage.cs**
- **PurchasePage.cs**
- **ReviewOrderPage.cs**

### Tests Directory
Contains test classes organized by functionality:
- **BundleTests.cs**: Tests related to bundle purchases.
- **ContactInfoTests.cs**: Tests for the contact information flow.

## Build Configuration

The project uses modern C# features and nullable reference types:

```xml
<PropertyGroup>
  <TargetFramework>net8.0</TargetFramework>
  <ImplicitUsings>enable</ImplicitUsings>
  <Nullable>enable</Nullable>
  <IsTestProject>true</IsTestProject>
</PropertyGroup>
```

## Known Issues and Troubleshooting

### Test Filtering with Categories
When filtering tests by category, use the `~` operator for partial matching, especially if tests have multiple categories:
```bash
dotnet test --filter "TestCategory~Smoke"
```

### Viewing Executed Tests
To see which tests are executed during the test run, use the `--logger` option with increased verbosity:
```bash
dotnet test --filter "TestCategory~Smoke" --logger "console;verbosity=detailed"
```

### Report Generation
If the test report is not being generated:
- Ensure that `ExtentService.Flush()` is called after all tests have run.
- Check that the report path is correctly configured in `ExtentService.cs`.
- Verify that your tests are not exiting prematurely due to unhandled exceptions.

### Parallel Test Execution
Parallel execution might interfere with report generation or test reliability.

