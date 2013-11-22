using System;
using System.Collections.Generic;
using System.Globalization;
using Teleopti.Analytics.Etl.Transformer;

namespace Teleopti.Analytics.Etl.TransformerTest.FakeData
{
    public sealed class DateFactory
    {
        #region Constructors (1) 

        private DateFactory()
        {
        }

        #endregion Constructors 

        #region Methods (2) 

        // Public Methods (2) 

        public static IList<DayDate> CreateDateCollection(CultureInfo culture)
        {
            IList<DayDate> retList = new List<DayDate>();

            DayDate date = new DayDate(new DateTime(2007, 1, 1, 0, 0, 0, DateTimeKind.Utc), culture);
            retList.Add(date);
            date = new DayDate(new DateTime(2007, 5, 31, 0, 0, 0, DateTimeKind.Utc), culture);
            retList.Add(date);
            date = new DayDate(new DateTime(2006, 12, 31, 0, 0, 0, DateTimeKind.Utc), culture);
            retList.Add(date);
            date = new DayDate(new DateTime(2006, 12, 30, 0, 0, 0, DateTimeKind.Utc), culture);
            retList.Add(date);
            date = new DayDate(new DateTime(2006, 12, 29, 0, 0, 0, DateTimeKind.Utc), culture);
            retList.Add(date);
            date = new DayDate(new DateTime(2006, 8, 31, 0, 0, 0, DateTimeKind.Utc), culture);
            retList.Add(date);

            return retList;
        }

        //public static IList<DayDate> CreateDateCollection2(CultureInfo culture)
        //{
        //    IList<DayDate> retList = new List<DayDate>();

        //    DayDate date = new DayDate(new DateTime(2007, 1, 1, 0, 0, 0, DateTimeKind.Utc), culture);
        //    retList.Add(date);
        //    date = new DayDate(new DateTime(2007, 1, 2, 0, 0, 0, DateTimeKind.Utc), culture);
        //    retList.Add(date);
        //    date = new DayDate(new DateTime(2007, 1, 3, 0, 0, 0, DateTimeKind.Utc), culture);
        //    retList.Add(date);
        //    date = new DayDate(new DateTime(2007, 1, 4, 0, 0, 0, DateTimeKind.Utc), culture);
        //    retList.Add(date);
        //    date = new DayDate(new DateTime(2007, 1, 5, 0, 0, 0, DateTimeKind.Utc), culture);
        //    retList.Add(date);

        //    return retList;
        //}

        #endregion Methods 
    }
}