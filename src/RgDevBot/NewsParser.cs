using System;
using System.Collections.Generic;
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
        private int _lastSentId = -1;

        public NewsParser(TelegramBot bot, SentConfig config)
        {
            _bot = bot;
            _config = config;
        }

        public void Parse()
        {
            var url = "https://rg-dev.ru/api/news/?type=news";
            var content = Get(url);

            var news = JsonConvert.DeserializeObject<NewsListResponse>(content);
            var latestNews = news?.results;

            Console.WriteLine($"[{DateTime.Now}] Получено {latestNews?.Count} новостей. Самая новая: {latestNews?.FirstOrDefault()?.id}.");

            foreach (var post in latestNews ?? new List<News>())
            {
                if (_config.ConfigValues.Contains(post.id))
                {
                    continue;
                }

                try
                {
                    var text = $"{post.title}:\r\nhttps://rg-dev.ru/press/news/all/{post.id}/";
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

        public string Get(string url, string contentType = "application/json")
        {
            var httpRequest = (HttpWebRequest)WebRequest.Create(url);
            httpRequest.ContentType = contentType;
            httpRequest.Method = "GET";

            var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                return result;
            }
        }
    }
}
