using CsvHelper;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;

namespace CelsiusNetWorthScraper
{
    internal class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                var xtext = "";
                try
                {
                    var wc = new WebClient().DownloadString("https://celsiusnetworth.com/random");
                    var htmlDoc = new HtmlDocument();
                    htmlDoc.LoadHtml(wc);
                    var Loser = new Loser();
                    Loser.Name = htmlDoc.DocumentNode.SelectNodes("//body/div[1]/div[1]/div[1]/h1[1]")[0].InnerText;
                    if (!Loser.Name.Contains("lost"))
                        continue;
                    Loser.Name = Loser.Name.Substring(0, Loser.Name.ToLower().IndexOf("lost")).Trim();
                    var Url = wc.Replace(wc.Substring(0, wc.IndexOf("slug:") + 5), "");
                    Loser.Url = $"https://celsiusnetworth.com/{Url.Substring(0, Url.IndexOf("}")).Replace("\"", "")}.htm";
                    var Table = htmlDoc.DocumentNode.SelectNodes("//tr").Skip(1);
                    Loser.Portfolio = new Portfolio();
                    Loser.Portfolio.Tokens = new List<Token>();
                    foreach (var item in Table)
                    {
                        var token = new Token();
                        var text = item.InnerText.Split('\n');
                        token.Name = text[0].Trim().ToUpper();
                        token.Amount = decimal.Parse(text[1].Trim());
                        token.Value = decimal.Parse(text[2].Replace("$", "").Trim());
                        Loser.Portfolio.Tokens.Add(token);
                    }

                    var LossText = htmlDoc.DocumentNode.SelectNodes("//h2")[0].InnerText.Trim().Split(':')[1].Replace("$", "").Trim();
                    Loser.Portfolio.TotalLoss = decimal.Parse(LossText);
                    using (var writer = new StreamWriter("losers.json", true))
                    {
                        writer.WriteLine(JsonConvert.SerializeObject(Loser));
                        writer.Flush();

                        Console.WriteLine($"Added {Loser.Name}, Total Loss: ${Loser.Portfolio.TotalLoss:n2}");
                    }
                    Thread.Sleep(3333);
                }
                catch { }
            }
        }
        public class Loser
        {
            public string Name { get; set; }
            public Portfolio Portfolio { get; set; }
            public string Url { get; set; }

        }
        public class Portfolio
        {
            public List<Token> Tokens;
            public decimal TotalLoss;
        }
        public class Token
        {
            public string Name;
            public decimal Amount;
            public decimal Value;
        }

    }
}
