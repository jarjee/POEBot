using System;
using POEBot.Templates;

namespace POEBot.PoeWiki.Models
{
    public enum PoePageType {
        Item,
        DivinationCard,
        Keystone
    }

    public class PoePageRef
    {
        public PoePageRef(string name, string url)
        {
            Name = name;
            Url = new Uri(url, UriKind.Absolute).AbsoluteUri;
        }
        public readonly string Name;
        public readonly string Url;
    }

    public class PoePage
    {
        public readonly PoePageType Type;

        public readonly string Html;

        private PoePage(PoePageType type, string pageBody)
        {
            Type = type;
            Html = pageBody;
        }

        public bool IsValid()
        {
            return Html == null;
        }

        public static PoePage InvalidPage()
        {
            return new PoePage(default(PoePageType), null);
        }

        public static PoePage ItemPage(string html)
        {
            return new PoePage(PoePageType.Item, html);
        }

        public static PoePage KeystonePage(string html)
        {
            return new PoePage(PoePageType.Keystone, html);
        }

        public static PoePage DivinationPage(string html)
        {
            return new PoePage(PoePageType.DivinationCard, html);
        }

    }
}