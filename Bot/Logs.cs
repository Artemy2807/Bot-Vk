using System;

namespace Bot
{
    /* Класс для вывода log в консоль */
    public class Logs
    {
        /* Выводит log в консоль */
        public static void Log(string lpText)
        {
            Console.WriteLine("[LOG]--> {0}", lpText);
        }
    }
}