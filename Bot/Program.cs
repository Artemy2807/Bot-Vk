using VkNet;
using System;
using VkNet.Model;
using System.Linq;
using Newtonsoft.Json;
using VkNet.Enums.Filters;
using VkNet.Enums.SafetyEnums;
using VkNet.Model.RequestParams;
using System.Collections.Generic;

namespace Bot
{
    /* Коды часто используемых смайликов */
    public struct Emoticons
    {
        public static string CheckMark = "&#9989;",
            Number = "&#8419;",
            I = "&#8505;",
            Title = "&#10134;&#10134;&#10134;&#10134;",
            Cross = "&#10060;";
    }

    /* Содержит id группы и access token для подключения бота */
    public class GroupSettings
    {
        public string AccessToken { get; set; }
        public ulong Id { get; set; }
    }
    
    /* Содержит ответы бота пользователям */
    public class MessagesUser
    {
        public string UserSupport { get; set; }
        public string HelpMenuUser { get; set; }
        public string HelpMenuAdmin { get; set; }
        public string AboutMenu { get; set; }
        public string PeopleNotFound { get; set; }
        public string NotDeleteAdmin { get; set; }
        public string BannedSuccses { get; set; }
        public string NotInList { get; set; }
        public string UnbannedSuccses { get; set; }
        public string NotList { get; set; }
        public string AnswerCommand { get; set; }
        public string CommandPrefixNot { get; set; }
        public string DeleteAdminSuccses { get; set; }
        public string AddAdminSuccses { get; set; }
        public IList<string> MapCommand { get; set; }
        public IList<string> MapNames { get; set; }
        public IList<string> BunnyHoop { get; set; }
        public IList<string> Surf { get; set; }
        public IList<string> Aim { get; set; }
        public IList<string> Vs { get; set; }
        public IList<string> CoopMission { get; set; }
    }

    /* Главный класс программы */
    class Program
    {
        
        private static VkApi api = new VkApi();

        /* Сообщения бота */
        private static MessagesUser _lpMessagesUserObject =
            JsonConvert.DeserializeObject<MessagesUser>(System.IO.File.ReadAllText("messages.json"));
        
        /* Админы */
        private static Admins _lpBotAdmins =
            JsonConvert.DeserializeObject<Admins>(System.IO.File.ReadAllText("admins.json"));
        
        /* Забаненые пользователи */
        private static BannedUsers _lpBannedUsers =
            JsonConvert.DeserializeObject<BannedUsers>(System.IO.File.ReadAllText("banned.json"));
        
        /* Настройки подключения к сообществу */
        private static GroupSettings _lpGroupSettings =
            JsonConvert.DeserializeObject<GroupSettings>(System.IO.File.ReadAllText("group.json"));
        
        /* Класс содержащий функции для администраторов */
        private static Admin lpAdmins = new Admin(_lpBotAdmins, _lpBannedUsers);

        /* Содержет ссылки для пользователей */
        private static IList<IList<string>> _lpLinks = new List<IList<string>>();
        
        static IList<string> lpCommands = new List<string>()
        {
            /* Команды для показа карт */
            "bunny", 
            "surf", 
            "aim", 
            "vs", 
            "mission", 
            /* Команды для обычного пользователя  */
            "help", 
            "помощь", 
            "about", 
            "maps", 
            /* Команды для администратора */
            "banned", 
            "unbanned", 
            "print", 
            "admin"
        };
        
        /* Главная функция программы */
        private static void Main(string[] args)
        {
            Logs.Log("Получаем сокращённые ссылки.");
            /* Получаем сокращённые ссылки */
            _lpLinks = CreateLinks();
            Logs.Log("Подключаемся к серверу, группе.");
            /* Авторизуемся через в вк Access token, который хранится в файле group.json */
            api.Authorize(new ApiAuthParams()
            {
                AccessToken = _lpGroupSettings.AccessToken
            });
            
            /* Подключаемся к серверу long poll api для получения сообщений */
            var s = api.Groups.GetLongPollServer(_lpGroupSettings.Id);
            /* В данном цикле будут обрабатываться сообщения пользователей */
            while (true)
            {
                /* Получаем новые события из группы */
                var poll = api.Groups.GetBotsLongPollHistory(
                    new BotsLongPollHistoryParams() {Server = s.Server, Ts = s.Ts, Key = s.Key, Wait = 25});
                /* Проверяем на наличие новых событий */
                if (poll?.Updates == null) continue;
                foreach (var a in poll.Updates)
                {
                    /* Обрабатываем, только сообщения пользователей */
                    if (a.Type == GroupUpdateType.MessageNew)
                    {
                        ProcessingMessage(a.Message.Text.ToLower(), a.Message.FromId);
                    }

                }

                s.Ts = poll.Ts;
            }
        }
        
        /* Реакция на сообщения пользователей */
        public static void ProcessingMessage(string lpUserMessage, long? lpUserDomain)
        { 
            /* Выводим информацию в log */
            Logs.Log("User id: " + lpUserDomain + ", User message: " + lpUserMessage);
            /* Переменные */
            IList<string> lpKeys = new List<string>();
            string lpAnswer = "";
            bool lpIsAdmin = lpAdmins.IsAdmin(lpUserDomain);
            /* Определяем, что за команда и наличие флагов к ней */
            int lpCommandNumber = GetCommand(lpUserMessage, lpCommands, ref lpKeys);
            if (lpAdmins.IsBanned(lpUserDomain)) return;
            /* Реакция на команду пользователя */
            switch (lpCommandNumber)
            {
                /* Команды: help, помощь */
               case 5:
               case 6:
               {
                   lpAnswer += Emoticons.Title + " Команды " + Emoticons.Title + "\n";
                   if(!lpIsAdmin) lpAnswer += _lpMessagesUserObject.HelpMenuUser;
                   else lpAnswer += _lpMessagesUserObject.HelpMenuUser + _lpMessagesUserObject.HelpMenuAdmin;
                   lpAnswer += _lpMessagesUserObject.AnswerCommand;
                   break;
               }
               /* Команда: maps */
               case 8:
               {
                   lpAnswer += Emoticons.Title + " Карты " + Emoticons.Title + "\n";
                   for (var i = 0; i < _lpMessagesUserObject.MapNames.Count; i++)
                   {
                       lpAnswer += Emoticons.CheckMark + " " + _lpMessagesUserObject.MapCommand[i] + " - " + _lpMessagesUserObject.MapNames[i] + "\n";
                   }

                   lpAnswer += _lpMessagesUserObject.AnswerCommand;

                   break;
               }
               /* Команды: bunny, surf, aim, vs, mission */
               case 0:
               case 1:
               case 2:
               case 3:
               case 4:
               {
                   lpAnswer += Emoticons.Title + " " + _lpMessagesUserObject.MapNames[lpCommandNumber] + " " + Emoticons.Title + "\n";
                   PrintLinks(_lpLinks[lpCommandNumber], ref lpAnswer);
                   break;
               }
               /* Команда: about */
               case 7:
               {
                   lpAnswer += Emoticons.Title + " О группе " + Emoticons.Title + "\n";
                   lpAnswer += _lpMessagesUserObject.AboutMenu;
                   break;
               }
               /* Команады: banned, unbanned */
               case 9:
               case 10:
               {
                   if (lpIsAdmin)
                   {
                       // Получаем из команды URL пользователя для бана/разбана
                       var lpUrl = lpKeys[0];
                       // Получаем главную часть URL пользователя вк
                       var lpShort = Url.GetShortId(lpUrl);
                       // Получаем по URL пользователя информацию о странице
                       long lpUserId;
                       try
                       {
                           var lpPerson = api.Users.Get(new string[] { lpShort }).FirstOrDefault();
                           lpUserId = lpPerson.Id;
                       }
                       catch
                       {
                           lpAnswer = Emoticons.Cross + " " + _lpMessagesUserObject.PeopleNotFound;
                           break;
                       }

                       if (lpCommandNumber == 9)
                       {
                           var lpIsAdminBanned = lpAdmins.IsAdmin(lpUserId);
                           if (lpIsAdminBanned)
                           {
                               lpAnswer = Emoticons.Cross + " " + _lpMessagesUserObject.NotDeleteAdmin;
                               break;
                           }
                           lpAdmins.BannedPeople(lpUserId, ref _lpBannedUsers);
                           lpAnswer = _lpMessagesUserObject.BannedSuccses;
                       }
                       else
                       {
                           if(lpAdmins.UnbannedPeople(lpUserId, ref _lpBannedUsers)) lpAnswer = _lpMessagesUserObject.UnbannedSuccses;
                           else lpAnswer = Emoticons.Cross + " " + _lpMessagesUserObject.NotInList;
                       }
                   }
                   break;
               }
               /* Команда: print */
               case 11:
               {
                   if (!lpIsAdmin)
                   {
                       lpAnswer = _lpMessagesUserObject.UserSupport;
                       break;
                   }
                   switch (lpKeys[0])
                   {
                       case "admins":
                       {
                           lpAnswer += Emoticons.Title + " Admins " + Emoticons.Title + "\n";
                           IList<string> lpNames = new List<string>();
                           GetFullName(_lpBotAdmins.AdminsBot, ref lpNames);
                           PrintLinks(lpNames, ref lpAnswer);
                           break;
                       }
                       case "banned":
                       {
                           lpAnswer += Emoticons.Title + " Banned " + Emoticons.Title + "\n";
                           IList<string> lpNames = new List<string>();
                           GetFullName(_lpBannedUsers.Banned, ref lpNames);
                           PrintLinks(lpNames, ref lpAnswer);
                           break;
                       }
                       default:
                       {
                           lpAnswer = Emoticons.Cross + " " + _lpMessagesUserObject.NotList;
                           break;
                       }
                   }
                   break;
               }
               /* Команда: admin */
               case 12:
               {
                   switch (lpKeys[0])
                   {
                       case "add":
                       case "delete":
                       {
                           string lpUrl = lpKeys[1];
                           string lpShort = Url.GetShortId(lpUrl);
                           long lpUserId;
                           try
                           {
                               User lpPerson = api.Users.Get(new string[] { lpShort }).FirstOrDefault();
                               lpUserId = lpPerson.Id;
                           }
                           catch
                           {
                               lpAnswer = Emoticons.Cross + " " + _lpMessagesUserObject.PeopleNotFound;
                               break;
                           }

                           if (lpKeys[0] == "delete")
                           {
                               if(lpAdmins.DeleteAdmin(lpUserId, ref _lpBotAdmins)) lpAnswer = _lpMessagesUserObject.DeleteAdminSuccses;
                               else lpAnswer = Emoticons.Cross + " " + _lpMessagesUserObject.NotInList;
                           }
                           else
                           {
                               lpAdmins.AddAdmin(lpUserId, ref _lpBotAdmins);
                               lpAnswer = _lpMessagesUserObject.AddAdminSuccses;
                           }
                           break;
                       }
                       default:
                       {
                           lpAnswer = Emoticons.Cross + " " + _lpMessagesUserObject.CommandPrefixNot;
                           PrintLinks(new List<string>() {"add", "delete"}, ref lpAnswer);
                           break;
                       }
                   }
                   break;
               }
               /* Неизвестная команда */
               case -1:
               {
                   lpAnswer = _lpMessagesUserObject.UserSupport;
                   break;
               }
            }
            /* Отправляем сообщение пользователю */
            SendMessage(lpAnswer, lpUserDomain);
        }

        /* Отправление сообщений пользователям */
        private static void SendMessage(string message, long? lpUserDomain)
        {
            var rnd = new Random();
            api.Messages.Send(new MessagesSendParams
            {
                RandomId = rnd.Next(),
                UserId = lpUserDomain,
                Message = message
            });
        }

        /* Создаёт короткии ссылки с помощью bitly api v4 */
        private static IList<IList<string>> CreateLinks()
        {
            IList<IList<string>> lpLinks = new List<IList<string>>();
            lpLinks.Add(Shorter.UpdateLinks(_lpMessagesUserObject.BunnyHoop));
            lpLinks.Add(Shorter.UpdateLinks(_lpMessagesUserObject.Surf));
            lpLinks.Add(Shorter.UpdateLinks(_lpMessagesUserObject.Aim));
            lpLinks.Add(Shorter.UpdateLinks(_lpMessagesUserObject.Vs));
            lpLinks.Add(Shorter.UpdateLinks(_lpMessagesUserObject.CoopMission));
            return lpLinks;
        }

        /* Оформляет массив ссылок для отправки пользователю */
        private static void PrintLinks(IList<string> lpLinks, ref string lpInput)
        {
            int lpIndex = 1;
            foreach (var lpLink in lpLinks)
            {
                lpInput += (lpIndex < 10 ? lpIndex.ToString() + Emoticons.Number : Emoticons.I) + " " + lpLink + "\n";
                lpIndex++;
            }
        }

        private static int GetCommand(string lpCommand, IList<string> lpSources, ref IList<string> lpKey)
        {
            var lpWords= lpCommand.Split(new char[]{' '}); 
            for (var i = 0; i < lpSources.Count; i++)
            {
                if (lpWords[0] == lpSources[i])
                {
                    for(var j = 1; j < lpWords.Length; j++) lpKey.Add(lpWords[j]);
                    return i;
                }
            }
            return -1;
        }
        
        /* Получение полного имени пользователя по его id*/
        private static void GetFullName(IList<string> lpUsersId, ref IList<string> lpNames)
        {
            foreach (var lpUserId in lpUsersId)
            {
                var lpPerson = api.Users.Get(new string[] { lpUserId }, ProfileFields.Domain).FirstOrDefault();
                if (lpPerson == null) continue;
                lpNames.Add(lpPerson.FirstName + " " + lpPerson.LastName + ", https://vk.com/" + lpPerson.Domain);
            }
            
        }
    }
}