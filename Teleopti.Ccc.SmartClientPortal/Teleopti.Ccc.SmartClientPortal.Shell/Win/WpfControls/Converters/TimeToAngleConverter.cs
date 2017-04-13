using System;
using System.Windows.Data;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.WpfControls.Converters
{
    
        [ValueConversion(typeof(DateTime), typeof(double))]
        public class TimeToAngleConverter : IValueConverter
        {
            public object Convert(object value, Type targetType,
                object parameter, System.Globalization.CultureInfo culture)
            {
                var hand = (HandType)Enum.Parse(typeof(HandType), parameter as string);
                if (value is DateTime)
                {
                    var timeValue = (DateTime)value;
                    if (hand == HandType.HourHand)
                        return timeValue.Hour*30 + timeValue.Minute*0.5;
                    
                    if (hand == HandType.MinuteHand)
                        return timeValue.Minute*6;
                }
                return null;
            }

            public object ConvertBack(object value, Type targetType,
                object parameter, System.Globalization.CultureInfo culture)
            {

                return null;
            }

            private enum HandType
            {
                HourHand,
                MinuteHand
            }
        } 
    }

