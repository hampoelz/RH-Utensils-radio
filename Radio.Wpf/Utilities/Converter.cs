using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Radio.Wpf.Utilities
{
    public static class Converter
    {
        public static class Online
        {

            public static (string title, string file) Youtube(string link)
            {
                if (new Uri(link).Host != new Uri("https://www.youtube.com/").Host) return (null, null);

                var wc = new WebClient();
                var json = wc.DownloadString("http://michaelbelgium.me/ytconverter/convert.php?youtubelink=" + link);

                if (!json.StartsWith("{") && !json.EndsWith("}")) return (null, null);

                var jsonData = JObject.Parse(json);

                if (jsonData["error"].ToString() == "true") return (null, null);

                var title = jsonData["title"].ToString();
                var url = jsonData["file"].ToString();

                return (title, url);
            }
        }
    }
}
