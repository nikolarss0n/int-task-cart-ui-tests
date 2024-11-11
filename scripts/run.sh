#!/bin/bash
# run-tests.sh
echo "Running all tests..."
dotnet msbuild -t:RunTests

# run-tests-headed.sh
echo "Running tests in headed mode..."
dotnet msbuild -t:TestHeaded

# run-tests-with-report.sh
echo "Running tests with report..."
dotnet msbuild -t:TestWithReport

# run-bundle-tests.sh
echo "Running bundle purchase tests..."
dotnet msbuild -t:TestBundlePurchase

# run-smoke-tests.sh
echo "Running smoke tests..."
dotnet msbuild -t:RunTests /p:TestCategory="Smoke"