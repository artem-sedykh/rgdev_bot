using System;
using System.Threading;
using NLog;
using RgDevBot.Config;

namespace RgDevBot
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new SentConfig(LogManager.GetCurrentClassLogger());
            var bot = new TelegramBot();
            var parser = new NewsParser(bot, config);
            while (true)
            {
                try
                {
                    parser.Parse();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                Thread.Sleep(TimeSpan.FromSeconds(60));
            }
        }
    }
}
