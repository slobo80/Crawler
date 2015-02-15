using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace Crawler
{
    class Program
    {
        private const string BaseUrl = "http://www.microsoftstore.com";
        private const string StartUrl = "/store/msca/en_CA/DisplayHelpPage/";
        private const int CrawlDepth = 1;
        private static List<string> IgnoreUrls = new List<string> { "javascript" };

        static void Main(string[] args)
        {
            RunAsync(StartUrl, CrawlDepth).Wait();
        }

        static async Task RunAsync(string url, int depth)
        {
            if (depth < 0)
            {
                return;
            }
            else if (IgnoreUrls.Find(u => url.Contains(u)) != null)
            {
                Console.WriteLine(string.Format("SKIPPED\t{0}", url));
                return;
            }

            Console.WriteLine(string.Format("VALID\t{0}", url));
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(BaseUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                HttpResponseMessage response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var product = await response.Content.ReadAsStringAsync();
                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(product);
                    var links = doc.DocumentNode.SelectNodes("//a[@href]");

                    if (links == null)
                    {
                        return;
                    }

                    foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//a[@href]"))
                    {
                        HtmlAttribute att = link.Attributes["href"];
                        await RunAsync(att.Value, depth - 1);
                    }
                }
                else
                {
                    Console.WriteLine("ERROR: {0}", url);
                }
            }
        }
    }
}
