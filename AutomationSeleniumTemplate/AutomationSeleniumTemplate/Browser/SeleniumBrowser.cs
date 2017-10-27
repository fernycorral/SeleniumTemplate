using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomationSeleniumTemplate.Browser
{
	public class SeleniumBrowser
	{
		IWebDriver driver;

		public object ProxyType { get; private set; }

		public SeleniumBrowser()
		{
			
			
			FirefoxProfile myprofile = new FirefoxProfileManager().GetProfile("mySeleniumProject");
			driver = new FirefoxDriver(myprofile);
			
		}

		public void DoClickById(string id)
		{
			driver.FindElement(By.Id(id)).Click();
		}

		public void GoToURL(string url)
		{
			driver.Url = url;	
		}

		public void DoClickByXPath(string XPath)
		{
			try
			{
				driver.FindElement(By.XPath(XPath)).Click();
			}
			catch(Exception e)
			{ }
		}

		public void SetText(string Xpath,string text)
		{
			driver.FindElement(By.XPath(Xpath)).SendKeys(text);
		}

		public string GetTextElement(IWebElement element)
		{
			return (string)(driver as IJavaScriptExecutor).ExecuteScript("return arguments[0].innerText", element);
		}

		public void ClickUntilElementAppears(string xpath, string wantedElementXpath)
		{
			WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromMilliseconds(30000));
			driver.FindElement(By.XPath(xpath)).Click();
			wait.Until(ExpectedConditions.ElementIsVisible(By.XPath(wantedElementXpath)));
		}

		public IList<IWebElement> FindElements(string xpath)
		{
			return driver.FindElements(By.XPath(xpath)); ;
		}
		public IWebElement FindElement(string xpath)
		{
			return driver.FindElement(By.XPath(xpath)); ;
		}

		public void Wait(int seconds)
		{
			driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(seconds));
		}

		public void SelectFromCombo(string xpath,string textOption)
		{
			new SelectElement(driver.FindElement(By.XPath(xpath))).AllSelectedOptions
				.First(E => E.Text == textOption).Click();
		}

		/// <summary>
		/// Clicks an element.
		/// </summary>
		/// <param name="elem">The element.</param>
		private void click(IWebElement elem)
		{
			if (elem.Displayed)
				elem.Click();
			else
				(driver as IJavaScriptExecutor).ExecuteScript("arguments[0].click()", elem);
		}

	}
}
