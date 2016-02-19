using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NodaTime;
using NodaTime.Text;

namespace DEXS.IO.CSV.Extensions
{
    public static class DateTimeExtensions
    {
        private static readonly OffsetPattern GmtPattern = OffsetPattern.Create("'GMT'+HH:mm", CultureInfo.InvariantCulture);
        private static readonly OffsetPattern UtcPattern = OffsetPattern.Create("'UTC'+HH:mm", CultureInfo.InvariantCulture);

        private static readonly Dictionary<string, string> AdCenterMicrosoftTimeZones = new Dictionary<string, string>
        {
            {"ABUDHABIMUSCAT", "Asia/Muscat"},
            {"ADELAIDE", "Australia/Adelaide"},
            {"ALASKA", "US/Alaska"},
            {"ALMATYNOVOSIBIRSK", "Asia/Almaty"},
            {"ALMATY_NOVOSIBIRSK", "Asia/Almaty"},
            {"AMSTERDAMBERLINBERNROMESTOCKHOLMVIENNA", "Europe/Amsterdam"},
            {"ARIZONA", "US/Arizona"},
            {"ASTANADHAKA", "Asia/Dhaka"},
            {"ATHENSBUCKARESTISTANBUL", "Europe/Athens"},
            {"ATHENSISLANDANBULMINSK", "Europe/Athens"},
            {"ATLANTICTIMECANADA", "Canada/Atlantic"},
            {"AUCKLANDWELLINGTON", "Pacific/Auckland"},
            {"AZORES", "Atlantic/Azores"},
            {"BAGHDAD", "Asia/Baghdad"},
            {"BAKUTBILISIYEREVAN", "Asia/Baku"},
            {"BANGKOKHANOIJAKARTA", "Asia/Bangkok"},
            {"BEIJINGCHONGQINGHONGKONGURUMQI", "Asia/Hong_Kong"},
            {"BELGRADEBRATISLAVABUDAPESTLJUBLJANAPRAGUE", "Europe/Belgrade"},
            {"BOGOTALIMAQUITO", "America/Bogota"},
            {"BRASILIA", "Brazil/East"},
            {"BRISBANE", "Australia/Brisbane"},
            {"BRUSSELSCOPENHAGENMADRIDPARIS", "Europe/Brussels"},
            {"BUCHAREST", "Europe/Bucharest"},
            {"BUENOSAIRESGEORGETOWN", "America/Buenos_Aires"},
            {"CAIRO", "Africa/Cairo"},
            {"CANBERRAMELBOURNESYDNEY", "Australia/Canberra"},
            {"CAPEVERDEISLAND", "Atlantic/Cape_Verde"},
            {"CARACASLAPAZ", "America/La_Paz"},
            {"CASABLANCAMONROVIA", "Africa/Monrovia"},
            {"CENTRALAMERICA", "US/Central"},
            {"CENTRALTIMEUSCANADA", "US/Central"},
            {"CHENNAIKOLKATAMUMBAINEWDELHI", "Asia/Calcutta"},
            {"CHIHUAHUALAPAZMAZATLAN", "America/Chihuahua"},
            {"DARWIN", "Australia/Darwin"},
            {"EASTERNTIMEUSCANADA", "US/Eastern"},
            {"EKATERINBURG", "Asia/Yekaterinburg"},
            {"FIJIKAMCHATKAMARSHALLISLAND", "Pacific/Fiji"},
            {"GREENLAND", "America/Godthab"},
            {"GREENWICHMEANTIMEDUBLINEDINBURGHLISBONLONDON", "Europe/London"},
            {"GUADALAJARAMEXICOCITYMONTERREY", "America/Monterrey"},
            {"GUAMPORTMORESBY", "Pacific/Guam"},
            {"HARAREPRETORIA", "Africa/Harare"},
            {"HAWAII", "US/Hawaii"},
            {"HELSINKIKYIVRIGASOFIATALLINNVILNIUS", "Europe/Helsinki"},
            {"HOBART", "Australia/Hobart"},
            {"INDIANAEAST", "US/East-Indiana"},
            {"INTERNATIONALDATELINEWEST", "Etc/GMT+12"},
            {"IRKUTSKULAANBATAAR", "Asia/Urumqi"},
            {"ISLAMABADKARACHITASHKENT", "Asia/Karachi"},
            {"JERUSALEM", "Asia/Jerusalem"},
            {"KABUL", "Asia/Kabul"},
            {"KATHMANDU", "Asia/Katmandu"},
            {"KRASNOYARSK", "Asia/Krasnoyarsk"},
            {"KUALALUMPURSINGAPORE", "Asia/Singapore"},
            {"KUWAITRIYADH", "Asia/Kuwait"},
            {"MAGADANSOLOMONISLANDNEWCALEDONIA", "Asia/Magadan"},
            {"MIDATLANTIC", "Atlantic/South_Georgia"},
            {"MIDWAYISLANDANDSAMOA", "Pacific/Midway"},
            {"MIDWAYISLANDAND_SAMOA", "Pacific/Midway"},
            {"MOSCOWSTPETERSBURGVOLGOGRAD", "Europe/Moscow"},
            {"MOUNTAINTIMEUSCANADA", "US/Mountain"},
            {"MOUNTAINTIME_US_CANADA", "US/Mountain"},
            {"NAIROBI", "Africa/Nairobi"},
            {"NEWFOUNDLAND", "Canada/Newfoundland"},
            {"NUKUALOFA", "Pacific/Tongatapu"},
            {"OSAKASAPPOROTOKYO", "Asia/Tokyo"},
            {"PACIFICTIMEUSCANADATIJUANA", "America/Los_Angeles"},
            {"PERTH", "Australia/Perth"},
            {"RANGOON", "Asia/Rangoon"},
            {"SANTIAGO", "America/Santiago"},
            {"SARAJEVOSKOPJEWARSAWZAGREB", "Europe/Sarajevo"},
            {"SASKATCHEWAN", "Canada/Saskatchewan"},
            {"SEOUL", "Asia/Seoul"},
            {"SRIJAYAWARDENEPURA", "Asia/Colombo"},
            {"TAIPEI", "Asia/Taipei"},
            {"TEHRAN", "Asia/Tehran"},
            {"VLADIVOSTOK", "Asia/Vladivostok"},
            {"WESTCENTRALAFRICA", "Africa/Lagos"},
            {"YAKUTSK", "Asia/Yakutsk"}
        };

        public static DateTimeZone AsDateTimeZone(this string timeZoneId)
        {
            var normalizedTimeZoneId = AdCenterMicrosoftTimeZones.ContainsKey(timeZoneId.ToUpper())
                ? AdCenterMicrosoftTimeZones[timeZoneId.ToUpper()]
                : timeZoneId;
            var dateTimeZone =
                DateTimeZoneProviders.Tzdb.GetZoneOrNull(normalizedTimeZoneId) ??
                DateTimeZoneProviders.Bcl.GetZoneOrNull(normalizedTimeZoneId);
            if (dateTimeZone != null)
            {
                return dateTimeZone;
            }
            var gmtResult = GmtPattern.Parse(normalizedTimeZoneId);
            var utcResult = UtcPattern.Parse(normalizedTimeZoneId);
            if (!gmtResult.Success && !utcResult.Success)
                throw new InvalidPatternException($"Could not parse timezone id '{timeZoneId}'.");
            var offset = gmtResult.Success ? gmtResult.Value : utcResult.Value;
            dateTimeZone = DateTimeZone.ForOffset(offset);
            return dateTimeZone;
        }

        public static ZonedDateTime AsZonedDateTime(this DateTime dateToConvert, string timeZoneId)
        {
            var normalizedTimeZoneId = AdCenterMicrosoftTimeZones.ContainsKey(timeZoneId.ToUpper())
                ? AdCenterMicrosoftTimeZones[timeZoneId.ToUpper()]
                : timeZoneId;
            var dateTimeZone =
                DateTimeZoneProviders.Tzdb.GetZoneOrNull(normalizedTimeZoneId) ??
                DateTimeZoneProviders.Bcl.GetZoneOrNull(normalizedTimeZoneId);
            Instant instant;
            ZonedDateTime zonedDateTime;
            if (dateTimeZone != null)
            {
                instant = Instant.FromDateTimeUtc(dateToConvert.ToUniversalTime());
                zonedDateTime = instant.InZone(dateTimeZone);
                return zonedDateTime;
            }
            var gmtResult = GmtPattern.Parse(normalizedTimeZoneId);
            var utcResult = UtcPattern.Parse(normalizedTimeZoneId);
            if (!gmtResult.Success && !utcResult.Success)
                throw new InvalidPatternException($"Could not parse timezone id '{timeZoneId}'.");
            var offset = gmtResult.Success ? gmtResult.Value : utcResult.Value;
            instant = Instant.FromDateTimeUtc(dateToConvert.ToUniversalTime());
            zonedDateTime = instant.InZone(DateTimeZone.ForOffset(offset));
            return zonedDateTime;
        }

        /// <summary>
        ///     Converts a DateTime to a DateTimeOffSet based on the AdWords timezone id.
        ///     here the timezoneid can be either formatted as
        ///     America/Chicago
        ///     or
        ///     GMT-08:00, GMT+01:00
        ///     use first schedule since all the same account timezone id
        /// </summary>
        /// <param name="dateToConvert"></param>
        /// <param name="accountTimezoneId"></param>
        /// <returns></returns>
        public static DateTimeOffset AsDateTimeOffset(this DateTime dateToConvert, string accountTimezoneId)
        {
            accountTimezoneId = AdCenterMicrosoftTimeZones.ContainsKey(accountTimezoneId.ToUpper())
                ? AdCenterMicrosoftTimeZones[accountTimezoneId.ToUpper()]
                : accountTimezoneId;
            var dateTimeZone = DateTimeZoneProviders.Tzdb.Ids
                .Where(id => string.Compare(id, accountTimezoneId, StringComparison.InvariantCultureIgnoreCase) == 0)
                .Select(id => DateTimeZoneProviders.Tzdb[id])
                .FirstOrDefault();
            if (dateTimeZone != null)
            {
                // was like America/Chicago
                var asOfAdWordsTimezone = Instant.FromDateTimeUtc(dateToConvert.ToUniversalTime())
                    .InZone(dateTimeZone)
                    .ToDateTimeOffset();
                return asOfAdWordsTimezone;
            }
            var gmtResult = GmtPattern.Parse(accountTimezoneId);
            var utcResult = UtcPattern.Parse(accountTimezoneId);
            if (!gmtResult.Success && !utcResult.Success)
            {
                throw new InvalidPatternException($"Could not parse timezone id {accountTimezoneId}.");
            }
            // was GMT-08:00 or UTC-08:00
            var offset = gmtResult.Success ? gmtResult.Value : utcResult.Value;
            var dateTimeOffset = Instant.FromDateTimeUtc(dateToConvert.ToUniversalTime())
                .InZone(DateTimeZone.ForOffset(offset))
                .ToDateTimeOffset();
            return dateTimeOffset;
        }

        /// <summary>
        ///     AsOffset gets the offset of the timezone parameter as it was as of the reference date.
        /// </summary>
        /// <param name="dateToConvert"></param>
        /// <param name="accountTimezoneId"></param>
        /// <returns></returns>
        public static TimeSpan AsOffset(this DateTime dateToConvert, string accountTimezoneId)
        {
            return dateToConvert.AsDateTimeOffset(accountTimezoneId).Offset;
        }

        public static DateTime FirstDayOfMonth(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, 1);
        }

        public static DateTime AtEndOfDay(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0, dateTime.Kind).AddDays(1).AddMilliseconds(-1);
        }

        public static DateTime LastDayOfMonth(this DateTime dateTime)
        {
            var daysInMonth = DateTime.DaysInMonth(dateTime.Year, dateTime.Month);
            return new DateTime(dateTime.Year, dateTime.Month, daysInMonth);
        }

        public static IEnumerable<DateTime> EachDayUntil(this DateTime from, DateTime thru)
        {
            for (var day = from.Date; day.Date <= thru.Date; day = day.AddDays(1))
            {
                yield return day;
            }
        }

        public static LocalDate AsLocalDate(this DateTime date)
        {
            return new LocalDate(date.Year, date.Month, date.Day);
        }

        public static IEnumerable<DateTimeOffset> EachDayUntil(this DateTime from, DateTime thru, string accountTimezoneId)
        {
            for (var day = from.Date; day.Date <= thru.Date; day = day.AddDays(1))
            {
                var offset = day.AsOffset(accountTimezoneId);
                yield return new DateTimeOffset(day, offset);
            }
        }

        public static DateTimeOffset AsDateTimeOffset(this DateTime dateTime, TimeSpan offset)
        {
            return new DateTimeOffset(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second, dateTime.Millisecond, offset);
        }

        public static string[] DefaultDateTimeFormatStrings =
        {
            "yyyy-MM-dd HH:mm:ss",
            "dd-MMM-yyyy HH:mm:ss",
            "dd-MMM-yyyy hh:mm:ss tt",
            "dd-MMM-yyyy HH:mm:ss.ffffff",
            "dd-MMM-yyyy hh:mm:ss.ffffff tt",
            "dd-MMM-yyyy hh.mm.ss tt",
            "dd-MMM-yyyy HH.mm.ss",
            "dd-MMM-yyyy HH.mm.ss.ffffff",
            "dd-MMM-yy hh.mm.ss.ffffff tt",
            "dd-MMM-yy HH:mm:ss",
            "dd-MMM-yy hh:mm:ss tt",
            "dd-MMM-yy HH:mm:ss.ffffff",
            "dd-MMM-yy hh:mm:ss.ffffff tt",
            "dd-MMM-yy hh.mm.ss tt",
            "dd-MMM-yy HH.mm.ss",
            "dd-MMM-yy HH.mm.ss.ffffff",
            "dd-MMM-yy hh.mm.ss.ffffff tt"
        };

        public static DateTime ToDateTime(this string value, string[] formats)
        {
            foreach (var format in formats)
            {
                try
                {
                    var tryDtr = DateTime.ParseExact(value, format, CultureInfo.InvariantCulture);
                    return tryDtr;
                }
                catch
                {
                    // ignore
                }
            }
            throw new InvalidPatternException($"Unable to parse any of the input values [input:{value}] [formats:{string.Join(",", formats)}]");
        }

        public static DateTime ToDateTime(this string value, string format = null)
        {
            if (string.IsNullOrEmpty(format))
            {
                DateTime dtr;
                var tryDtr = DateTime.TryParse(value, out dtr); // Try default
                if (!tryDtr)
                {
                    dtr = ToDateTime(value, DefaultDateTimeFormatStrings);
                }
                return dtr;
            }
            //try
            //{
                var result = DateTime.ParseExact(value, format, CultureInfo.InvariantCulture);
                return result;
            /*}
            catch
            {
                return DateTime.MinValue;
            }*/
        }
    }
}