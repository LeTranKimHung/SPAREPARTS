using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using Xunit;
using System;

namespace SparePartsManagement.Tests.UI
{
    public class AuthTests : BaseTest
    {
        [Fact]
        public void Login_ValidAdmin_ShouldSucceed()
        {
            // LoginAsAdmin already waits for a successful login indicator (dropdown or sidebar)
            LoginAsAdmin();
            
            // Just double check we have some login indicator (either layout)
            var hasDropdown = Driver.FindElements(By.Id("userDropdown")).Count > 0;
            var hasSidebarLogout = Driver.FindElements(By.Id("logoutForm")).Count > 0;
            
            Assert.True(hasDropdown || hasSidebarLogout, "Should have either userDropdown (Public) or logoutForm (Admin) after login");
            
            Logout();
        }

        [Fact]
        public void Login_InvalidCredentials_ShouldProvideError()
        {
            Driver.Navigate().GoToUrl($"{BaseUrl}/Account/Login");
            
            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(10));
            var emailInput = wait.Until(ExpectedConditions.ElementIsVisible(By.Id("Email")));
            
            emailInput.SendKeys("invalid@account.com");
            Driver.FindElement(By.Id("Password")).SendKeys("WrongPassword123");
            
            var loginButton = Driver.FindElement(By.CssSelector("form[action='/Account/Login'] button[type='submit']"));
            JsClick(loginButton);

            // Wait for feedback in .text-danger areas, specifically the validation summary
            var errorElement = wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("form .text-danger")));
            Assert.True(errorElement.Displayed);
        }
    }
}
