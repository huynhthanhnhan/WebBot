using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Toolkit.Uwp.Notifications;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.InputFiles;
using System.IO;
using Windows.Storage;
using System.Configuration;

namespace WebBot
{
    class Program
    {
        private static ChromeDriver driver;

        [Obsolete]
        static void Main(string[] args)
        {
            ChromeOptions options = new ChromeOptions();
            options.AddArguments("--headless");
            options.AddArgument("--profile-directory=Default");
            options.AddArgument("start-maximized");
            options.AddUserProfilePreference("download.default_directory", @"E:\Downloads");
            options.AddUserProfilePreference("plugins.always_open_pdf_externally", true);
            driver = new ChromeDriver("Driver", options);
            DownloadK12(driver);
            //Download64(driver);
        }
        public static void Download64(ChromeDriver driver)
        {
            driver.Url = "http://cohoc.net/64-que-dich.html";
            var elements = driver.FindElements(By.XPath("//div[@class='vung-list']/h3/a"));
            var listURl = new List<string>();
            foreach (var item in elements)
            {
                listURl.Add(item.GetAttribute("href"));
            }
            foreach (var item in listURl)
            {
                driver.Url = item;
                Thread.Sleep(500);
                var fileName = item.Replace("http://cohoc.net/", "");
                var path = Environment.ExpandEnvironmentVariables(@"E:\Downloads\cohoc");
                var filePath = Path.Combine(path, fileName);
                var a = driver.PageSource;
                File.WriteAllText(filePath, a);
            }
            Console.WriteLine();
        }

        public static void DownloadK12(ChromeDriver driver)
        {
            driver.Url = "https://tgg-caibe-thcsanthaidong.k12online.vn/";
            var element = driver.FindElement(By.XPath("//input[@name='fields[username]']"));
            element.SendKeys("xxx");
            Thread.Sleep(500);
            element = driver.FindElement(By.XPath("//input[@name='fields[password]']"));
            element.SendKeys("xxx");
            Thread.Sleep(500);
            element = driver.FindElement(By.XPath("//button[@class='btn btn-primary btn-login']"));
            element.Click();
            Thread.Sleep(2000);
            
            Thread.Sleep(2000);
            var elements = driver.FindElements(By.XPath("//a[@href='#courseResult2']"));
            elements[0].Click();
            Thread.Sleep(1000);
            var listStudent = new List<Student>();
            AddStudent(driver, listStudent, "module2002");


            foreach (var item in listStudent)
            {
                driver.Navigate().GoToUrl(item.URL);
                //new WebDriverWait(driver, TimeSpan.FromSeconds(5)).Until(d => d.FindElements(By.XPath("//embed[@id='plugin']")).Count > 0);
                try
                {
                    Thread.Sleep(2000);
                    // Output a PDF of the first page in A4 size at 90% scale
                    var printOptions = new Dictionary<string, object>
        {
            { "paperWidth", 210 / 25.4 },
            { "paperHeight", 297 / 25.4 },
            { "scale", 0.9 },
            { "pageRanges", "1" }
        };
                    var printOutput = driver.ExecuteChromeCommandWithResult("Page.printToPDF", printOptions) as Dictionary<string, object>;
                    var pdf = Convert.FromBase64String(printOutput["data"] as string);
                    File.WriteAllBytes($"/{item.Class}/{item.Name}.pdf", pdf);
                    item.IsSuccess = true;
                    Console.WriteLine($"Downloaded {item.ID}/{listStudent.Count}");
                }
                catch (Exception e)
                {
                    item.IsSuccess = false;
                }

            }
            foreach (var item in listStudent)
            {
                Console.WriteLine($"{item.ID}-{item.IsSuccess}-{item.Class}-{item.Name}");

            }
        }
        protected static void AddStudent(ChromeDriver driver, List<Student> listStudent, string moduleName)
        {
            var listTR = driver.FindElements(By.XPath($"//div[@id='{moduleName}']/div[3]/div/table/tbody/tr"));
            for (int i = 1; i <= listTR.Count; i++)
            {
                var name = driver.FindElement(By.XPath($"//div[@id='{moduleName}']/div[3]/div/table/tbody/tr[{i}]/td[2]/div[1]")).Text;
                var _class = driver.FindElement(By.XPath($"//div[@id='{moduleName}']/div[3]/div/table/tbody/tr[{i}]/td[4]")).Text;
                _class = _class.Replace("/", "-");
                var button = driver.FindElement(By.XPath($"//div[@id='{moduleName}']/div[3]/div/table/tbody/tr[{i}]/td[10]/a"));
                var href = button.GetAttribute("href");
                var downloadURL = href.Replace("detail", "print");
                var ID = listStudent.Count + 1;
                var newStudent = new Student()
                {
                    ID = ID,
                    Name = name,
                    Class = _class,
                    URL = downloadURL,
                    IsSuccess = false
                };
                listStudent.Add(newStudent);
            }
            var elements = driver.FindElements(By.XPath("//a[@class='next']"));
            if (elements.Count == 3)
            {
                elements[1].Click();
                Thread.Sleep(3000);
                AddStudent(driver, listStudent, moduleName);
            }
        }
    }
    public class Student
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Class { get; set; }
        public string URL { get; set; }
        public bool IsSuccess { get; set; }
    }
}
