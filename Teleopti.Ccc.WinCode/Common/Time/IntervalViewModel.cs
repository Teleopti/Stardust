using System;
using System.ComponentModel;
using System.Windows;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Common.Time
{
    public class IntervalViewModel : DependencyObject, INotifyPropertyChanged
    {
        #region Fields
        private static DateTimePeriod _defaultPeriod = new DateTimePeriod();
        private DateTime _label = _defaultPeriod.StartDateTime;

        public static readonly DependencyProperty PeriodProperty =
           DependencyProperty.Register("Period", typeof(DateTimePeriod),
           typeof(IntervalViewModel),
           new UIPropertyMetadata(_defaultPeriod, PeriodChanged));

        #endregion

        #region Properties
        public DateTime Label
        {
            get { return _label; }
            private set
            {
                if (value != _label)
                    _label = value;
                SendPropertyChanged("Label");
            }
        }

        public DateTimePeriod Period
        {
            get { return (DateTimePeriod)GetValue(PeriodProperty); }
            set { SetValue(PeriodProperty, value); }
        }

        #endregion


        #region private
        private static void PeriodChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            IntervalViewModel model = (IntervalViewModel)d;
            DateTimePeriod newValue = (DateTimePeriod)e.NewValue;
            model.Label = newValue.StartDateTime;
        }

        private void SendPropertyChanged(string property)
        {
        	var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(property));
        }
        #endregion

        #region public
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

    }
}
