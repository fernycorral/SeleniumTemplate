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
			
			browser.DoClickById("flight-origin-hp-flight");
			browser.SetText("//input[@id='flight-origin-hp-flight']", "Chihuahua, Chihuahua, Mexico (CUU-General Roberto Fierro Villalobos Intl.)");
			browser.SetText("//input[@id='flight-destination-hp-flight']", "Barcelona, Spain (BCN-All Airports)");




		}
		[TearDown]
		public void TearDown()
		{

		}
	}
}
