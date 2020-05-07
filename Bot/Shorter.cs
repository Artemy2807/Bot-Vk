using System;
using BitlyAPI;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Bot
{

    /* Содержит access token для bitly api v4 */
    public struct ShortIn
    {
        public string Key { get; set; }
    }
    
    /* Класс для сокращения ссылок */
    class Shorter
    {
        /* Читаем файл конфигурации для bitly api */
        private static ShortIn lpShorterInfo = JsonConvert.DeserializeObject<ShortIn>(System.IO.File.ReadAllText("shorter.json"));
        
        /* Создаёт сокращённую ссылку */
        private static string GetShortLink(string lpLongUrl, string lpKey)
        {
            var lpShortUrl = "";
            try
            {
                var bitly = new Bitly(lpKey);
                var linkResponse = bitly.PostShorten(lpLongUrl);
                lpShortUrl = linkResponse.Result.Link;
            }
            catch
            {
                lpShortUrl = "error";
            }
            return lpShortUrl;
        }

        /* Вспомогательная функция для создания массива коротких ссылок */
        public static IList<string> UpdateLinks(IList<string> lpLinks)
        {
            IList<string> lpShortLinks = new List<string>();
            foreach (var lpTemp in lpLinks)
            {
                var lpShort = GetShortLink(lpTemp, lpShorterInfo.Key);
                lpShortLinks.Add(lpShort);
            }

            return lpShortLinks;
        }
    }
}