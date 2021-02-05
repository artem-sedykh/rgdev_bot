using System.Collections.Generic;
using System.Text;

namespace RgDevBot.ObjectModel
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 

    public class NewsListResponse
    {
        public int count { get; set; }
        public string next { get; set; }
        public object previous { get; set; }
        public List<News> results { get; set; }
    }

}
