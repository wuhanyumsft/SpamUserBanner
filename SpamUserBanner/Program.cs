using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace SpamUserBanner
{
    class Program
    {
        static void Main(string[] args)
        {
            HttpClient client = new HttpClient()
            {
                BaseAddress = new Uri("https://social.msdn.microsoft.com/"),
            };
            client.DefaultRequestHeaders.Add("Cookie", "APPXRPSSA={credential}");

            while (true)
            {
                try
                {
                    var response = client.GetAsync("Forums/zh-CN/home").Result;
                    var responseText = response.Content.ReadAsStringAsync().Result;
                    HtmlDocument pageDocument = new HtmlDocument();
                    pageDocument.LoadHtml(responseText);
                    var elements = pageDocument.DocumentNode.SelectNodes("//span[contains(@class,'lastpost')]");
                    Dictionary<string, int> dict = new Dictionary<string, int>();
                    foreach (var element in elements)
                    {
                        if (element.InnerText.Contains("Created by"))
                        {
                            var author = element.SelectSingleNode(".//a//span").InnerText.Replace(" - ", "");
                            if (!dict.ContainsKey(author))
                            {
                                dict[author] = 0;
                            }
                            dict[author]++;
                        }
                    }

                    foreach (var author in dict.Keys)
                    {
                        if (dict[author] >= 10)
                        {
                            Console.WriteLine(author);
                            BanUser(client, author);
                        }
                    }

                    Console.WriteLine($"Run at: {DateTime.Now.ToLocalTime()}.");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                Thread.Sleep(TimeSpan.FromMinutes(5));
            }
        }

        public static void BanUser(HttpClient client, string displayName)
        {
            var response = client.PostAsync("/Forums/admin/spam", new StringContent($"spamUserDisplayName={displayName}&delSpamAndBanUserButton=Delete+spam+and+ban+user&shouldDeleteUserPosts=true&shouldDeleteUserPosts=false", Encoding.UTF8, "application/x-www-form-urlencoded")).Result;
            var responseText = response.Content.ReadAsStringAsync().Result;
            if (responseText.Contains("Banned successfully"))
            {
                Console.WriteLine($"Banned {displayName} successfully");
            }
            else
            {
                Console.WriteLine($"Banned {displayName} failed.");
            }
        }
    }
}
