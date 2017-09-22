using System;

namespace Teleopti.Support.LicTool
{
    static class LicenseExpirationDate
    {
        public static DateTime GetLicenseExpirationDate(DateTime fromdate, int days)
        {
            
            DateTime expiration = fromdate.AddDays(days);
            

            // We want Thu
            int DoW = (int)expiration.DayOfWeek;

            if (DoW == 0) // Sun
            {
                expiration = expiration.AddDays(-3);
            }
            else if (DoW == 6) // Sat
            {
                expiration = expiration.AddDays(-2);
            }
            else if (DoW == 1) // Mon 
            {
                expiration = expiration.AddDays(3);
            }
            else if (DoW == 2) // Tue
            {
                expiration = expiration.AddDays(2);
            }
            else if (DoW == 3) // Wed
            {
                expiration = expiration.AddDays(1);
            }
            else if (DoW == 5) // Fri
            {
                expiration = expiration.AddDays(-1);


            }
            // add until noon
            expiration = expiration.AddHours(12);

            return expiration;



        }
    }
}