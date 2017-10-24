//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using OpenQA.Selenium;
//using System.IO;
//using System.Runtime.Serialization.Formatters.Binary;
//using OpenQA.Selenium.Chrome;
//using OpenQA.Selenium.Support.Events;
//using System.Diagnostics;
//using System.Collections.ObjectModel;
//using OpenQA.Selenium.Support.UI;
//using System.Threading;

//namespace SeleniumBrowserWrapper
//{
//	/// <summary>
//	/// Wraps the functionality of an IWebDriver, which Selenium uses to interact with a browser,
//	/// and adds features.
//	/// </summary>
//	/// <see cref="https://selenium.googlecode.com/svn/trunk/docs/api/dotnet/html/N_OpenQA_Selenium.htm"/>
//	public class SeleniumBrowser : IDisposable
//	{
//		/// <summary>
//		/// Creates a new browser with Chrome driver.
//		/// </summary>
//		public SeleniumBrowser() : this(WebDrivers.Chrome) { }

//		/// <summary>
//		/// Creates a new browser with the specified web driver.
//		/// </summary>
//		/// <param name="webDriver">Value to initialize a Selenium browser with a specific WebDriver class.</param>
//		public SeleniumBrowser(WebDrivers webDriver)
//		{
//			IWebDriver iwb = null;
//			string dir = Path.Combine(
//					Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "WebDrivers");
//			switch (webDriver)
//			{
//				case WebDrivers.Firefox:
//					iwb = new OpenQA.Selenium.Firefox.FirefoxDriver();
//					break;
//				case WebDrivers.InternetExplorer:
//					iwb = new OpenQA.Selenium.IE.InternetExplorerDriver(dir);
//					break;
//				case WebDrivers.PhantomJs:
//					iwb = new OpenQA.Selenium.PhantomJS.PhantomJSDriver(dir);
//					break;
//				case WebDrivers.ChromeHeadless:
//					var options = new ChromeOptions();
//					options.AddArgument("--headless");
//					options.AddArgument("--disable-gpu");
//					iwb = new OpenQA.Selenium.Chrome.ChromeDriver(dir, options);
//					break;
//				default:
//					iwb = new OpenQA.Selenium.Chrome.ChromeDriver(dir);
//					break;
//			}
//			Init(iwb);
//		}

//		/// <summary>
//		/// Creates a new browser with the specified web driver.
//		/// </summary>
//		/// <param name="webDriver">The web driver. Web drivers are in the OpenQA.Selenium namespace,
//		/// like OpenQA.Selenium.Chrome.ChromeDriver and OpenQA.Selenium.PhantomJS.PhantomJSDriver.</param>
//		public SeleniumBrowser(IWebDriver webDriver)
//		{
//			Init(webDriver);
//		}

//		/// <summary>
//		/// Creates a new browser with the specified web driver.
//		/// </summary>
//		private void Init(IWebDriver webDriver)
//		{
//			d = new EventFiringWebDriver(webDriver);
//			d.Manage().Timeouts().SetScriptTimeout(TimeSpan.FromSeconds(60));
//			cookies = new Dictionary<string, IEnumerable<Cookie>>();
//			loadedCookies = new HashSet<string>();
//			loadCookies();
//			d.Navigated += SelBrowser_Navigated;
//			CurrentFrame = "/";
//		}


//		#region Fields

//		/// <summary>
//		/// A javascript code library that contains common functions.
//		/// </summary>
//		private readonly string jsCode = Properties.Resources.selenium_utils;

//		/// <summary>
//		/// The web driver object that interacts with the browser./>
//		/// </summary>
//		private EventFiringWebDriver d;

//		/// <summary>
//		/// Used to lock thread access to cookies.
//		/// </summary>
//		private readonly object cookiesLock = new object();

//		/// <summary>
//		/// Associates a domain (key) with a list of cookies.
//		/// </summary>
//		private Dictionary<string, IEnumerable<Cookie>> cookies;

//		/// <summary>
//		/// The domains for which cookies are loaded.
//		/// </summary>
//		private HashSet<string> loadedCookies;

//		/// <summary>
//		/// Used to download files.
//		/// </summary>
//		private readonly System.Net.WebClient client = new System.Net.WebClient();

//		#endregion


//		#region Properties

//		/// <summary>
//		/// Gets the Selenium WebDriver object, through which the user controls the browser.
//		/// </summary>
//		public IWebDriver WebDriver
//		{
//			get { return d; }
//		}

//		/// <summary>
//		/// Gets the source of the page last loaded by the browser.
//		/// </summary>
//		public string Html
//		{
//			get { return d.PageSource; }
//		}

//		/// <summary>
//		/// Gets the document element of the current page.
//		/// </summary>
//		public IWebElement DocumentElement
//		{
//			get { return d.FindElement(By.TagName("html")); }
//		}

//		/// <summary>
//		/// Gets or sets the URL the browser is currently displaying.
//		/// </summary>
//		/// <exception cref="WebDriverTimeoutException">When the URL exceeds a predefined time to load.</exception>
//		public string Url
//		{
//			get
//			{
//				Stopwatch stopwatch = new Stopwatch();
//				stopwatch.Start();
//				while (stopwatch.Elapsed.TotalMilliseconds < 10000)
//				{
//					string url = d.Url;
//					if (url != "")
//						return url;
//				}
//				return null;
//			}
//			set { d.Url = value; }
//		}

//		/// <summary>
//		/// Gets or sets a value that indicates whether the browser saves and loads cookies.
//		/// </summary>
//		public bool EnableCookies { get; set; }

//		/// <summary>
//		/// Gets the cookie string.
//		/// </summary>
//		public string CookieString
//		{
//			get
//			{
//				var cookies = d.Manage().Cookies.AllCookies;
//				return string.Join("; ", cookies.Select(c => string.Format("{0}={1}", c.Name, c.Value)));
//			}
//		}

//		/// <summary>
//		/// Gets the frame the browser has focus in.
//		/// </summary>
//		public string CurrentFrame { get; private set; }

//		#endregion



//		/// <summary>
//		/// Performs a click in the specified element.
//		/// </summary>
//		/// <param name="xPath">XPath of the element to click. If the path
//		/// returns more than one, the first element is chosen.</param>
//		/// <param name="continueOnError">Continue even in case of error.</param>
//		public void Click(string xPath, bool continueOnError = false)
//		{
//			try
//			{
//				var elem = Get(xPath, continueOnError);
//				click(elem);
//			}
//			catch
//			{
//				if (continueOnError) return;
//				else throw;
//			}
//		}

//		/// <summary>
//		/// Performs a click in the specified element.
//		/// </summary>
//		/// <param name="node">Element to click.</param>
//		/// <param name="continueOnError">Continue even in case of error.</param>
//		public void Click(IWebElement node, bool continueOnError = false)
//		{
//			try
//			{
//				click(node);
//			}
//			catch
//			{
//				if (continueOnError) return;
//				else throw;
//			}
//		}

//		/// <summary>
//		/// Keeps clicking a web element until it is invalid.
//		/// </summary>
//		/// <param name="node">Element to click.</param>
//		/// <param name="continueOnError">Continue even in case of error.</param>
//		public void ClickUntilStale(IWebElement node)
//		{
//			while (!IsStaleElement(node))
//				Click(node, true);
//		}

//		/// <summary>
//		/// Keeps clicking a web element until it is invalid.
//		/// </summary>
//		/// <param name="xpath">The XPath of the element to click.</param>
//		/// <param name="continueOnError">Continue even in case of error.</param>
//		public void ClickUntilStale(string xpath)
//		{
//			var node = Get(xpath);
//			ClickUntilStale(node);
//		}

//		/// <summary>
//		/// Checks if a web element is stale. An stale element is not usable.
//		/// </summary>
//		/// <param name="elem">The web element.</param>
//		/// <returns>True if stale; false if enabled.</returns>
//		public bool IsStaleElement(IWebElement elem, int wait = 0)
//		{
//			try
//			{
//				if (wait > 0)
//					Thread.Sleep(wait);
//				bool flag = elem.Displayed;
//				return false;
//			}
//			catch (StaleElementReferenceException)
//			{
//				return true;
//			}
//		}

//		/// <summary>
//		/// Sets the inner text of the specified element.
//		/// </summary>
//		/// <param name="text">Text to set.</param>
//		/// <param name="xPath">XPath of the element to click. If the path
//		/// returns more than one, the first element is chosen.</param>
//		/// <param name="continueOnError">Continue even in case of error.</param>
//		public void SetText(string text, string xPath, bool continueOnError = false)
//		{
//			try
//			{
//				var elem = Get(xPath, continueOnError);
//				elem.Clear();
//				elem.SendKeys(text);
//			}
//			catch
//			{
//				if (continueOnError) return;
//				else throw;
//			}
//		}

//		/// <summary>
//		/// Finds an element by XPath.
//		/// </summary>
//		/// <param name="xPath">XPath to search the DOM for the element. If the path
//		/// returns more than one, the first element is chosen.</param>
//		/// <param name="continueOnError">Continue even in case of error.</param>
//		public IWebElement Get(string xPath, bool continueOnError = false)
//		{
//			int timeout = continueOnError ? 0 : 10000;
//			return Get(xPath, timeout, continueOnError);
//		}

//		/// <summary>
//		/// Finds an element by XPath.
//		/// </summary>
//		/// <param name="xPath">XPath to search the DOM for the element. If the path
//		/// returns more than one, the first element is chosen.</param>
//		/// <param name="timeOut">Milliseconds to wait for the element to appear.</param>
//		/// <param name="continueOnError">Continue even in case of error.</param> 
//		public IWebElement Get(string xPath, int timeOut, bool continueOnError = false)
//		{
//			try
//			{
//				if (timeOut > 0)
//				{
//					var wait = new WebDriverWait(d.WrappedDriver, TimeSpan.FromMilliseconds(timeOut));
//					return wait.Until(wd => wd.FindElement(By.XPath(xPath)));
//				}
//				else return d.FindElement(By.XPath(xPath));
//			}
//			catch (Exception e)
//			{
//				if (e is WebDriverTimeoutException || e is NoSuchElementException)
//				{
//					if (continueOnError) return null;
//					else throw;
//				}
//				else throw;
//			}
//		}

//		/// <summary>
//		/// Find elements by XPath.
//		/// </summary>
//		/// <param name="xPath">XPath to search the DOM for the elements.</param>
//		/// <returns>A list of web elements. An empty list if none is found.</returns>
//		public IList<IWebElement> GetAll(string xPath)
//		{
//			return d.FindElements(By.XPath(xPath));
//		}

//		/// <summary>
//		/// Find elements by XPath.
//		/// </summary>
//		/// <param name="xPath">XPath to search the DOM for the elements.</param>
//		/// <param name="timeout">Milliseconds to wait for the element to appear. If the timeout
//		/// passes an <see cref="OpenQA.Selenium.WebDriverTimeoutException"/> is thrown.</param>
//		/// <param name="continueOnError">Continue without throwing <see cref="OpenQA.Selenium.WebDriverTimeoutException"/>.</param>
//		/// <returns>A list of web elements. An empty list if none is found.</returns>
//		public IList<IWebElement> GetAll(string xPath, int timeout, bool continueOnError = false)
//		{
//			try
//			{
//				if (timeout > 0)
//				{
//					WebDriverWait wait = new WebDriverWait(d.WrappedDriver, TimeSpan.FromMilliseconds(timeout));
//					var elems = new ReadOnlyCollection<IWebElement>(new IWebElement[0]);
//					var result = wait.Until(wd =>
//					{
//						elems = wd.FindElements(By.XPath(xPath));
//						return elems.Count > 0;
//					});
//					return elems;
//				}
//				else return d.FindElements(By.XPath(xPath));
//			}
//			catch (WebDriverTimeoutException)
//			{
//				if (continueOnError) return new IWebElement[0];
//				else throw;
//			}
//		}

//		/// <summary>
//		/// Waits until an element exists and is displayed.
//		/// </summary>
//		/// <param name="xpaths">The XPaths of the elements to find.</param>
//		/// <returns>A number indicating the index of the first element that was shown. The index
//		/// corresponds to an XPath argument passed to this function.</returns>
//		public int WaitUntilShown(params string[] xpaths)
//		{
//			return WaitUntilShown(0, xpaths);
//		}

//		/// <summary>
//		/// Waits until an element exists and is displayed.
//		/// </summary>
//		/// <param name="timeout">Time allowed to wait before throwing an exception.</param>
//		/// <param name="xpaths">The XPaths of the elements to find.</param>
//		/// <returns>A number indicating the index of the first element that was shown. The index
//		/// corresponds to an XPath argument passed to this function.</returns>
//		public int WaitUntilShown(int timeout, params string[] xpaths)
//		{
//			Stopwatch time = null;
//			if (timeout > 0)
//			{
//				time = new Stopwatch();
//				time.Start();
//			}
//			while (true)
//			{
//				for (int i = 0; i < xpaths.Length; i++)
//				{
//					if (ExpectedConditions.ElementIsVisible(By.XPath(xpaths[i]))(d) != null)
//						return i;
//				}
//				if (timeout > 0 && time.ElapsedMilliseconds > timeout)
//					throw new NoSuchElementException("None of these expected elements were shown: " + string.Join(" | ", xpaths));
//			}
//		}


//		#region Window/Frame methods

//		/// <summary>
//		/// Waits for a window to appear.
//		/// </summary>
//		/// <param name="name">Name or handle of the window.</param>
//		/// <param name="timeOut">Time to wait for the window.</param>
//		/// <returns>True if the window is found; false otherwise.</returns>
//		public bool WaitWindow(string name, int timeOut = 10000)
//		{
//			Stopwatch stopwatch = new Stopwatch();
//			stopwatch.Start();
//			while (stopwatch.Elapsed.TotalMilliseconds < timeOut)
//			{
//				try
//				{
//					d.WrappedDriver.SwitchTo().Window(name);
//					return true;
//				}
//				catch (NoSuchWindowException) { }
//			}
//			return false;
//		}

//		/// <summary>
//		/// Waits for a window to appear.
//		/// </summary>
//		/// <param name="index">index of the window.</param>
//		/// <param name="timeOut">Time to wait for the window.</param>
//		/// <returns>True if the window is found; false otherwise.</returns>
//		public bool WaitWindow(int index, int timeOut = 10000)
//		{
//			Stopwatch stopwatch = new Stopwatch();
//			stopwatch.Start();
//			while (stopwatch.Elapsed.TotalMilliseconds < timeOut)
//			{
//				try
//				{
//					SwitchToWindow(index);
//					return true;
//				}
//				catch { }
//			}
//			return false;
//		}

//		/// <summary>
//		/// Switches the browser focus to a window.
//		/// </summary>
//		/// <param name="name">Name or handle of the window.</param>
//		public void SwitchToWindow(string name = "")
//		{
//			if (name != "")
//			{
//				d.WrappedDriver.SwitchTo().Window(name);
//			}
//			else
//				d.WrappedDriver.SwitchTo().Window(d.CurrentWindowHandle);
//			CurrentFrame = "/";
//		}

//		/// <summary>
//		/// Switches the browser focus to a window.
//		/// </summary>
//		/// <param name="index">Index of the window.</param>
//		public void SwitchToWindow(int index)
//		{
//			if (index < d.WindowHandles.Count)
//			{
//				string handle = d.WindowHandles[index];
//				SwitchToWindow(handle);

//			}
//			else throw new IndexOutOfRangeException();
//		}

//		/// <summary>
//		/// Closes a window.
//		/// </summary>
//		/// <param name="index">Index of the window. An empty or negative
//		/// index defaults to the current window.</param>
//		public void CloseWindow(int index = -1)
//		{
//			if (index < 0)
//			{
//				try { index = d.WindowHandles.IndexOf(d.CurrentWindowHandle); }
//				catch (NoSuchWindowException)
//				{
//					index = d.WindowHandles.Count - 1;
//				}
//			}
//			if (index < d.WindowHandles.Count)
//			{
//				string current = d.CurrentWindowHandle;
//				string closed = d.WindowHandles[index];
//				if (current != closed)
//					d.WrappedDriver.SwitchTo().Window(closed);
//				d.Close();
//				if (current != closed)
//					d.WrappedDriver.SwitchTo().Window(current);
//				else if (index - 1 < d.WindowHandles.Count && index - 1 >= 0)
//				{
//					d.WrappedDriver.SwitchTo().Window(d.WindowHandles[index - 1]);
//				}
//			}
//		}

//		/// <summary>
//		/// Switches the browser focus to a page in a specified window and frame.
//		/// </summary>
//		/// <param name="path">Path to the page, separated by slashes. The first value is
//		/// the window name or handle; the rest of the values are the path of frames to follow.
//		/// E.g.: "window/frame/subframe". To use the current window start with a slash:
//		/// "/frame/subframe". </param>
//		public void SwitchTo(string path)
//		{
//			string[] p = path.Split(new[] { '/' }, StringSplitOptions.None);
//			if (p[0] != "")
//				d.WrappedDriver.SwitchTo().Window(p[0]);
//			else
//				d.WrappedDriver.SwitchTo().Window(d.CurrentWindowHandle);
//			for (int i = 1; i < p.Length; i++)
//			{
//				if (string.IsNullOrEmpty(p[i]))
//					continue;
//				if (d.WrappedDriver is ChromeDriver)
//				{
//					var elem = Get("//frame[@id='" + p[i] + "' or @name='" + p[i] + "']"
//							   + "|//iframe[@id='" + p[i] + "' or @name='" + p[i] + "']");
//					d.WrappedDriver.SwitchTo().Frame(elem);
//				}
//				else d.WrappedDriver.SwitchTo().Frame(p[i]);
//			}
//			CurrentFrame = path;
//		}

//		/// <summary>
//		/// Switches to a window containing the specified element.
//		/// </summary>
//		/// <param name="xpath">An HTML element of the top frame of a window.</param>
//		public void FindWindow(string xpath)
//		{
//			string currentWindow = "";
//			try
//			{
//				d.WrappedDriver.SwitchTo().ActiveElement();
//				currentWindow = d.CurrentWindowHandle;
//			}
//			catch { }
//			foreach (var w in d.WindowHandles)
//			{
//				d.WrappedDriver.SwitchTo().Window(w);
//				if (Get(xpath, true) != null)
//					return;
//			}
//			d.WrappedDriver.SwitchTo().Window(currentWindow);
//			throw new NotFoundException("Window not found.");
//		}

//		/// <summary>
//		/// Switches to a window containing the specified element.
//		/// </summary>
//		/// <param name="xpath">An HTML element of the top frame of a window.</param>
//		/// <param name="timeOut">Milliseconds to wait for the window to appear.</param>
//		public bool FindWindow(string xpath, int timeOut)
//		{
//			Stopwatch stopwatch = new Stopwatch();
//			stopwatch.Start();
//			while (stopwatch.Elapsed.TotalMilliseconds < timeOut)
//			{
//				try
//				{
//					FindWindow(xpath);
//					return true;
//				}
//				catch (NotFoundException) { }
//			}
//			return false;
//		}

//		#endregion


//		/// <summary>
//		/// Executes JavaScript in the context of the currently selected frame or window.
//		/// </summary>
//		/// <param name="script">The JavaScript code to execute.</param>
//		/// <param name="args">The arguments to the script.</param>
//		/// <returns>For an HTML element, this method returns a IWebElement
//		/// For a number, a Int64 is returned. 
//		/// For a boolean, a Boolean is returned. 
//		/// For all other cases a String is returned. 
//		/// For an array, we check the first element, and attempt to return a List(T) of that type, following the rules above. Nested lists are not supported. 
//		/// If the value is null or there is no return value, a null reference (Nothing in Visual Basic) is returned.</returns>
//		public object RunScript(string script, params object[] args)
//		{
//			return d.ExecuteScript(script, args);
//		}

//		/// <summary>
//		/// Downloads a file using XmlHttpRequest.
//		/// (Unable to get all types of files. Some requests are truncated. If this
//		/// happens use <see cref="RequestBytes"/> instead).
//		/// </summary>
//		/// <param name="url">URL to the file.</param>
//		/// <param name="fileName">Full path of the file to be saved on disk.</param>
//		/// <returns>The path of the saved file.</returns>
//		public string DownloadXhr(string url, string fileName)
//		{
//			string result = RequestXhr(url);
//			string dir = Path.GetDirectoryName(fileName);
//			Directory.CreateDirectory(dir);
//			File.WriteAllText(fileName, (string)result, Encoding.UTF8);
//			return fileName;
//		}

//		/// <summary>
//		/// Downloads a file using WebClient.
//		/// </summary>
//		/// <param name="url">URL of the file to download.</param>
//		/// <param name="localPath">Location of the file in disk.</param>
//		public void Download(string url, string localPath)
//		{
//			client.Headers[System.Net.HttpRequestHeader.Cookie] = CookieString;
//			string dir = Path.GetDirectoryName(localPath);
//			Directory.CreateDirectory(dir);
//			client.DownloadFile(url, localPath);
//		}

//		/// <summary>
//		/// Requests a string from a URL using <see cref="System.Net.WebClient"/>.
//		/// </summary>
//		/// <param name="url">The URL.</param>
//		public string Request(string url)
//		{
//			client.Headers[System.Net.HttpRequestHeader.Cookie] = CookieString;
//			return client.DownloadString(url);
//		}

//		/// <summary>
//		/// Performs a POST operation with the specified URL using XmlHttpRequest. 
//		/// </summary>
//		/// <param name="url">The requested URL.</param>
//		/// <returns>The response as a string.</returns>
//		public string PostXhr(string url, string data)
//		{
//			string jsDownload =
//				"var done = arguments[2]; "
//				+ "var xhr; "
//				+ "if (window.XMLHttpRequest) { " // code for IE7+, Firefox, Chrome, Opera, Safari
//				+ "xhr = new XMLHttpRequest(); } "
//				+ "else { "  // code for IE6, IE5
//				+ "xhr = new ActiveXObject('Microsoft.XMLHTTP'); } "
//				+ "xhr.open('POST', arguments[0], true); "
//				+ "xhr.onload = function() { done(xhr.responseText); }; "
//				+ "xhr.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded; charset=UTF-8'); "
//				+ "xhr.send(arguments[1]); ";
//			object result = ((IJavaScriptExecutor)d).ExecuteAsyncScript(jsDownload, url, data);
//			return (string)result;
//		}

//		/// <summary>
//		/// Performs a GET operation with the specified URL using XmlHttpRequest. 
//		/// (Unable to get all types of files. Some requests are truncated. If this
//		/// happens use <see cref="RequestBytes"/> instead).
//		/// </summary>
//		/// <param name="url">The requested URL.</param>
//		/// <returns>The response as a string.</returns>
//		public string RequestXhr(string url)
//		{
//			string jsDownload =
//				"var done = arguments[1]; " +
//				"var xhr; " +
//				"if (window.XMLHttpRequest) { " +// code for IE7+, Firefox, Chrome, Opera, Safari
//					"xhr = new XMLHttpRequest(); } " +
//				"else { " + // code for IE6, IE5
//					"xhr = new ActiveXObject('Microsoft.XMLHTTP'); } " +
//				"xhr.open('GET', arguments[0], true); " +
//				"xhr.onload = function() { done(xhr.responseText); }; " +
//				"xhr.send(null); ";
//			object result = ((IJavaScriptExecutor)d).ExecuteAsyncScript(jsDownload, url);
//			return (string)result;
//		}

//		/// <summary>
//		/// Performs a GET operation with the specified URL using XmlHttpRequest 
//		/// retrieving the data as an array of byte. 
//		/// </summary>
//		/// <remarks>Can correctly request resources that <see cref="RequestXhr"/> return incomplete.</remarks>
//		/// <param name="url">The requested URL.</param>
//		/// <returns>The response as a byte array.</returns>
//		public byte[] RequestBytes(string url)
//		{
//			string jsDownload =
//				"var done = arguments[1]; " +
//				"var xhr; " +
//				"if (window.XMLHttpRequest) { " +// code for IE7+, Firefox, Chrome, Opera, Safari
//					"xhr = new XMLHttpRequest(); } " +
//				"else { " + // code for IE6, IE5
//					"xhr = new ActiveXObject('Microsoft.XMLHTTP'); } " +
//				"xhr.open('GET', arguments[0], true); " +
//				"xhr.onload = function() { done(Array.prototype.slice.call(new Uint8Array(xhr.response)).toString()); }; " +
//				//"xhr.overrideMimeType('text\\/plain; charset=x-user-defined');" +
//				"xhr.responseType = 'arraybuffer';" +
//				"xhr.send(null); ";
//			//"return [xhr.responseText];";
//			object result = ((IJavaScriptExecutor)d).ExecuteAsyncScript(jsDownload, url);
//			var arr = ((string)result).Split(','); // Result comes as a ReadOnlyCollection of long.
//			byte[] bytes = new byte[arr.Length];
//			for (int i = 0; i < bytes.Length; i++)
//			{
//				bytes[i] = byte.Parse(arr[i]);
//			}
//			return bytes;
//		}

//		/// <summary>
//		/// Finds the first element that matches an XPath expression, searching in all frames.
//		/// </summary>
//		/// <param name="xpath">The XPath expression.</param>
//		/// <returns>The XPath of the first element found. </returns>
//		public string FindFirst(string xpath)
//		{
//			var result = d.ExecuteScript(jsCode +
//				"var result = getFirstByXpathAllFrames(top.document, arguments[0]);" +
//				"return getPathTo(result)", xpath);
//			return (string)result;
//		}

//		public object FindFirst2(string xpath)
//		{
//			var result = d.ExecuteScript(jsCode +
//				"var result = getFirstByXpathAllFrames(top.document, arguments[0]);" +
//				"return result", xpath);
//			return result;
//		}

//		/// <summary>
//		/// Finds a node in all frames of the current page and runs js code. Used to 
//		/// manipulate nodes inside frames without switching frames.
//		/// </summary>
//		/// <param name="xpath">The XPath expression to find the node.</param>
//		/// <param name="js">Javascript code to run for the selected node. For example ".click();"</param>
//		public void JsNode(string xpath, string js)
//		{
//			d.ExecuteScript(jsCode +
//				"var node = getFirstByXpathAllFrames(top.document, arguments[0]);" +
//				"node" + js, xpath);
//		}

//		/// <summary>
//		/// Finds elements that match an XPath expression, searching in all frames. 
//		/// </summary>
//		/// <remarks>The returned XPaths may refer to elements not in the active frame.</remarks>
//		/// <param name="xpath">The XPath expression.</param>
//		/// <returns>The XPath for each element found. Elements in a different frames may have the same XPath.</returns>
//		public string[] FindAll(string xpath, bool getFrames = false)
//		{
//			object result = d.ExecuteScript(
//				jsCode + "return findAll(top.document, arguments[0], arguments[1]);", xpath, getFrames);
//			var col = (System.Collections.ObjectModel.ReadOnlyCollection<object>)result;
//			var ary = col.Select(e => (string)e).ToArray();
//			return ary;
//		}


//		#region WebElement methods

//		/// <summary>
//		/// Gets the XPath of an element.
//		/// </summary>
//		/// <param name="elem">Element to get the XPath from.</param>
//		/// <returns>A string representing an XPath.</returns>
//		public string XPath(IWebElement elem)
//		{
//			return WebDriverExtensions.GetXpath(elem, (IJavaScriptExecutor)d);
//		}

//		/// <summary>
//		/// Gets the outer source HTML of an element.
//		/// </summary>
//		/// <param name="elem">Web element.</param>
//		public string OuterHtml(IWebElement elem)
//		{
//			return elem.GetAttribute("outerHTML");
//		}

//		/// <summary>
//		/// Gets the inner source HTML of an element.
//		/// </summary>
//		/// <param name="elem">Web element.</param>
//		public string InnerHtml(IWebElement elem)
//		{
//			return elem.GetAttribute("innerHTML");
//		}

//		/// <summary>
//		/// Gets the text of an element. An alternative for the cases when IWebElement.Text returns empty.
//		/// </summary>
//		/// <param name="elem">The web element to get the text from.</param>
//		public string GetText(IWebElement elem)
//		{
//			return (string)d.ExecuteScript("return arguments[0].innerText", elem);
//		}

//		/// <summary>
//		/// Gets the href attribute of an element. An alternative for IWebElement.GetAttribute(), which doesn't return the exact href string.
//		/// </summary>
//		/// <param name="elem">The web element to get the text from.</param>
//		public string GetHref(IWebElement elem)
//		{
//			return (string)d.ExecuteScript("return arguments[0].getAttribute('href')", elem);
//		}

//		/// <summary>
//		/// Gets the specified attribute of an element.
//		/// </summary>
//		/// <param name="xpath">XPath to search the DOM for the element. If the path
//		/// returns more than one, the first element is chosen.</param>
//		/// <param name="attribute">The name of the attribute.</param>
//		/// <param name="defaultValue">A default value to return if the attribute does not exist.</param>
//		public string GetAttribute(string xpath, string attribute, string defaultValue = null)
//		{
//			var elem = Get(xpath, defaultValue != null);
//			return GetAttribute(elem, attribute, defaultValue);
//		}

//		/// <summary>
//		/// Gets the specified attribute of an element.
//		/// </summary>
//		/// <param name="elem">Web element.</param>
//		/// <param name="attribute">The name of the attribute.</param>
//		/// <param name="defaultValue">A default value to return if the attribute does not exist.</param>
//		public string GetAttribute(IWebElement elem, string attribute, string defaultValue = null)
//		{
//			if (defaultValue == null)
//				return elem.GetAttribute(attribute);
//			else
//			{
//				if (elem != null)
//					return elem.GetAttribute(attribute);
//				else
//					return defaultValue;
//			}
//		}

//		/// <summary>
//		/// Sets the href attribute of an anchor, or adds an anchor
//		/// to an element.
//		/// </summary>
//		/// <param name="elem">Web element.</param>
//		/// <param name="href">The href value.</param>
//		public void SetAnchor(IWebElement elem, string href)
//		{
//			if (elem.TagName != "a")
//			{
//				string js = "var e = arguments[0]; " +
//					"var a = document.createElement('a'); " +
//					"while (e.lastChild) " +
//						"a.appendChild(e.firstChild);" +
//					"e.appendChild(a); " +
//					"return a";
//				elem = (IWebElement)d.ExecuteScript(js, elem);
//			}
//			SetAttribute(elem, "href", href);
//			RemoveAttribute(elem, "onclick");
//		}

//		/// <summary>
//		/// Sets the href attribute of an anchor, or adds an anchor
//		/// to an element.
//		/// </summary>
//		/// <param name="xpath">XPath to search the DOM for the element. If the path
//		/// returns more than one, the first element is chosen.</param>
//		/// <param name="href">The href value.</param>
//		public void SetAnchor(string xpath, string href)
//		{
//			SetAnchor(Get(xpath), href);
//		}

//		/// <summary>
//		/// Sets the value of a WebElement attribute.
//		/// </summary>
//		/// <param name="elem">WebElement that contains the attribute to set.</param>
//		/// <param name="name">Name of the attribute.</param>
//		/// <param name="value">Value to set.</param>
//		public void SetAttribute(IWebElement elem, string name, string value)
//		{
//			string js = "arguments[0].setAttribute(arguments[1], arguments[2])";
//			d.ExecuteScript(js, elem, name, value);
//		}

//		/// <summary>
//		/// Removes an attribute from an element.
//		/// </summary>
//		/// <param name="elem">The web element.</param>
//		/// <param name="name">The name of the attribute.</param>
//		public void RemoveAttribute(IWebElement elem, string name)
//		{
//			string js = "arguments[0].removeAttribute(\"" + name + "\");";
//			d.ExecuteScript(js, elem);
//		}

//		/// <summary>
//		/// Gets the last child of an element.
//		/// </summary>
//		/// <param name="elem">The web element.</param>
//		public IWebElement LastChild(IWebElement elem)
//		{
//			try
//			{ return elem.FindElement(By.XPath("*[last()]")); }
//			catch (NoSuchElementException)
//			{ return null; }
//		}

//		#endregion


//		/// <summary>
//		/// Quits the web driver.
//		/// </summary>
//		public void Dispose()
//		{
//			client.Dispose();
//			d.Quit();
//			Directory.EnumerateDirectories(Path.GetTempPath(), "scoped_dir*").AsParallel().ForAll(d =>
//			{
//				try
//				{
//					Directory.Move(d, d + "_");
//					Directory.Delete(d + "_", true);
//				}
//				catch { }
//			});
//		}

//		/// <summary>
//		/// Clicks a modal window.
//		/// </summary>
//		/// <param name="continueOnError">Continue in case of error.</param>
//		public void ClickAlert(bool continueOnError = false)
//		{
//			try
//			{
//				d.WrappedDriver.SwitchTo().Alert().Accept();
//			}
//			catch //TODO: Which exception
//			{
//				if (continueOnError) return;
//				else throw;
//			}
//		}




//		/// <summary>
//		/// Clicks an element.
//		/// </summary>
//		/// <param name="elem">The element.</param>
//		private void click(IWebElement elem)
//		{
//			if (elem.Displayed)
//				elem.Click();
//			else
//				d.ExecuteScript("arguments[0].click()", elem);
//		}


//		/// <summary>
//		/// Saves the cookies of the current page's domain to a file.
//		/// </summary>
//		/// <param name="domain"></param>
//		private void saveCookies(string domain)
//		{
//			lock (cookiesLock)
//			{
//				var f = new BinaryFormatter();
//				if (!cookies.ContainsKey(domain))
//					cookies.Add(domain, null);
//				var cookieList = new List<Cookie>();
//				ReadOnlyCollection<Cookie> pageCookies = new ReadOnlyCollection<Cookie>(new List<Cookie>());
//				try
//				{ pageCookies = d.Manage().Cookies.AllCookies; }
//				catch (WebDriverException e) // Possibly a cookie has an empty name.
//				{ /* Do nothing. Looks like it auto-fixes later. */ }
//				foreach (var c in pageCookies)
//				{
//					cookieList.Add(new Cookie(c.Name, c.Value, c.Domain, c.Path, c.Expiry));
//				}
//				cookies[domain] = cookieList;
//				FileStream fs = new FileStream("cookies-selenium.bin", FileMode.Create, FileAccess.Write, FileShare.None);
//				f.Serialize(fs, cookies);
//				fs.Close();
//			}
//		}

//		/// <summary>
//		/// Loads cookies from a file.
//		/// </summary>
//		private void loadCookies()
//		{
//			lock (cookiesLock)
//			{
//				if (!File.Exists("cookies-selenium.bin")) return;
//				var bf = new BinaryFormatter();
//				FileStream fs = new FileStream("cookies-selenium.bin", FileMode.Open, FileAccess.Read, FileShare.Read);
//				cookies = (Dictionary<string, IEnumerable<Cookie>>)bf.Deserialize(fs);
//				fs.Close();
//			}
//		}

//		/// <summary>
//		/// Loads the cookies of the specified domain.
//		/// </summary>
//		/// <returns>True if cookies were retrieved from a local file.</returns>
//		private bool loadCookies(string domain)
//		{
//			if (loadedCookies.Contains(domain)) return false;
//			if (!cookies.ContainsKey(domain)) return false;
//			var cookieManager = d.Manage().Cookies;
//			cookieManager.DeleteAllCookies();
//			foreach (var c in cookies[domain])
//			{
//				try
//				{
//					cookieManager.AddCookie(c);
//				}
//				catch (WebDriverException e)
//				{ /* Ignore cookie. */ }
//			}
//			loadedCookies.Add(domain);
//			return true;
//		}

//		/// <summary>
//		/// Loads and updates cookies while navigating.
//		/// </summary>
//		void SelBrowser_Navigated(object sender, WebDriverNavigationEventArgs e)
//		{
//			if (!EnableCookies)
//				return;
//			string domain = new Uri(e.Url).Host;
//			bool loaded = loadCookies(domain);
//			if (loaded)
//				d.Navigate().GoToUrl(e.Url);
//			else
//			{
//				saveCookies(new Uri(e.Url).Host);
//				if (!loadedCookies.Contains(domain))
//					loadedCookies.Add(domain);
//			}
//		}

//		/// <summary>
//		/// Cleans a string that has quotes and/or double quotes to a concat
//		/// so it can be used in XPath. 
//		/// </summary>
//		/// <remarks>Quotes or double quotes in a string in XPath could be interpreted as a string delimiter
//		/// when not desired.</remarks>
//		/// <example>The string <code>I'm reading "Harry Potter"</code>
//		/// gets converted to <code>concat('I',"'",'m reading ','"','Harry Potter','"')</code>.</example>
//		/// <param name="s"></param>
//		/// <returns>A string representing an XPath concat function that builds an equivalent
//		/// of the input string.</returns>
//		public static string CleanStringForXpath(string s)
//		{
//			var parts = System.Text.RegularExpressions.Regex.Matches(
//				s, "[^'\"]+|['\"]",
//				System.Text.RegularExpressions.RegexOptions.Compiled)
//				.Cast<System.Text.RegularExpressions.Match>()
//				.Select(m =>
//				{
//					if (m.Value == "'") return "\"'\""; //  output "'"
//					if (m.Value == "\"") return "'\"'"; // output '"'
//					return "'" + m.Value + "'";
//				}).ToArray();
//			if (parts.Length == 1)
//				return "'" + s + "'";
//			return "concat(" + string.Join(",", parts) + ")";
//		}
//	}

//	/// <summary>
//	/// Values to initialize a Selenium Browser with a specific WebDriver class.
//	/// </summary>
//	public enum WebDrivers
//	{
//		/// <summary>
//		/// Provides a mechanism to write tests against Chrome. Uses an instance of the OpenQA.Selenium.Chrome.ChromeDriver.
//		/// </summary>
//		Chrome,

//		/// <summary>
//		/// WebDriver uses Chrome in headless mode.
//		/// </summary>
//		ChromeHeadless,

//		/// <summary>
//		/// Provides a way to access Internet Explorer to run your tests by creating a InternetExplorerDriver instance.
//		/// </summary>
//		InternetExplorer,

//		/// <summary>
//		/// Provides a way to access Firefox to run tests.
//		/// </summary>
//		Firefox,

//		/// <summary>
//		/// Provides a way to access PhantomJS to run your tests by creating a PhantomJSDriver instance.
//		/// PhantomJS is a scripted, headless browser used for automating web page interaction. 
//		/// </summary>
//		PhantomJs
//	}
//}
