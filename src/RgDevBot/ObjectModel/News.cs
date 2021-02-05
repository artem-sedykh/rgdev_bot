using System;

namespace RgDevBot.ObjectModel
{
    public class News
    {
        public int id { get; set; }
        public string title { get; set; }
        public string slug { get; set; }
        public string type { get; set; }
        public string intro { get; set; }
        public object image_display { get; set; }
        public object mobile_image_display { get; set; }
        public object image_preview { get; set; }
        public object mobile_image_preview { get; set; }
        public DateTime date { get; set; }
    }
}