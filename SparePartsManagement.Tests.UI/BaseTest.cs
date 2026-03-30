using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using Xunit;
using System;
using System.Linq;

namespace SparePartsManagement.Tests.UI
{
    public class BaseTest : IDisposable
    {
        protected IWebDriver Driver;
        protected string BaseUrl = "http://localhost:5132";

        public BaseTest()
        {
            var options = new ChromeOptions();
            options.AddArgument("--headless"); 
            options.AddArgument("--window-size=1920,1080");
            options.AddArgument("--disable-gpu");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");

            Driver = new ChromeDriver(options);
            Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
        }

        protected void LoginAsAdmin()
        {
            Driver.Navigate().GoToUrl($"{BaseUrl}/Account/Login");
            
            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(15));
            try {
                var emailInput = wait.Until(ExpectedConditions.ElementIsVisible(By.Id("Email")));
                
                emailInput.Clear();
                emailInput.SendKeys("admin@spareparts.com");

                var passwordInput = Driver.FindElement(By.Id("Password"));
                passwordInput.Clear();
                passwordInput.SendKeys("Admin123");

                var loginButton = Driver.FindElement(By.CssSelector("form[action='/Account/Login'] button[type='submit']"));
                JsClick(loginButton);

                // Wait for URL change
                wait.Until(d => !d.Url.Contains("/Account/Login"));
                
                // Check for either Public Layout (#userDropdown) or Admin Layout (#logoutForm)
                wait.Until(d => d.FindElements(By.Id("userDropdown")).Count > 0 || d.FindElements(By.Id("logoutForm")).Count > 0);
            } catch (Exception ex) {
                string errorText = "";
                try {
                     var errorElements = Driver.FindElements(By.CssSelector(".text-danger"));
                     errorText = string.Join(" | ", errorElements.Select(e => e.Text).Where(t => !string.IsNullOrEmpty(t) && t != "PARTS"));
                } catch { }
                
                throw new Exception($"LoginAsAdmin failed. URL: {Driver.Url}. Errors found: {errorText}. Detail: {ex.Message}");
            }
        }

        protected void Logout()
        {
            try {
                // Public layout
                var userDropdown = Driver.FindElements(By.Id("userDropdown")).FirstOrDefault();
                if (userDropdown != null && userDropdown.Displayed) {
                    JsClick(userDropdown);
                    var logoutButtonPublic = Driver.FindElements(By.CssSelector("form[action='/Account/Logout'] button")).FirstOrDefault();
                    if (logoutButtonPublic != null) JsClick(logoutButtonPublic);
                } 
                // Admin layout
                else {
                    var logoutButtonAdmin = Driver.FindElements(By.CssSelector("#logoutForm button")).FirstOrDefault();
                    if (logoutButtonAdmin != null) JsClick(logoutButtonAdmin);
                }
            } catch { }
        }

        protected void JsClick(IWebElement element)
        {
            IJavaScriptExecutor executor = (IJavaScriptExecutor)Driver;
            executor.ExecuteScript("arguments[0].scrollIntoView({block: 'center', inline: 'nearest'});", element);
            System.Threading.Thread.Sleep(500); 
            executor.ExecuteScript("arguments[0].click();", element);
        }

        public void Dispose()
        {
            Driver.Quit();
        }
    }
}
