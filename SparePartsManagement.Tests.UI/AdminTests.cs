using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using Xunit;
using System;
using System.Linq;

namespace SparePartsManagement.Tests.UI
{
    public class AdminTests : BaseTest
    {
        [Fact]
        public void AdminDashboard_ShouldLoadAfterLogin()
        {
            LoginAsAdmin();
            Driver.Navigate().GoToUrl($"{BaseUrl}/Admin/Products/Index");
            
            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(10));
            wait.Until(ExpectedConditions.ElementExists(By.Id("logoutForm")));
            
            // Layout specific check: title exists in the header <h5>
            Assert.Contains("Quản lý Phụ tùng", Driver.PageSource);
        }

        [Fact]
        public void CreateProduct_ShouldDisplayInList()
        {
            LoginAsAdmin();
            Driver.Navigate().GoToUrl($"{BaseUrl}/Admin/Products/Create");

            string productName = "UI-TEST-" + Guid.NewGuid().ToString().Substring(0, 8);
            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(10));
            
            var nameInput = wait.Until(ExpectedConditions.ElementIsVisible(By.Id("Name")));
            nameInput.SendKeys(productName);
            
            // Wait for categories to be available (seeded in Program.cs)
            var categorySelect = new SelectElement(Driver.FindElement(By.Id("CategoryId")));
            categorySelect.SelectByIndex(1); 

            var brandSelect = new SelectElement(Driver.FindElement(By.Id("BrandId")));
            brandSelect.SelectByIndex(1);

            var priceInput = Driver.FindElement(By.Id("Price"));
            priceInput.Clear();
            priceInput.SendKeys("150000");

            var stockInput = Driver.FindElement(By.Id("StockQuantity"));
            stockInput.Clear();
            stockInput.SendKeys("50");

            var submitButton = Driver.FindElement(By.CssSelector("form[action='/Admin/Products/Create'] button[type='submit']"));
            JsClick(submitButton);

            // Redirection: Check if it reaches the index page. We check for containment to be more flexible.
            wait.Until(d => d.Url.ToLower().Contains("/admin/products"));

            // Check if product was created
            Assert.Contains(productName, Driver.PageSource);
        }
    }
}
