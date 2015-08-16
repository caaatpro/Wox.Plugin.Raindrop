using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using File = System.IO.File;

namespace Wox.Plugin.Raindrop
{

    // работа плагина
    public class Main : IPlugin
    {
        private string _url = "";
        private string _filename = Directory.GetCurrentDirectory() + @"\Plugins\Wox.Plugin.Raindrop\rss.xml";

        public List<Result> Query(Query query)
        {
            var result = new List<Result>();

            if (query.ActionParameters.Count > 0)
            {
                var str = query.ActionParameters[0].ToLower();

                XDocument doc = XDocument.Load(_filename);

                //Console.WriteLine(doc.Descendants("item"));

                foreach (var item in doc.Descendants("item"))
                {
                    var title = item.Element("title").Value;
                    var SubTitle = ""; 
                    foreach (var category in item.Elements("category"))
                    {
                        if (SubTitle != "")
                        {
                            SubTitle += ",";
                        }
                        SubTitle += " " + category.Value;
                    }
                    
                    var link = item.Element("link").Value;

                    if (title.ToLower().Contains(str) || SubTitle.ToLower().Contains(str) ||
                        link.ToLower().Contains(str) ||
                        SubTitle.ToLower().Contains(str))
                    {
                        result.Add(new Result()
                        {
                            Title = title,
                            SubTitle = SubTitle,
                            IcoPath = GetIco(link),
                            Action = c =>
                            {
                                Process.Start(link);
                                return true;
                            }
                        });
                    }
                }
            }

            return result;
        }

        // инициализация
        public void Init(PluginInitContext context)
        {
            // получение ссылки RSS

            var line = File.ReadAllLines(Directory.GetCurrentDirectory() + @"\Plugins\Wox.Plugin.Raindrop\url.txt").First();
            if (line != null)
            {
                _url = line;
            }
            else
            {
                return;
            }

            // скачиваем rss файл
            new WebClient().DownloadFile(_url, _filename);
        }

        // получение иконки
        public string GetIco(string href)
        {
            var cacheFodler = Directory.GetCurrentDirectory() + @"\Plugins\Wox.Plugin.Raindrop\icons";

            if (!href.Contains("://"))
                href = "http://" + href;

            var url = new Uri(href).Host;
            var path = string.Format(@"{0}\{1}.png", cacheFodler, url);
            var ico = new Uri(@"http://favicon.yandex.ru/favicon/" + url);

            // Download the image file            
            if (!File.Exists(path))
                new WebClient().DownloadFile(ico, path);
            return path;
        }

    }
}
