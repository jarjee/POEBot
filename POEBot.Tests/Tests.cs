using System;
using System.IO;
using NUnit.Framework;
using POEBot.PoeWiki;

namespace POEBot.Tests
{
    [TestFixture]
    public class Tests
    {
        [Test]
        public void RenderPages()
        {
            var poeWikiService = new PoeWikiService();

            using (FileStream fs = new FileStream("keystone.png", FileMode.Create))
            {
                var page = poeWikiService.GetPoeWikiPage("http://pathofexile.gamepedia.com/Chaos_Inoculation").Result;

                File.WriteAllText("keystone.html", poeWikiService.GetWikiHtml(page));
                poeWikiService.GetWikiImage(page).WriteTo(fs);
                fs.Flush();
            }

            using (FileStream fs = new FileStream("item.png", FileMode.Create))
            {
                var page = poeWikiService.GetPoeWikiPage("http://pathofexile.gamepedia.com/Citrine_Amulet").Result;

                File.WriteAllText("item.html", poeWikiService.GetWikiHtml(page));
                poeWikiService.GetWikiImage(page).WriteTo(fs);
                fs.Flush();
            }

            using (FileStream fs = new FileStream("divcard.png", FileMode.Create))
            {
                var page = poeWikiService.GetPoeWikiPage("http://pathofexile.gamepedia.com/Rats").Result;

                File.WriteAllText("divcard.html", poeWikiService.GetWikiHtml(page));
                poeWikiService.GetWikiImage(page).WriteTo(fs);
                fs.Flush();
            }
        }
    }
}