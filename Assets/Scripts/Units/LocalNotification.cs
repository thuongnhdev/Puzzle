using System.Collections.Generic;
using System;
using System.Globalization;
using UnityEngine;
namespace Data
{
    public class LocalNotification
    {
        public readonly List<int> DaysOfWeek;
        public readonly int Hour;
        public readonly int Minute;
        public readonly List<string> Contents;

        public LocalNotification(List<int> day, List<string> content, int hour , int minute, UnityEngine.SystemLanguage lang = UnityEngine.SystemLanguage.English)
        {
            //DaysOfWeek = jsonObj.parseJsonObjList("days_of_week", content => int.Parse(content.ToString()));
            //string HHMM = jsonObj.tryParseJsonString("hh_mm");
            //string keyContent = "contents"; //english

            //if(lang == UnityEngine.SystemLanguage.Unknown)
            //{
            //    if(UnityEngine.Application.systemLanguage == UnityEngine.SystemLanguage.Russian)
            //        keyContent = "contentsRussian";
            //    else if(UnityEngine.Application.systemLanguage == UnityEngine.SystemLanguage.Japanese)
            //        keyContent = "contentsJapanese";
            //    else if(UnityEngine.Application.systemLanguage == UnityEngine.SystemLanguage.ChineseSimplified)
            //        keyContent = "contentsChinese";
            //     else if(UnityEngine.Application.systemLanguage == UnityEngine.SystemLanguage.French)
            //         keyContent = "contentsFrench";
            //     else if(UnityEngine.Application.systemLanguage == UnityEngine.SystemLanguage.German)
            //         keyContent = "contentsGerman";
            //}
            //else if(lang == UnityEngine.SystemLanguage.Russian)
            //    keyContent = "contentsRussian";
            //else if(lang == UnityEngine.SystemLanguage.Japanese)
            //    keyContent = "contentsJapanese";
            //else if(lang == UnityEngine.SystemLanguage.ChineseSimplified)
            //    keyContent = "contentsChinese";
            // else if(lang == UnityEngine.SystemLanguage.French)
            //     keyContent = "contentsFrench";
            // else if(lang == UnityEngine.SystemLanguage.German)
            //     keyContent = "contentsGerman";


            //Contents = jsonObj.parseJsonObjList(keyContent, content => content.ToString());
            //DateTime time = DateTime.ParseExact(HHMM, "HH:mm",
            //CultureInfo.InvariantCulture);

            //Hour = time.Hour;
            //Minute = time.Minute;
            DaysOfWeek = day;
            Contents = content;
            Hour = hour;
            Minute = minute;
        }
        public LocalNotification(string content, DateTime time)
        {
            DaysOfWeek.Add((int)time.DayOfWeek);
            Contents.Add(content);
            Hour = time.Hour;
            Minute = time.Minute;
        }
    }
}
