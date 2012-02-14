using System;
using System.Windows.Controls;
using System.Windows.Input;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Common.Time.Timeline;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WpfControls.Controls.Time.Timeline
{
    /// <summary>
    /// Interaction logic for TimeControlView.xaml
    /// </summary>
    public partial class TimelineControlView : UserControl
    {
        private DateTime _mouseDownTime = DateHelper.MinSmallDateTime.ToUniversalTime();

        public TimelineControlView()
        {
            InitializeComponent();
            PreviewMouseUp += TimelineControlView_PreviewMouseUp;
        }

        private void TimelineControlView_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            Keyboard.Focus(hiddenButtonForFocusThatShouldBeHandledInViewModelInstead);
        }

        #region MoveToModelIfPossible
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private void DateTimePeriodPanel_MouseEnter(object sender, MouseEventArgs e)
        {
            TimelineControlViewModel model = GetModel();
            if (model != null)
            {
                model.ShowHoverTime = true;
            }
            e.Handled = true;

        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private void DateTimePeriodPanel_MouseLeave(object sender, MouseEventArgs e)
        {
            TimelineControlViewModel model = GetModel();
            if (model != null)
            {
                model.ShowSelectedPeriod = false;
                model.ShowHoverTime = false;
            }
            e.Handled = true;
        }



        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private void DateTimePeriodPanel_MouseMove(object sender, MouseEventArgs e)
        {
            var panel = sender as DateTimePeriodPanel;
           
            if (panel != null)
            {
                TimelineControlViewModel model = GetModel();
                if (model != null)
                {
                    model.HoverTime = panel.GetUtcDateTimeFromPosition(e.GetPosition(panel).X);

                    if (e.RightButton == MouseButtonState.Pressed)
                    {
                        
                        if (model.HoverTime <= _mouseDownTime)
                        {
                            model.SelectedPeriod = new DateTimePeriod(model.HoverTime, _mouseDownTime);     
                        }
                        else
                            model.SelectedPeriod = new DateTimePeriod(_mouseDownTime,model.HoverTime);
                        model.ShowSelectedPeriod = true;
                    }

                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private void DateTimePeriodPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var panel = sender as DateTimePeriodPanel;
           
            if (panel != null && e.RightButton == MouseButtonState.Pressed)
            {
                TimelineControlViewModel model = GetModel();
                if (model != null)
                {
                    _mouseDownTime = panel.GetUtcDateTimeFromPosition(e.GetPosition(panel).X);
                    model.SelectedPeriod = new DateTimePeriod(_mouseDownTime, _mouseDownTime.AddMinutes(1));

                }
              
            }
            e.Handled = true;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private void DateTimePeriodPanel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var panel = sender as DateTimePeriodPanel;
            if (panel != null)
            {
                TimelineControlViewModel model = GetModel();
                if (model != null)
                {
                    model.ShowSelectedPeriod = false;
                    DateTime mouseUpTime = panel.GetUtcDateTimeFromPosition(e.GetPosition(panel).X);
                    if (mouseUpTime > model.SelectedPeriod.StartDateTime && e.RightButton==MouseButtonState.Pressed)
                    {
                        model.SelectedPeriod = new DateTimePeriod(model.SelectedPeriod.StartDateTime, mouseUpTime);
                    }
                }
            }

        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private TimelineControlViewModel GetModel()
        {
            return DataContext as TimelineControlViewModel;
        }
        #endregion //tempMoveToModelIfPossible

    }
}
