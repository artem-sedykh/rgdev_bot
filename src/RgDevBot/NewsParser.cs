using System;
using System.IO;
using System.Linq;
using System.Net;
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
            var content = Get();

            var root = JsonConvert.DeserializeObject<Root>(content);
            var latestNews = root.data.allPosts.edges.OrderByDescending(e => e.node.date).ToArray();

            Console.WriteLine($"[{DateTime.Now}] Получено {latestNews.Length} новостей. Самая новая: {latestNews?.FirstOrDefault()?.node.title}.");

            foreach (var post in latestNews)
            {
                if (_config.ConfigValues.Contains(post.node.id))
                {
                    continue;
                }

                try
                {
                    var text = $"{post.node.title}:\r\nhttps://www.absrealty.ru/news/{post.node.slug}/";
                    Console.WriteLine(text);
                    _bot.SendMessage(text);

                    _config.ConfigValues.Add(post.node.id);
                    _config.Save();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        public string Get(int lastNews = 20)
        {
            var totalPages = GetTotalPages();

            if ((totalPages - lastNews) < 0)
                lastNews = totalPages;

            var after = Base64Encode($"arrayconnection:${totalPages - lastNews + 1}");

            var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://www.absrealty.ru/graphql/");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                var json =
                    "{\"query\":\"query allPosts($first: Int, $after: String, $isSpecial: Boolean, $project: String, $category: String, $year: String) {\\n  allPosts(first: $first, after: $after, isSpecial: $isSpecial, project: $project, category: $category, year: $year) " +
                    "{\\n    pageInfo {\\n      endCursor\\n      hasNextPage\\n    }\\n    totalCount\\n    edges {\\n      node {\\n        id\\n        slug\\n        date\\n        title\\n        shortDescription\\n       " +
                    " projects {\\n          id\\n          title\\n          slug\\n          outerSite\\n        }\\n        imagePageNewsDisplay\\n        imagePageNewsPreview\\n        imageDisplay\\n        imagePreview\\n      }\\n    }\\n  }\\n}" +
                    "\\n\",\"variables\":{\"project\":\"peredelkino-blizhnee\",\"first\":" + lastNews + ",\"after\":\"" + after + "\"}}";

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
    }
}
