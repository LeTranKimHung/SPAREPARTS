using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using Xunit;
using System;
using System.Linq;

namespace SparePartsManagement.Tests.UI
{
    public class PublicTests : BaseTest
    {
        [Fact]
        public void HomePage_LoadsSuccessfully()
        {
            Driver.Navigate().GoToUrl(BaseUrl);
            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(10));
            
            // Wait for body to be loaded
            wait.Until(d => d.FindElement(By.TagName("body")));
            
            Assert.Contains("PHỤ TÙNG XE", Driver.PageSource.ToUpper());
            
            var buyNowButton = Driver.FindElement(By.CssSelector("a[href='/PublicProduct']"));
            Assert.True(buyNowButton.Displayed);
        }

        [Fact]
        public void SearchProduct_WithKeywords_ShouldNavigateToResults()
        {
            Driver.Navigate().GoToUrl(BaseUrl);
            
            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(10));
            var searchInput = wait.Until(ExpectedConditions.ElementIsVisible(By.Name("keyword")));
            searchInput.SendKeys("Lọc gió");
            
            var searchButton = Driver.FindElement(By.CssSelector("form[action='/PublicProduct'] button[type='submit']"));
            JsClick(searchButton);

            wait.Until(d => d.Url.Contains("/PublicProduct") && d.Url.Contains("keyword=L%E1%BB%8Dc+gi%C3%B3"));

            Assert.Contains("/PublicProduct", Driver.Url);
        }

        [Fact]
        public void AddToCart_ShouldIncreaseCount()
        {
            Driver.Navigate().GoToUrl($"{BaseUrl}/PublicProduct");
            
            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(10));
            var addToCartButtons = wait.Until(d => d.FindElements(By.CssSelector("button[type='submit']")).Where(b => b.Text.Contains("THÊM VÀO GIỎ")).ToList());
            
            if (addToCartButtons.Any())
            {
                var badgeElement = Driver.FindElement(By.CssSelector(".badge.bg-danger"));
                string initialText = badgeElement.Text.Trim();
                int initialCount = string.IsNullOrEmpty(initialText) ? 0 : int.Parse(initialText);

                JsClick(addToCartButtons.First());

                // Wait for badge text to update
                wait.Until(d => {
                    var badge = d.FindElement(By.CssSelector(".badge.bg-danger"));
                    string currentText = badge.Text.Trim();
                    int currentCount = string.IsNullOrEmpty(currentText) ? 0 : int.Parse(currentText);
                    return currentCount > initialCount;
                });

                var newCountText = Driver.FindElement(By.CssSelector(".badge.bg-danger")).Text;
                Assert.True(int.Parse(newCountText) > initialCount);
            }
        }
    }
}
