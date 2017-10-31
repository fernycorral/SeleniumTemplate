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
		private string homeWindowName;

		public object ProxyType { get; private set; }

		public SeleniumBrowser()
		{
			
			
			FirefoxProfile myprofile = new FirefoxProfileManager().GetProfile("mySeleniumProject");
			driver = new FirefoxDriver(myprofile);
			homeWindowName = driver.WindowHandles.Last();
			
		}

		/// <summary>
		/// Wait until specific element appear
		/// </summary>
		/// <param name="xpath">desired element xpath</param>
		public void WaitUntilElementAppears(string xpath)
		{
			IWebElement element = (new WebDriverWait(driver, TimeSpan.FromMilliseconds(30000))
				.Until(ExpectedConditions.ElementToBeClickable(By.XPath(xpath))));
		}
		/// <summary>
		/// Find element by ID attribute and click it
		/// </summary>
		/// <param name="id"></param>
		public void DoClickById(string id)
		{
			driver.FindElement(By.Id(id)).Click();
		}

		/// <summary>
		/// Go to specific URL
		/// </summary>
		/// <param name="url"></param>
		public void GoToURL(string url)
		{
			driver.Url = url;	
		}

		/// <summary>
		/// Change to new opened window
		/// </summary>
		public void SwitchToNewWindow()
		{
			string windowName = driver.WindowHandles.Last();
			driver.SwitchTo().Window(windowName);
		}
		/// <summary>
		/// Get back to main window 
		/// </summary>
		public void ReturnToMainWindow()
		{
			driver.Close();
			driver.SwitchTo().Window(homeWindowName);
		}

		/// <summary>
		/// Do Click using Xpath
		/// </summary>
		/// <param name="XPath">Xpath where you want to click</param>
		public void DoClickByXPath(string XPath)
		{
			try
			{
				driver.FindElement(By.XPath(XPath)).Click();
			}
			catch(Exception e)
			{ }
		}

		/// <summary>
		/// Write text over input box
		/// </summary>
		/// <param name="Xpath">input XPATH</param>
		/// <param name="text">text to be wrote</param>
		public void SetText(string Xpath,string text)
		{
			driver.FindElement(By.XPath(Xpath)).SendKeys(text);
		}

		/// <summary>
		/// Retrieve text from an element
		/// </summary>
		/// <param name="element">Element</param>
		/// <returns></returns>
		public string GetTextElement(IWebElement element)
		{
			return (string)(driver as IJavaScriptExecutor).ExecuteScript("return arguments[0].innerText", element);
		}

		/// <summary>
		/// Click and wait until a specific element appears
		/// </summary>
		/// <param name="xpath">Desired element xpath</param>
		/// <param name="wantedElementXpath">Wanted element xpath</param>
		public void ClickUntilElementAppears(string xpath, string wantedElementXpath)
		{
			WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromMilliseconds(30000));
			driver.FindElement(By.XPath(xpath)).Click();
			wait.Until(ExpectedConditions.ElementIsVisible(By.XPath(wantedElementXpath)));
		}

		/// <summary>
		/// Find multiple elements using same xpath
		/// </summary>
		/// <param name="xpath">xpath to match</param>
		/// <returns></returns>
		public IList<IWebElement> FindElements(string xpath)
		{
			return driver.FindElements(By.XPath(xpath)); ;
		}
		/// <summary>
		/// Find element using xpath
		/// </summary>
		/// <param name="xpath">Desired element xpath</param>
		/// <returns></returns>
		public IWebElement FindElement(string xpath)
		{
			return driver.FindElement(By.XPath(xpath)); ;
		}
		/// <summary>
		/// Let driver waite for some seconds
		/// </summary>
		/// <param name="seconds">seconds to wait</param>
		public void Wait(int seconds)
		{
			driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(seconds));
		}

		public String GetComboBoxSelectedVal(string xpath)
		{
			return new SelectElement(driver.FindElement(By.XPath(xpath))).SelectedOption.Text;
		}

		/// <summary>
		/// Picks an option from a combobox
		/// </summary>
		/// <param name="xpath">combobox xpath</param>
		/// <param name="textOption">option wanted</param>
		public void SelectFromCombo(string xpath,string textOption)
		{
			new SelectElement(driver.FindElement(By.XPath(xpath))).Options
				.First(E => E.Text == textOption).Click();
		}

		

		/// <summary>
		/// Clicks an element.
		/// </summary>
		/// <param name="elem">The element.</param>
		public void Click(IWebElement elem)
		{
			if (elem.Displayed)
				elem.Click();
			else
				(driver as IJavaScriptExecutor).ExecuteScript("arguments[0].click()", elem);
		}

		public void Close()
		{
			driver.Close();
			driver.Dispose();
			
			
		}

	}
}
