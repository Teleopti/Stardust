﻿using System;

namespace Teleopti.Ccc.WinCode.Converters.DateTimeConverter
{
    public class DateTimeToLocalConverter:DateTimeBaseConverter
    {
        public override object Transform(DateTime convertedDateTime, object parameter)
        {
            return convertedDateTime;
        }

        public override object TransformBack(DateTime convertedDateTime, object parameter)
        {
            return convertedDateTime;
        }
    }

    
}
