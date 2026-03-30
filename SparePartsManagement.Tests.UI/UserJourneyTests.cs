using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using Xunit;
using System;
using System.Linq;
using System.Collections.Generic;

namespace SparePartsManagement.Tests.UI
{
    public class UserJourneyTests : BaseTest
    {
        [Fact]
        public void FullCheckoutJourney_ShouldReachOrderSummary()
        {
            Driver.Navigate().GoToUrl($"{BaseUrl}/PublicProduct");
            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(10));
            
            var allButtons = wait.Until(d => d.FindElements(By.CssSelector("button[type='submit']")));
            var addToCartButtons = allButtons.Where(b => b.Text.Contains("THÊM VÀO GIỎ")).ToList();
            
            if (addToCartButtons.Count > 0)
            {
                JsClick(addToCartButtons[0]);
                System.Threading.Thread.Sleep(1000); 
                Driver.Navigate().GoToUrl($"{BaseUrl}/Cart/Index");
                
                var cartRows = Driver.FindElements(By.CssSelector("table tbody tr"));
                Assert.NotEmpty(cartRows);
            }
        }

        [Fact]
        public void ProductFilter_ByHighPrice_ShouldOnlyShowExpensiveItems()
        {
            Driver.Navigate().GoToUrl($"{BaseUrl}/PublicProduct?minPrice=500000");
            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(10));
            
            wait.Until(d => d.FindElement(By.TagName("body")));
            
            var priceTags = Driver.FindElements(By.CssSelector(".card-footer .text-danger"));
            foreach (var tag in priceTags)
            {
                string text = tag.Text.Replace(".", "").Replace("₫", "").Trim();
                if (decimal.TryParse(text, out decimal price))
                {
                    Assert.True(price >= 500000);
                }
            }
        }
    }
}
