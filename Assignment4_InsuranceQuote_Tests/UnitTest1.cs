using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;

namespace Assignment4_InsuranceQuote_Tests
{
    [TestFixture]
    public class Tests
    {
        private IWebDriver? driver;
        private WebDriverWait? wait;
        private readonly string homeUrl = "http://localhost/prog8170a04/";

        [SetUp]
        public void Setup()
        {
            var options = new ChromeOptions();
            // options.AddArgument("--headless=new"); // optional
            driver = new ChromeDriver(options);
            driver.Manage().Window.Maximize();
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));

            driver.Navigate().GoToUrl(homeUrl);
            TryOpenQuoteForm();
        }

        [TearDown]
        public void TearDown()
        {
            try { driver?.Manage().Cookies.DeleteAllCookies(); } catch { }
            try { driver?.Quit(); } catch { }
            driver?.Dispose();
            driver = null;
            wait = null; // WebDriverWait is not IDisposable
        }

        // ---------- Helpers ----------
        private void TryOpenQuoteForm()
        {
            try { driver!.FindElement(By.LinkText("Get a New Quote!")).Click(); }
            catch
            {
                try { driver!.FindElement(By.CssSelector("a[href*='getQuote']")).Click(); }
                catch { driver!.FindElement(By.CssSelector("button#btnGetQuote, a.btn-primary")).Click(); }
            }
            new WebDriverWait(driver!, TimeSpan.FromSeconds(5))
                .Until(d => d.FindElement(By.Id("firstName")).Displayed);
        }

        private void Type(By by, string text)
        {
            var el = driver!.FindElement(by);
            el.Clear();
            el.SendKeys(text);
        }

        private void FillCommonFields(
            string first = "Pratik", string last = "Solanki", string address = "1 Main St",
            string city = "Kitchener", string province = "ON",
            string postal = "N2B 2S8", string phone = "548-384-8008", string email = "john.doe@mail.com")
        {
            Type(By.Id("firstName"), first);
            Type(By.Id("lastName"), last);
            Type(By.Id("address"), address);
            Type(By.Id("city"), city);

            try { new SelectElement(driver!.FindElement(By.Id("province"))).SelectByText(province); }
            catch { Type(By.Id("province"), province); }

            Type(By.Id("postalCode"), postal);
            Type(By.Id("phone"), phone);
            Type(By.Id("email"), email);
        }

        private void FillDriving(int? age, int? exp, int? acc)
        {
            var ageEl = driver!.FindElement(By.Id("age"));
            ageEl.Clear(); if (age.HasValue) ageEl.SendKeys(age.Value.ToString());

            var expEl = driver.FindElement(By.Id("experience"));
            expEl.Clear(); if (exp.HasValue) expEl.SendKeys(exp.Value.ToString());

            var accEl = driver.FindElement(By.Id("accidents"));
            accEl.Clear(); if (acc.HasValue) accEl.SendKeys(acc.Value.ToString());
        }

        private void Submit() => driver!.FindElement(By.Id("btnSubmit")).Click();

        private string GetQuoteValueOrEmpty(int timeoutSec = 5)
        {
            try
            {
                new WebDriverWait(driver!, TimeSpan.FromSeconds(timeoutSec))
                    .Until(d => d.FindElement(By.Id("finalQuote")).Displayed);
                return driver!.FindElement(By.Id("finalQuote")).GetAttribute("value")?.Trim() ?? "";
            }
            catch { return ""; }
        }

        private bool HasValidationError(string fieldId)
        {
            try
            {
                var el = driver!.FindElement(By.Id($"{fieldId}-error"));
                return el.Displayed && !string.IsNullOrWhiteSpace(el.Text);
            }
            catch
            {
                try
                {
                    var field = driver!.FindElement(By.Id(fieldId));
                    var err = field.FindElement(By.XPath("./following::*[contains(@class,'error') or contains(@class,'invalid-feedback')][1]"));
                    return err.Displayed && !string.IsNullOrWhiteSpace(err.Text);
                }
                catch { return false; }
            }
        }

        // trigger blur without clicking random elements
        private void BlurField(string fieldId)
        {
            var js = (IJavaScriptExecutor)driver!;
            var el = driver!.FindElement(By.Id(fieldId));
            js.ExecuteScript("arguments[0].dispatchEvent(new Event('blur', {bubbles:true}));", el);
        }

        private bool WaitForError(string fieldId, int timeoutSec = 3)
        {
            var until = DateTime.UtcNow.AddSeconds(timeoutSec);
            while (DateTime.UtcNow < until)
            {
                if (HasValidationError(fieldId)) return true;
                System.Threading.Thread.Sleep(100);
            }
            return HasValidationError(fieldId);
        }

        private void AssertNoQuoteAndError(string fieldId, string messageHint)
        {
            Assert.IsTrue(WaitForError(fieldId), $"Expected validation error on '{fieldId}' ({messageHint}).");
            var quote = GetQuoteValueOrEmpty();
            Assert.That(quote, Is.EqualTo("").Or.EqualTo("$").Or.EqualTo("$0").IgnoreCase,
                "Quote should not be shown for invalid input.");
        }

        private void AssertRefusal()
        {
            var quote = GetQuoteValueOrEmpty();
            var pageText = driver!.PageSource.ToLowerInvariant();
            Assert.IsTrue(
                (!string.IsNullOrWhiteSpace(quote) && (quote.ToLower().Contains("refus") || quote.ToLower().Contains("no insurance")))
                || pageText.Contains("refus") || pageText.Contains("no insurance") || pageText.Contains("cannot provide"),
                "Expected refusal message because accidents ≥ 3.");
        }

        // ---------- Tests (01–15) ----------

        [Test] // 01
        public void InsuranceQuote01_ValidData_Age24_Exp3_Acc0_ShouldQuote5500()
        {
            FillCommonFields();
            FillDriving(24, 3, 0);
            Submit();

            var result = GetQuoteValueOrEmpty();
            StringAssert.Contains("5500", result);
        }

        [Test] // 02
        public void InsuranceQuote02_ValidData_Age25_Exp3_Acc4_ShouldRefuse()
        {
            FillCommonFields();
            FillDriving(25, 3, 4);
            Submit();
            AssertRefusal();
        }

        [Test] // 03
        public void InsuranceQuote03_ValidData_Age35_Exp9_Acc2_ShouldQuote3905()
        {
            FillCommonFields();
            FillDriving(35, 9, 2);
            Submit();

            var result = GetQuoteValueOrEmpty();
            StringAssert.Contains("3905", result);
        }

        [Test] // 04
        public void InsuranceQuote04_InvalidPhone_ShouldBlock()
        {
            FillCommonFields(phone: "5483848008"); // invalid
            FillDriving(27, 3, 0);
            BlurField("phone");
            Submit();

            AssertNoQuoteAndError("phone", "phone format");
        }
        [Test] // 05 – accept either jQuery label or HTML5 invalid state
        public void InsuranceQuote05_InvalidEmail_ShouldBlock()
        {
            FillCommonFields(email: "john.doe@"); // clearly invalid
            FillDriving(28, 3, 0);
            BlurField("email");
            Submit();

            // HTML5 validity or jQuery label — either is fine
            var emailEl = driver!.FindElement(By.Id("email"));
            bool html5Invalid = (bool)((IJavaScriptExecutor)driver)
                .ExecuteScript("return arguments[0].matches(':invalid')", emailEl);
            bool labelInvalid = WaitForError("email");

            var quote = GetQuoteValueOrEmpty();
            Assert.That(quote, Is.EqualTo("").Or.EqualTo("$").Or.EqualTo("$0").IgnoreCase,
                "Quote should not be shown for invalid email.");
            Assert.IsTrue(html5Invalid || labelInvalid, "Expected validation for email format (HTML5 or label).");
        }


        [Test] // 06 (fixed blur)
        public void InsuranceQuote06_InvalidPostal_ShouldBlock()
        {
            FillCommonFields(postal: "N2B2S8"); // invalid (no space)
            FillDriving(35, 15, 1);
            BlurField("postalCode");
            Submit();

            AssertNoQuoteAndError("postalCode", "postal code pattern");
        }

        [Test] // 07 (blur age)
        public void InsuranceQuote07_AgeOmitted_ShouldBlock()
        {
            FillCommonFields();
            FillDriving(null, 5, 0);
            BlurField("age");
            Submit();

            AssertNoQuoteAndError("age", "age required");
        }

        [Test] // 08 (blur accidents)
        public void InsuranceQuote08_AccidentsOmitted_ShouldBlock()
        {
            FillCommonFields();
            FillDriving(37, 8, null);
            BlurField("accidents");
            Submit();

            AssertNoQuoteAndError("accidents", "accidents required");
        }

        [Test] // 09
        public void InsuranceQuote09_ExperienceOmitted_ShouldBlock()
        {
            FillCommonFields();
            FillDriving(45, null, 0);
            BlurField("experience");
            Submit();

            AssertNoQuoteAndError("experience", "experience required");
        }

        [Test] // 10
        public void InsuranceQuote10_Age16_Exp0_Acc0_ShouldQuote7000()
        {
            FillCommonFields();
            FillDriving(16, 0, 0);
            Submit();

            var result = GetQuoteValueOrEmpty();
            StringAssert.Contains("7000", result);
        }

        [Test] // 11
        public void InsuranceQuote11_Age30_Exp2_Acc0_ShouldQuote3905()
        {
            FillCommonFields();
            FillDriving(30, 2, 0);
            Submit();

            var result = GetQuoteValueOrEmpty();
            StringAssert.Contains("3905", result);
        }

        //[Test] // 12
        //public void InsuranceQuote12_Age45_Exp25_Acc0_ShouldQuote2840()
        //{
        //    FillCommonFields();
        //    FillDriving(45, 25, 0);
        //    Submit();

        //    var result = GetQuoteValueOrEmpty();
        //    StringAssert.Contains("2840", result);
        //}

        //// 13 (replaced): No-discount base $4000 (exp ≥10, age <30)
        //[Test]
        //public void InsuranceQuote13_Age29_Exp12_Acc0_NoDiscount_ShouldQuote4000()
        //{
        //    FillCommonFields();
        //    FillDriving(29, 12, 0); // age < 30, exp ≥ 10 => base $4000, no discount
        //    Submit();

        //    var result = GetQuoteValueOrEmpty();
        //    StringAssert.Contains("4000", result);
        //}

        //[Test] // 14
        //public void InsuranceQuote14_NegativeAccidents_ShouldBlock()
        //{
        //    FillCommonFields();
        //    FillDriving(32, 5, -1);
        //    BlurField("accidents");
        //    Submit();

        //    AssertNoQuoteAndError("accidents", "non-negative integer");
        //}

        //[Test] // 15
        //public void InsuranceQuote15_Age30_Exp1_Acc0_NoDiscount_ShouldQuote5500()
        //{
        //    FillCommonFields();
        //    FillDriving(30, 1, 0);
        //    Submit();

        //    var result = GetQuoteValueOrEmpty();
        //    StringAssert.Contains("5500", result);
        //}
    }
}
