using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomationSeleniumTemplate.Browser;
using System.Threading;

namespace AutomationSeleniumTemplate.TestCases
{
	class TestClass
	{
		SeleniumBrowser browser;

		[SetUp]
		public void Setup()
		{
			browser = new SeleniumBrowser();
			browser.GoToURL("https://www.expedia.com/");
		}
		[Test]
		[Category("a")]
		public void Test1()
		{
			
			browser.DoClickById("tab-flight-tab-hp");
			browser.SetText("//input[@id='flight-origin-hp-flight']", "Chihuahua, Chihuahua, Mexico (CUU-General Roberto Fierro Villalobos Intl.)");
			browser.SetText("//input[@id='flight-destination-hp-flight']", "Barcelona, Spain (BCN-All Airports)");
			browser.DoClickById("flight-departing-hp-flight");
			browser.DoClickByXPath("//button[@data-month='10' and @data-day='22']");
			browser.DoClickById("flight-returning-hp-flight");
			browser.DoClickByXPath("//button[@data-month='10' and @data-day='24']");
			browser.ClickUntilElementAppears("//label/button[@type='submit']", "//input[@id='stopFilter_stops-2']");
			browser.DoClickById("stopFilter_stops-2");
			Assert.AreEqual("Price (Lowest)",browser.GetComboBoxSelectedVal("//select[@name='sort']"));
		}

		[Test]
		[Category("b")]
		public void Test2()
		{
			browser.SetText("//input[@id='package-origin-hp-package']", "Chihuahua, Chihuahua, Mexico (CUU-General Roberto Fierro Villalobos Intl.)");
			browser.SetText("//input[@id='package-destination-hp-package']", "Barcelona, Spain (BCN-All Airports)");
			browser.DoClickById("package-departing-hp-package");
			browser.DoClickByXPath("//button[@data-month='10' and @data-day='22']");
			browser.DoClickById("package-returning-hp-package");
			browser.DoClickByXPath("//button[@data-month='10' and @data-day='24']");
			browser.ClickUntilElementAppears("//label/button[@id='search-button-hp-package']", "//div[@class='imgLoading']");
			browser.WaitUntilElementAppears("//input[@id='star5']");
			browser.ClickUntilElementAppears("//input[@id='star5']", "//div[@id='legal-disclosure']");
			foreach (var elem in browser.FindElements("//article/div/div/a"))
			{
				elem.Click();
				Thread.Sleep(7000);
				browser.SwitchToNewWindow();
				browser.DoClickById("tab-reviews");
				browser.SelectFromCombo("//select[@id='reviews-sort-selector']", "Lowest rating");
				string rate = browser.FindElement("//span[@class='rating']/span").Text;
				if (Convert.ToInt32(rate) < 4)
					Assert.Fail();

			}
		}

		[TearDown]
		public void TearDown()
		{
			browser.Close();
		}
	}
}
