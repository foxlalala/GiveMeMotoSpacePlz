using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;
using System.Data;

namespace GiveMeMotoSpacePlz
{
    class ClawInfoFromWeb
    {
        public ClawInfoFromWeb()
        {
            // 1.抓當月清單
            var fightSpace = ClawItccWebSite(DateTime.Now.ToString("yyyy"), DateTime.Now.AddMonths(0).ToString("MM"));

            // 2.抓次月清單 & 併入
            fightSpace.AddRange(ClawItccWebSite(DateTime.Now.ToString("yyyy"), DateTime.Now.AddMonths(1).ToString("MM")));

            // 3.篩選搶車位的黃領帶
            fightSpace
                .Where(x => x.topic.Contains("永慶")).ToList()
                .ForEach(x => Console.WriteLine(DateTime.Now.ToString("yyyy") + " / " + x.month + " / " + x.day + "\t" + x.topic + "\t" + x.room));

            #if !DEBUG
            Console.WriteLine("!!");
            Console.WriteLine("Hello ~ 請按任意鍵關閉...");
            Console.ReadKey();
            #endif
        }

        private List<TheMeeting> ClawItccWebSite(string searchYear, string searchMonth)
        {
            var result = new List<TheMeeting>();

            // 1.使用 HtmlAgilityPack 分析 XPath
            HtmlWeb webClient = new HtmlWeb();

            // 2.將網址放入在webClient.Load
            HtmlDocument doc = webClient.Load($"http://www.ticc.com.tw/main_ch/EventsCalendar.aspx?uid=146&pid=&YYYY={searchYear}&MM={searchMonth}&DD=01#");

            // 3.取得要分析的 HTML 節點 (div list)
            HtmlNodeCollection divList = doc.DocumentNode.SelectNodes(@"/html/body/div[3]/div/div/div[3]/div");

            foreach (HtmlNode dailyMeetings in divList)
            {
                // 4.div class=list 才有每日的會議清單
                if (!dailyMeetings.GetAttributeValue("class", "").Contains("list"))
                    continue;

                var month = dailyMeetings.SelectNodes("./div[1]/div[1]")[0].InnerText;
                var day = dailyMeetings.SelectNodes("./div[1]/div[2]")[0].InnerText;

                var customList = dailyMeetings.SelectNodes("./div[2]/div");

                foreach (HtmlNode custom in customList)
                {
                    var topic = custom.SelectNodes("./div[1]")[0].InnerText;
                    var room = custom.SelectNodes("./div[2]")[0].InnerText;

                    result.Add(new TheMeeting() { month = month, day = day, topic = topic, room = room });
                }
            }

            return result;
        }

        private class TheMeeting
        {
            public string month { get; set; }

            public string day { get; set; }

            public string topic { get; set; }

            public string room { get; set; }
        }
    }
}
