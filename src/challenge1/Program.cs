using System;
using System.Collections.Generic;
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
        private static readonly IWebDriver Driver = new InternetExplorerDriver();
        private static readonly string reportLocation = @"e:\challenge1_report.html";
        private static readonly string baseUrl = @"http://slashdot.org/";

        private static void Main()
        {
            // init extent reports
            _extent = new ExtentReports(reportLocation, DisplayOrder.NewestFirst);
            _extent.LoadConfig("extent-config.xml");
            var test = _extent.StartTest("Challenge 1", "This is the challenge 1 test");
            var optionVotes = RunTest(test);
            if (optionVotes != string.Empty)
            {
                test.Log(LogStatus.Pass, "number of people that have voted for that same option: " + Convert.ToInt16(optionVotes));
            }
            else
            {
                test.Log(LogStatus.Fail, "number of people that have voted for that same option: --");
            }
            _extent.EndTest(test);
            _extent.Flush();

            Driver.Quit();

        }

        private static string RunTest(ExtentTest test)
        {
            Driver.Url = baseUrl;
            AssertTitle("Slashdot: News for nerds, stuff that matters", test);
            try
            {

                var articles = Driver.FindElements(By.CssSelector(@"span[class='story-title']"));
                test.Log(LogStatus.Info, "Print how many articles are on the page ", articles.Count.ToString());
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

                AssertTitle("Slashdot Poll | I think my current job will ...", test);
                string[] labelTexts = Driver.FindElements(By.CssSelector("div[class='poll-bar-label']")).ToList().ConvertAll(x => x.Text).ToArray();
                string[] labelVotes = Driver.FindElements(By.CssSelector("div[class='poll-bar-group']>div>div>div")).ToList().ConvertAll(x => x.Text.Split()[0]).ToArray();

                string optionVotes = labelVotes[Array.IndexOf(labelTexts, optionText)];

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
            if (Driver.Title == expectedTitle)
            {
                test.Log(LogStatus.Pass, "Check page title", "Page title is: " + expectedTitle);
            }
            else
            {
                test.Log(LogStatus.Fail, "Check page title", "Incorrect page title" + Driver.Title);
            }
        }

    }
}

