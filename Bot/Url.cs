using System;

namespace Bot
{
    /* Класс для обработки ссылок вк */
    public class Url
    {
        /* Вырезает главную часть ссылки на страницу вк */
        public static string GetShortId(string lpUrl)
        {
            if (lpUrl.Length <= 15) return "";
            string lpShort = lpUrl.Substring(15);
            Console.WriteLine("Short: {0}", lpShort);
            return lpShort;
        }
    }
}