using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using AngleSharp.Common;
using AngleSharp.Html.Parser;
using Newtonsoft.Json;
using RgDevBot.Config;
using RgDevBot.ObjectModel;

namespace RgDevBot
{
    public class NewsParser
    {
        private readonly TelegramBot _bot;
        private readonly SentConfig _config;

        public NewsParser(TelegramBot bot, SentConfig config)
        {
            _bot = bot;
            _config = config;
        }

        public void Parse()
        {
            var content = Get(20);

            var root = JsonConvert.DeserializeObject<Root>(content);

            var absrealtyNews = root.data.allPosts.edges.Select(e=>e.node).OrderByDescending(e => e.date).ToArray();

            Console.WriteLine($"[{DateTime.Now}] Получено {absrealtyNews.Length} новостей(absrealty). Самая новая: {absrealtyNews.FirstOrDefault()?.title}.");

            var ukkcNews = GetUKKCNews();

            Console.WriteLine($"[{DateTime.Now}] Получено {ukkcNews.Length} новостей(ukkc). Самая новая: {ukkcNews.FirstOrDefault()?.title}.");

            var latestNews = absrealtyNews.Concat(ukkcNews).ToArray();

            foreach (var post in latestNews)
            {
                if (_config.ConfigValues.Contains(post.id))
                {
                    continue;
                }

                try
                {
                    var text = post.GetText();
                    Console.WriteLine(text);
                    _bot.SendMessage(text);

                    _config.ConfigValues.Add(post.id);
                    _config.Save();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        public string Get(int top = 20)
        {
            var total = GetTotalPages();

            if ((total - top) < 0)
                top = total;

            var after = Base64Encode($"arrayconnection:${total - top}");

            var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://www.absrealty.ru/graphql/");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                var json =
                    "{\"query\":\"query allPosts($first: Int, $after: String, $isSpecial: Boolean, $project: String, $category: String, $year: String) {\\n  allPosts(first: $first, after: $after, isSpecial: $isSpecial, project: $project, category: $category, year: $year) " +
                    "{\\n    pageInfo {\\n      endCursor\\n      hasNextPage\\n    }\\n    totalCount\\n    edges {\\n      node {\\n        id\\n        slug\\n        date\\n        title\\n        shortDescription\\n       " +
                    " projects {\\n          id\\n          title\\n          slug\\n          outerSite\\n        }\\n        imagePageNewsDisplay\\n        imagePageNewsPreview\\n        imageDisplay\\n        imagePreview\\n      }\\n    }\\n  }\\n}" +
                    "\\n\",\"variables\":{\"project\":\"peredelkino-blizhnee\",\"first\":" + top + ",\"after\":\"" + after + "\"}}";

                streamWriter.Write(json);
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var json = streamReader.ReadToEnd();

                return json;
            }
        }

        private int GetTotalPages()
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://www.absrealty.ru/graphql/");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                var json = "{\"query\":\"query allPosts($first: Int, $after: String, $isSpecial: Boolean, $project: String, $category: String, $year: String) {\\n  allPosts(first: $first, after: $after, isSpecial: $isSpecial, project: $project, category: $category, year: $year) {\\n    pageInfo {\\n      endCursor\\n      hasNextPage\\n    }\\n    totalCount\\n    edges {\\n      node {\\n        id\\n        slug\\n        date\\n        title\\n        shortDescription\\n        projects {\\n          id\\n          title\\n          slug\\n          outerSite\\n        }\\n        imagePageNewsDisplay\\n        imagePageNewsPreview\\n        imageDisplay\\n        imagePreview\\n      }\\n    }\\n  }\\n}\\n\",\"variables\":{\"project\":\"peredelkino-blizhnee\",\"first\":6,\"after\":\"YXJyYXljb25uZWN0aW9uOjE=\"}}";

                streamWriter.Write(json);
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var json = streamReader.ReadToEnd();

                var root = JsonConvert.DeserializeObject<Root>(json);
                var totalPages = (root?.data.allPosts.totalCount).GetValueOrDefault();

                Console.WriteLine($"[{DateTime.Now}] Всего {totalPages} новостей.");

                return totalPages;
            }
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        private static Node[] GetUKKCNews()
        {
            var result = new List<Node>();

            var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://www.uk-kc.ru/peredelkino/?news_count=50");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "GET";

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using var streamReader = new StreamReader(httpResponse.GetResponseStream());

            var html = streamReader.ReadToEnd();

            var parser = new HtmlParser();
            var document = parser.ParseDocument(html);

            var news = document.QuerySelectorAll(".content-padding .news.clearfix");

            foreach (var element in news)
            {
                if (string.IsNullOrWhiteSpace(element.Id))
                {
                    continue;
                }

                foreach (var aElement in element.QuerySelectorAll("a"))
                {
                    var h2Elements = aElement.QuerySelectorAll("h2");

                    if (h2Elements.Length <= 0) continue;

                    var title = h2Elements.First().InnerHtml;
                    var href = aElement.Attributes["href"].Value;

                    result.Add(new Node
                    {
                        id = element.Id,
                        title = title,
                        slug = href,
                        type = NewsSource.uk_kc
                    });

                    break;
                }
            }

            return result.ToArray();
        }
    }
}
