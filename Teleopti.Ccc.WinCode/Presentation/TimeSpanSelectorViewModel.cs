using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Presentation
{
    /// <summary>
    /// ViewModel for TimeSpanSetter
    /// Min/Max TimeSpan The minimum/maximum timespan
    /// TimeSpan The selected value
    /// Snap Snap to intervals on/off
    /// </summary>
    /// <remarks>
    /// Created by: henrika
    /// Created date: 2008-08-14
    /// </remarks>
    public class TimeSpanSelectorViewModel : DependencyObject
    {
        //Default values
        private static readonly TimeSpan _defaultTimeSpan = TimeSpan.FromMinutes(15);
        private const double _defaultMin = 1;
        private const double _defaultMax = 60;
    
    
        #region DependencyProperties

        public TimeSpan TimeSpan
        {
            get { return (TimeSpan)GetValue(TimeSpanProperty); }
            set { SetValue(TimeSpanProperty, value); }
        }

        public MinMax<TimeSpan> MinMax
        {
            get { return (MinMax<TimeSpan>)GetValue(MinMaxProperty); }
            set { SetValue(MinMaxProperty, value); }
        }

        public bool Snap
        {
            get { return (bool) GetValue(SnapProperty); }
            set { SetValue(SnapProperty, value); }
        }

        public static readonly DependencyProperty MinMaxProperty =
            DependencyProperty.Register("MinMax", typeof (MinMax<TimeSpan>), typeof (TimeSpanSelectorViewModel),
                                        new UIPropertyMetadata(new MinMax<TimeSpan>(TimeSpan.FromMinutes(_defaultMin),TimeSpan.FromMinutes(_defaultMax)),
                                        OnMixMaxValueChange));

        public static readonly DependencyProperty SnapProperty =
            DependencyProperty.Register("Snap", 
            typeof (bool), 
            typeof (TimeSpanSelectorViewModel),
            new UIPropertyMetadata(true));

        public static readonly DependencyProperty TimeSpanProperty =
            DependencyProperty.Register("TimeSpan", typeof(TimeSpan), 
            typeof(TimeSpanSelectorViewModel), 
            new UIPropertyMetadata(_defaultTimeSpan, OnTimeSpanValueChanged, CoerceTimeSpan));

        private static void OnTimeSpanValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //Notify PropertyChanged if necesseary
        }

        private static object CoerceTimeSpan(DependencyObject d, object baseValue)
        {
            TimeSpanSelectorViewModel model = (TimeSpanSelectorViewModel)d;
            TimeSpan newValue = (TimeSpan)baseValue;
            if (newValue < model.MinMax.Minimum) return model.MinMax.Minimum;
            if (newValue > model.MinMax.Maximum) return model.MinMax.Maximum;
            return newValue;
        }

        private static void OnMixMaxValueChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.CoerceValue(TimeSpanProperty);
        }

        #endregion //DependencyProperties

       
    }
}
