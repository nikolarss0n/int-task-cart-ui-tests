namespace YourProject.UITests.Core.Reporting.Documentation;

using YourProject.UITests.Core.Reporting.Documentation.Templates;
using YourProject.UITests.Core.Reporting.Documentation.Models;

public static class TestDocumentation
{
    public static class Templates
    {
        public static string GenerateDocumentation(TestDoc doc) => 
            DocumentationTemplates.GenerateDocumentation(doc);
    }

    public static class Results
    {
        public static string GenerateResults(TestResult result) => 
            ResultsTemplates.GenerateResults(result);
    }
}