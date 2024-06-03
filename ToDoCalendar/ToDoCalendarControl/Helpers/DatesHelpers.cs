using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;


namespace ToDoCalendarControl
{
    static class DatesHelpers
    {
        public static string GetLetterFromDayOfWeek(DayOfWeek dayOfWeek)
        {
            if (DoDaysOfWeekRequireMoreSpace())
                return CultureInfo.CurrentUICulture.DateTimeFormat.GetDayName(dayOfWeek).Substring(0, 3).ToUpper();
            else
                return CultureInfo.CurrentUICulture.DateTimeFormat.GetDayName(dayOfWeek).Substring(0, 1).ToUpper();

            //return DateTimeFormatInfo.CurrentInfo.GetDayName(dayOfWeek).Substring(0, 1).ToUpper();
            //return dayOfWeek.ToString().Substring(0, 1).ToUpper();
        }

        public static bool DoDaysOfWeekRequireMoreSpace()
        {
            return CultureInfo.CurrentUICulture.Name.StartsWith("zh");
        }

        public static string GetMonthHeaderText(DateTime firstDayOfMonth)
        {
            return CultureInfo.CurrentUICulture.DateTimeFormat.GetMonthName(firstDayOfMonth.Month).ToUpper() + " " + firstDayOfMonth.Year.ToString();
            //return DateTimeFormatInfo.CurrentInfo.GetMonthName(firstDayOfMonth.Month).ToUpper() + " " + firstDayOfMonth.Year.ToString();
            //return firstDayOfMonth.ToString("MMMM yyyy", CultureInfo.CurrentUICulture);
        }

        public static bool IsWorkDay(DateTime date)
        {
            return (GetWeekdayState(CultureInfo.CurrentCulture, date.DayOfWeek) != WeekdayState.Weekend);
            //return date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday;
        }

        public static bool IsFirstDayOfWeek(DateTime date)
        {
            return date.DayOfWeek == CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;
        }

        public static DateTime GetTodayDateWithoutTime()
        {
            return GetDateWithoutTime(DateTime.Now);
        }

        public static DateTime GetDateWithoutTime(DateTime dateTime)
        {
            return new DateTime(dateTime.Date.Ticks, DateTimeKind.Unspecified);
        }
        

        //-------------------------------------------
        // NOTE: THE CODE BELOW WAS ADAPTED FROM: http://stackoverflow.com/questions/2019098/finding-weekend-days-based-on-culture
        //-------------------------------------------

        // The weekday/weekend state for a given day.
        public enum WeekdayState
        {
            /// <summary>
            /// A work day.
            /// </summary>
            Workday,
            /// <summary>
            /// A weekend.
            /// </summary>
            Weekend,
            /// <summary>
            /// Morning is a workday, afternoon is the start of the weekend.
            /// </summary>
            WorkdayMorning
        }

        // Returns the English version of the country name. Extracted from the CultureInfo.EnglishName.
        public static string GetCountryEnglishName(CultureInfo ci)
        {
            string[] parts = ci.EnglishName.Split(new[] {'(', ')'}, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2)
                return ci.EnglishName;
            parts = parts[1].Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
            return parts[parts.Length - 1].Trim();
        }

        // Returns the English version of the language name. Extracted from the CultureInfo.EnglishName.
        public static string GetLanguageEnglishName(CultureInfo ci)
        {
            string[] parts = ci.EnglishName.Split(new[] {'('}, StringSplitOptions.RemoveEmptyEntries);
            return parts[0].Trim();
        }

        // Return if the passed in day of the week is a weekend.
        // note: state pulled from http://en.wikipedia.org/wiki/Workweek_and_weekend
        public static WeekdayState GetWeekdayState(CultureInfo ci, DayOfWeek day)
        {
            string[] items = ci.Name.Split(new[] {'-'}, StringSplitOptions.RemoveEmptyEntries);
            switch (items[items.Length - 1])
            {
                case "DZ": // Algeria
                case "BH": // Bahrain
                case "BD": // Bangladesh
                case "EG": // Egypt
                case "IQ": // Iraq
                case "IL": // Israel
                case "JO": // Jordan
                case "KW": // Kuwait
                case "LY": // Libya
                // Northern Malaysia (only in the states of Kelantan, Terengganu, and Kedah)
                case "MV": // Maldives
                case "MR": // Mauritania
                case "NP": // Nepal
                case "OM": // Oman
                case "QA": // Qatar
                case "SA": // Saudi Arabia
                case "SD": // Sudan
                case "SY": // Syria
                case "AE": // U.A.E.
                case "YE": // Yemen
                    return day == DayOfWeek.Thursday || day == DayOfWeek.Friday
                        ? WeekdayState.Weekend
                        : WeekdayState.Workday;

                case "AF": // Afghanistan
                case "IR": // Iran
                    if (day == DayOfWeek.Thursday)
                        return WeekdayState.WorkdayMorning;
                    return day == DayOfWeek.Friday ? WeekdayState.Weekend : WeekdayState.Workday;

                case "BN": // Brunei Darussalam
                    return day == DayOfWeek.Friday || day == DayOfWeek.Sunday
                        ? WeekdayState.Weekend
                        : WeekdayState.Workday;

                case "MX": // Mexico
                case "TH": // Thailand
                    if (day == DayOfWeek.Saturday)
                        return WeekdayState.WorkdayMorning;
                    return day == DayOfWeek.Saturday || day == DayOfWeek.Sunday
                        ? WeekdayState.Weekend
                        : WeekdayState.Workday;

            }

            // most common Saturday/Sunday
            return day == DayOfWeek.Saturday || day == DayOfWeek.Sunday ? WeekdayState.Weekend : WeekdayState.Workday;
        }
    }
}
