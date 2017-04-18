using System;
using System.Windows;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers
{

    /// <summary>
    /// TimeZoneInfo that inherits down the visualtree
    /// </summary>
    /// <remarks>
    /// Usage: StackPanel VisualTreeTimeZone.TimeZoneInfo="{Binding Path=TimeZone}" 
    /// Default is TimeZoneInfo.Local
    /// Created by: henrika
    /// Created date: 2009-04-01
    /// </remarks>
    public static class VisualTreeTimeZoneInfo
    {
        public static TimeZoneInfo GetTimeZoneInfo(DependencyObject obj)
        {
            return (TimeZoneInfo)obj.GetValue(TimeZoneInfoProperty);
        }
        public static void SetTimeZoneInfo(DependencyObject obj, TimeZoneInfo value)
        {
            obj.SetValue(TimeZoneInfoProperty, value);
        }
        public static readonly DependencyProperty TimeZoneInfoProperty =
        DependencyProperty.RegisterAttached("TimeZoneInfo",
        typeof(TimeZoneInfo),
        typeof(VisualTreeTimeZoneInfo),
        new FrameworkPropertyMetadata(TimeZoneInfo.Local, FrameworkPropertyMetadataOptions.Inherits,TimeZoneInfoChanged));

        /// <summary>
        /// Times the zone info changed.
        /// </summary>
        /// <remarks>
        /// Note: Might not be the best solution, dont know how to this without casting...
        /// If the datacontext implements the interface IVisualTimeZoneInfoMonitor, it will call its TimeZoneInfoChanged-method
        /// </remarks>
        private static void TimeZoneInfoChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TimeZoneInfo oldTimeZoneInfo = (TimeZoneInfo)e.OldValue;
            TimeZoneInfo newTimeZoneInfo = (TimeZoneInfo)e.NewValue;
            if (!oldTimeZoneInfo.Equals(newTimeZoneInfo))
            {

                FrameworkElement container = d as FrameworkElement;
                if (container != null)
                {
                    var monitor = container.DataContext as IVisualTimeZoneInfoMonitor;
                    if (monitor != null)
                    {
                        monitor.TimeZoneInfoChanged(newTimeZoneInfo);
                    }
                }
            }
        }
    }
}


