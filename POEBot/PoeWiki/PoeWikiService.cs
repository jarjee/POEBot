using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NReco.ImageGenerator;
using POEBot.PoeWiki.Models;
using RestSharp.Extensions;

namespace POEBot.PoeWiki
{
    /**
     * FEATURE REQUESTS:
     * 1) Vendor Rewards for Skill Gems
     * 2) Only produce images for valid pages
    */
    public class PoeWikiService
    {
        public async Task<PoePageRef> GetPoePage(string searchTerm)
        {
            var results = await GetPoePages(searchTerm);
            return results.FirstOrDefault();
        }

        public async Task<List<PoePageRef>> GetPoePages(string searchTerm)
        {
            using (WebClient wc = new WebClient())
            {
                var sanitisedTerm = searchTerm.UrlEncode();
                Stream website = await wc.OpenReadTaskAsync(new Uri($"http://pathofexile.gamepedia.com/api.php?action=opensearch&format=json&formatversion=2&search={sanitisedTerm}&namespace=0&limit=10&suggest=true", UriKind.Absolute));
                using (StreamReader sr = new StreamReader(website))
                {
                    var searchResult = JArray.Load(new JsonTextReader(sr));
                    List<PoePageRef> results = new List<PoePageRef>();
                    //Result is a [TERM, [COMPLETED NAME], [UNKNOWN], [URLS]]
                    if (searchResult.Count == 4)
                    {
                        //Get the total number of returned items
                        for (int i = 0; i < searchResult[1].Count(); i++)
                        {
                            results.Add(new PoePageRef(searchResult[1][i].ToString(), searchResult[3][i].ToString()));
                        }
                    }
                    return results;
                }
            }
        }

        public string GetWikiHtml(PoePage page)
        {
            string itemboxTemplateLocation = ConfigurationManager.AppSettings[PoeWikiSettings.ItemBoxLocation];
            string itemboxTemplate = File.ReadAllText(itemboxTemplateLocation);
            return itemboxTemplate.Replace("#itembox", page.Html);
        }

        public async Task<MemoryStream> GetWikiImage(string poeUrl)
        {
            var itemBox = await GetPoeWikiPage(poeUrl);
            return GetWikiImage(itemBox);
        }

        public MemoryStream GetWikiImage(PoePage itemBox)
        {
            var result = GetWikiHtml(itemBox);

            var convert = new NReco.ImageGenerator.HtmlToImageConverter
            {
                Width = 348,
                CustomArgs = "--quality 85",
                Zoom = 2
            };
            var image = convert.GenerateImage(result, ImageFormat.Png);
            var stream = new MemoryStream(image) {Position = 0};

            return stream;
        }

        public async Task<PoePage> GetPoeWikiPage(string poeUrl)
        {
            using (WebClient wc = new WebClient())
            {
                Stream website = await wc.OpenReadTaskAsync(new Uri(poeUrl, UriKind.Absolute));
                using (StreamReader sr = new StreamReader(website))
                {
                    HtmlDocument poePage = new HtmlDocument();
                    poePage.LoadHtml(sr.ReadToEnd());

                    var itemBox = poePage.DocumentNode.SelectSingleNode("//span[contains(@class,'infobox-page-container')]");

                    if (itemBox != null)
                    {
                        return GetPoeItemPage(itemBox);
                    }

                    var divCard = poePage.DocumentNode.SelectSingleNode("//div[contains(@class, 'item-box -divicard -floatright')]");
                    if (divCard != null)
                    {
                        return PoePage.DivinationPage(divCard.OuterHtml);
                    }

                    var keystone = poePage.DocumentNode.SelectSingleNode("//div[contains(@class, 'infocard passive-keystone')]");
                    if (keystone != null)
                    {
                        return PoePage.KeystonePage(keystone.OuterHtml);
                    }

                    return PoePage.InvalidPage();
                }
            }
        }

        public PoePage GetPoeItemPage(HtmlNode itemBox)
        {
            if (itemBox != null)
            {
                //Remove everything other than the first
                var totalChildren = itemBox.ChildNodes.Count;
                for (var i = 1; i < totalChildren; i++)
                {
                    itemBox.ChildNodes[1].Remove();
                }

                return PoePage.ItemPage(itemBox.InnerHtml);
            }
            return PoePage.InvalidPage();
        }
    }
}