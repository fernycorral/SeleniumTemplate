using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomationSeleniumTemplate.Browser;

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





		}
		[TearDown]
		public void TearDown()
		{

		}
	}
}
