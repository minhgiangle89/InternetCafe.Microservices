using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InternetCafe.Common.Interfaces
{
    public interface ITimeZoneService
    {
        DateTime GetVietnamTime();
        DateTime GetUtcTime();
        DateTime ConvertToVietnamTime(DateTime utcTime);
        DateTime ConvertToUtc(DateTime vietnamTime);
    }
}
