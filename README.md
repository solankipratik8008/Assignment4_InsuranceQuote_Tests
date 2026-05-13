# Insurance Quote Automated Testing - NUnit & Selenium

This repository contains automated UI tests for an Insurance Quote web application. The test suite was built using C#, NUnit, Selenium WebDriver, and ChromeDriver to validate quote calculations, input validation, and refusal scenarios.

## Project Overview

The purpose of this project is to test an insurance quote form running locally at:

```text
http://localhost/prog8170a04/
The automated tests open the quote form in Chrome, enter customer and driving information, submit the form, and verify whether the application returns the correct quote amount or blocks invalid input.

Technologies Used
C#
NUnit
Selenium WebDriver
ChromeDriver
WebDriverWait
Visual Studio
Git / GitHub
Key Testing Areas
Valid insurance quote calculations
Refusal logic for high accident count
Phone number validation
Email validation
Postal code validation
Required field validation
Age, experience, and accident input validation
Negative accident value validation
UI interaction using Selenium WebDriver
Browser setup and teardown using NUnit lifecycle methods
Test Coverage

The test suite includes 15 automated tests covering both positive and negative scenarios.

Examples include:

Valid driver quote: Age 24, experience 3 years, 0 accidents should return 5500
Driver refusal: Accident count greater than or equal to 3 should show a refusal message
Invalid phone number should block quote generation
Invalid email should block quote generation
Invalid postal code should block quote generation
Missing age, experience, or accident fields should show validation errors
Negative accident count should be blocked
Different age and experience combinations should return expected quote values
Features
Automated Chrome browser testing
Reusable helper methods for form input
Explicit waits for reliable UI testing
Validation error detection
Quote result verification
Clean browser teardown after each test
Tests for both successful and invalid form submissions
Project Structure
Assignment4_InsuranceQuote_Tests/
│
├── Tests.cs
├── Assignment4_InsuranceQuote_Tests.csproj
├── Assignment4_InsuranceQuote_Tests.sln
├── .gitignore
└── README.md
How to Run
Clone this repository:
git clone https://github.com/solankipratik8008/Assignment4_InsuranceQuote_Tests.git
Open the solution file in Visual Studio:
Assignment4_InsuranceQuote_Tests.sln
Make sure the Insurance Quote web application is running locally:
http://localhost/prog8170a04/
Restore NuGet packages if needed.
Run the tests using Visual Studio Test Explorer.
Important Notes
ChromeDriver must be installed or available through the Selenium WebDriver package setup.
The tested web application must be running before executing the test suite.
The test URL can be updated in the homeUrl variable inside the test class if the application runs on a different local path.
What I Learned

Through this project, I practiced writing automated UI tests for a real web form and learned how to:

Use NUnit test lifecycle methods such as [SetUp] and [TearDown]
Automate browser interactions with Selenium WebDriver
Use explicit waits for dynamic UI elements
Validate form errors and expected output values
Structure reusable helper methods for cleaner test code
Test both valid and invalid user input scenarios
Improve confidence in application quality through automated testing
Author

Pratik Kumar Solanki
GitHub: https://github.com/solankipratik8008


## Best resume line for this project

For future resumes, use this:

```text
Insurance Quote Test Automation - C#, NUnit, Selenium WebDriver
Built 15 automated browser tests for an insurance quote web app; validated quot
