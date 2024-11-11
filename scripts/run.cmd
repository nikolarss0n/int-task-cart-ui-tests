:: run-tests.cmd
@echo off
echo Running all tests...
dotnet msbuild -t:RunTests
pause

:: run-tests-headed.cmd
@echo off
echo Running tests in headed mode...
dotnet msbuild -t:TestHeaded
pause

:: run-tests-with-report.cmd
@echo off
echo Running tests with report...
dotnet msbuild -t:TestWithReport
pause

:: run-bundle-tests.cmd
@echo off
echo Running bundle purchase tests...
dotnet msbuild -t:TestBundlePurchase
pause

:: run-smoke-tests.cmd
@echo off
echo Running smoke tests...
dotnet msbuild -t:RunTests /p:TestCategory="Smoke"
pause