using System;
using System.IO;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace ImageDisplayerConsole
{
    class Program
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SwHide = 0;
        const int SwShow = 5;

        static async Task Main(string[] args)
        {
            // Hide the console window
            IntPtr hwnd = GetConsoleWindow();
            ShowWindow(hwnd, SwHide);

            string filePath = "C:\\Users\\Admin\\Documents\\Test"; // File path (must change)
            ChromeOptions options = new ChromeOptions();
            options.AddArgument("--start-fullscreen");

            IWebDriver driver = new ChromeDriver(options);
            IJavaScriptExecutor browser = (IJavaScriptExecutor)driver;

            while (true)
            {
                string[] files = Directory.GetFiles(filePath);

                foreach (string file in files)
                {
                    try
                    {
                        string[] parts = Path.GetFileNameWithoutExtension(file).Split(';');

                        if (Path.GetExtension(file).Equals(".web", StringComparison.OrdinalIgnoreCase))
                        {
                            string[] fileParts = File.ReadAllText(file).Split(';');
                            if (fileParts.Length == 3 && int.TryParse(fileParts[0], out int duration) && int.TryParse(fileParts[1], out int zoom))
                            {
                                string url = fileParts[2];
                                driver.Navigate().GoToUrl(url);
                                browser.ExecuteScript($"document.body.style.zoom='{zoom}%'");
                                await Task.Delay(duration * 1000);
                            }
                        }
                        else if (parts.Length >= 2 && int.TryParse(parts[1], out int durationSec))
                        {
                            if (Path.GetExtension(file).Equals(".pdf", StringComparison.OrdinalIgnoreCase) && parts.Length == 3 && int.TryParse(parts[2], out int nbPages))
                            {
                                for (int i = 1; i <= nbPages; i++)
                                {
                                    string pdfUrl = $"{file}#page={i}&view=Fit&scrollbar=0&toolbar=0";
                                    driver.Navigate().GoToUrl(pdfUrl);
                                    driver.Navigate().Refresh();
                                    await Task.Delay(durationSec * 1000 / nbPages);
                                }
                            }
                            else if (parts.Length == 2)
                            {
                                string nonPdfUrl = $"file:///{file}#page=1&view=Fit&scrollbar=0&toolbar=0&scrollbar=0";
                                driver.Navigate().GoToUrl(nonPdfUrl);
                                await Task.Delay(durationSec * 1000);
                            }
                            else if (parts.Length == 3 && int.TryParse(parts[2], out int zoom))
                            {
                                string generalUrl = $"file:///{file}";
                                driver.Navigate().GoToUrl(generalUrl);
                                browser.ExecuteScript($"document.body.style.zoom='{zoom}%'");
                                await Task.Delay(durationSec * 1000);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                    }
                }
            }
        }
    }
}
