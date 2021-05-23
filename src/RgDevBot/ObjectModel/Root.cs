using System;
using System.Collections.Generic;

namespace RgDevBot.ObjectModel
{
    public enum NewsSource { absrealty = 0,  uk_kc = 1 }


    public class PageInfo
    {
        public string endCursor { get; set; }
        public bool hasNextPage { get; set; }
    }

    public class Project
    {
        public string id { get; set; }
        public string title { get; set; }
        public string slug { get; set; }
        public string outerSite { get; set; }
    }

    public class Node
    {
        private static Uri UKKCBaseUri = new Uri("https://www.uk-kc.ru/peredelkino/");

        public string id { get; set; }
        public string slug { get; set; }
        public DateTime date { get; set; }
        public string title { get; set; }
        public NewsSource type { get; set; }

        public string GetUrl()
        {
            switch (type)
            {
                case NewsSource.absrealty:
                    return $"https://www.absrealty.ru/news/{slug}/";
                case NewsSource.uk_kc:
                {
                    if (Uri.TryCreate(slug, UriKind.Absolute, out _)) 
                        return slug;

                    var url = (new Uri(UKKCBaseUri, slug)).ToString();
                    if (url.EndsWith("/") == false)
                        url += "/";

                    return url;
                }
                default:
                {
                    throw new NotImplementedException();
                }
            }
        }

        public string GetText()
        {
            return $"{type.ToString()}: {title}:\r\n{GetUrl()}";
        }
    }

    public class Edge
    {
        public Node node { get; set; }
    }

    public class AllPosts
    {
        public PageInfo pageInfo { get; set; }
        public int totalCount { get; set; }
        public List<Edge> edges { get; set; }
    }

    public class Data
    {
        public AllPosts allPosts { get; set; }
    }

    public class Root
    {
        public Data data { get; set; }
    }
}