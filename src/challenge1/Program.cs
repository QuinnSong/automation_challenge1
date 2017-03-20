using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using RelevantCodes.ExtentReports;
using OpenQA.Selenium;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Support.UI;

namespace Challenge1
{
    internal class Program
    {
        private static ExtentReports _extent;
        private static ExtentTest _test;
        private static readonly IWebDriver Driver = new InternetExplorerDriver();
        private static readonly string reportLocation = @"challenge1_report.html";
        private static readonly string screenshotLocation = @"challenge1_screenshot.png";
        private static readonly string baseUrl = @"http://slashdot.org/";

        private static void Main()
        {
            // init extent reports
            _extent = new ExtentReports(reportLocation, DisplayOrder.NewestFirst);
            _extent.LoadConfig("extent-config.xml");
            _test = _extent.StartTest("Challenge 1", "This is the challenge 1 test");
            var optionVotes = RunTest(_test);
            if (optionVotes != string.Empty)
            {
                _test.Log(LogStatus.Pass, "people voted for the same option: " + Convert.ToInt16(optionVotes));
            }
            else
            {
                _test.Log(LogStatus.Fail, "people voted for the same option: --");
            }
            _extent.EndTest(_test);

            //_extent.Flush();
            _extent.Close();
            Driver.Quit();

        }

        private static void TakeScreenshot(IWebDriver driver, string fileName)
        {
            ITakesScreenshot screenshotDriver = driver as ITakesScreenshot;
            if (screenshotDriver == null)
            {
                _test.Log(LogStatus.Fail, "Error in taking screenshot!");
            }
            else
            {
                Screenshot screenshot = screenshotDriver.GetScreenshot();
                screenshot.SaveAsFile(fileName, ImageFormat.Png);
            }
            
        }

        private static string RunTest(ExtentTest test)
        {
            Driver.Url = baseUrl;
            AssertTitle("stuff that matters", test);
            try
            {

                var articles = Driver.FindElements(By.CssSelector(@"span[class='story-title']"));
                test.Log(LogStatus.Info, "Print articles qty on the page ", articles.Count.ToString());
                var allIcons = Driver.FindElements(By.CssSelector(@"span[class='topic'] > a >img"));
                List<string> allTitles =
                    allIcons.ToList().ConvertAll(x => x.GetAttribute("title")).ToList();
                var uniqueIcons =
                    from title in allTitles
                    group title by title into t
                    select new { value = t.Key, count = t.Count() };
                foreach (var t in uniqueIcons)
                {
                    // Print a list of unique (different) icons used on article titles and how many times was it used 
                    test.Log(LogStatus.Info, "Unique icon title: " + t.value, "Used times: " + t.count.ToString());
                }

                IWebElement form = Driver.FindElement(By.Id("pollBooth"));
                IWebElement[] labels = Driver.FindElements(By.CssSelector(@"form#pollBooth>label")).ToArray();

                // pickup a random option (label)
                Random rand = new Random();
                int index = rand.Next(0, labels.Length);

                labels[index].Click();
                string optionText = labels[index].Text;
                // Vote for some random option on the daily poll 
                form.Submit();

                WebDriverWait wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(3));
                wait.Until(d => d.FindElement(By.CssSelector("div[class='poll-bar-label']")));

                AssertTitle("humans discover life on another ...", test);
                string[] labelTexts = Driver.FindElements(By.CssSelector("div[class='poll-bar-label']")).ToList().ConvertAll(x => x.Text).ToArray();
                string[] labelVotes = Driver.FindElements(By.CssSelector("div[class='poll-bar-group']>div>div>div")).ToList().ConvertAll(x => x.Text.Split()[0]).ToArray();

                string optionVotes = labelVotes[Array.IndexOf(labelTexts, optionText)];
                TakeScreenshot(Driver, screenshotLocation);
                _test.Log(LogStatus.Info, "Screenshot - " + _test.AddScreenCapture(screenshotLocation)); // add screenshot

                // Return the number of people that have voted for that same option 
                return optionVotes;
            }
            catch (Exception e)
            {
                test.Log(LogStatus.Error, "Exception occurred: ", e.Message);
                return string.Empty;
            }

        }

        private static void AssertTitle(string expectedTitle, ExtentTest test)
        {
            if (Driver.Title.EndsWith(expectedTitle))
                test.Log(LogStatus.Pass, "Check page title", "Page title was verified");
            else
            {
                test.Log(LogStatus.Fail, "Check page title", "Incorrect page title" + Driver.Title);
            }
        }

    }
}

