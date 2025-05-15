using InternetCafe.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InternetCafe.Common.Services
{
    public class TimeZoneService : ITimeZoneService
    {
        private readonly TimeZoneInfo _vietnamTimeZone;
        public const string VIETNAM_TIMEZONE_ID = "SE Asia Standard Time";

        public TimeZoneService()
        {
            _vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById(VIETNAM_TIMEZONE_ID);
        }

        public DateTime GetVietnamTime()
        {
            return TimeZoneInfo.ConvertTime(DateTime.Now, _vietnamTimeZone);
        }

        public DateTime GetUtcTime()
        {
            return DateTime.UtcNow;
        }

        public DateTime ConvertToVietnamTime(DateTime utcTime)
        {
            return TimeZoneInfo.ConvertTime(utcTime, _vietnamTimeZone);
        }

        public DateTime ConvertToUtc(DateTime vietnamTime)
        {
            return TimeZoneInfo.ConvertTimeToUtc(vietnamTime, _vietnamTimeZone);
        }
    }
}
