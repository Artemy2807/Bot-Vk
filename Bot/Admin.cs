using System;
using System.IO;
using System.Linq;
using VkNet;
using VkNet.Model;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Bot
{
    /* Содержит администраторов */
    public class Admins
    {
        public IList<string> AdminsBot { get; set; }
    }
    
    /* Содержит забаненых пользователей */
    public class BannedUsers
    {
        public IList<string> Banned { get; set; }
    }
    
    /* Обработка команд администратора */
    public class Admin
    {
        /* Переменные */
        private static string lpBannedUserFile = "banned.json",
            lpAdminFile = "admins.json";
        private static Admins _lpAdmins;
        private static BannedUsers _lpBanned;
        
        /* Конструктор */
        public Admin(Admins lpSourcesAdmin, BannedUsers lpSourcesBanned)
        {
            _lpAdmins = lpSourcesAdmin;
            _lpBanned = lpSourcesBanned;
        }
        
        /* Проверяет, является ли человек с lpUserId администратором */
        public bool IsAdmin(long? lpUserId)
        {
            return (_lpAdmins.AdminsBot.IndexOf(lpUserId.ToString()) >=0);
        }

        /* Проверяет, забанен ли пользователь с lpUserId */
        public bool IsBanned(long? lpUserId)
        {
            return (_lpBanned.Banned.IndexOf(lpUserId.ToString()) >=0);
        }

        /* Добавляет человека в список администраторов */
        public void AddAdmin(long? lpUserId, ref Admins lpAdminsIn)
        {
            lpAdminsIn.AdminsBot.Add(lpUserId.ToString());
            var lpResult = JsonConvert.SerializeObject(lpAdminsIn, Newtonsoft.Json.Formatting.Indented);
            using (var sw = new StreamWriter(lpAdminFile, false, System.Text.Encoding.Default))
            {
                sw.WriteLineAsync(lpResult);
            }

            _lpAdmins = lpAdminsIn;
        }
        
        /* Удаляет человека из списка администраторов */
        public bool DeleteAdmin(long? lpUserId, ref Admins lpAdminsIn)
        {
            var lpStatus = lpAdminsIn.AdminsBot.Remove(lpUserId.ToString());
            if (!lpStatus) return lpStatus;
            var lpResult = JsonConvert.SerializeObject(lpAdminsIn, Newtonsoft.Json.Formatting.Indented);
            using (var sw = new StreamWriter(lpAdminFile, false, System.Text.Encoding.Default))
            {
                sw.WriteLineAsync(lpResult);
            }
            _lpAdmins = lpAdminsIn;
            return lpStatus;
        }

        /* Вносит человека в черный список */
        public void BannedPeople(long? lpUserId, ref BannedUsers lpList)
        {
            lpList.Banned.Add(lpUserId.ToString());
            var lpResult = JsonConvert.SerializeObject(lpList, Newtonsoft.Json.Formatting.Indented);
            using (var sw = new StreamWriter(lpBannedUserFile, false, System.Text.Encoding.Default))
            {
                sw.WriteLineAsync(lpResult);
            }

            _lpBanned = lpList;
        }

        /* Удаляет человека из чёрного листа */
        public bool UnbannedPeople(long? lpUserId, ref BannedUsers lpList)
        {
            var lpStatus = lpList.Banned.Remove(lpUserId.ToString());
            if (!lpStatus) return lpStatus;
            var lpResult = JsonConvert.SerializeObject(lpList, Newtonsoft.Json.Formatting.Indented);
            using (var sw = new StreamWriter(lpBannedUserFile, false, System.Text.Encoding.Default))
            {
                sw.WriteLineAsync(lpResult);
            }
            _lpBanned = lpList;
            return lpStatus;
        }
        
    }
}