using System.Collections.Generic;
using NLog;

namespace RgDevBot.Config
{
    public class SentConfig: AbstractConfig<List<int>>
    {
        public SentConfig(ILogger logger) : base(logger, "sent.json")
        {
        }
    }
}
