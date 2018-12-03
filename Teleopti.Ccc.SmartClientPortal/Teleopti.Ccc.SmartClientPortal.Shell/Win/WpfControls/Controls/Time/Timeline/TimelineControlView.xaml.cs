using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Time.Timeline;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.WpfControls.Controls.Time.Timeline
{
    /// <summary>
    /// Interaction logic for TimeControlView.xaml
    /// </summary>
    public partial class TimelineControlView : UserControl
    {
        public TimeSpan ElapsedTime
        {
            get { return (TimeSpan) GetValue(ElapsedTimeProperty); }
            set { SetValue(ElapsedTimeProperty, value); }
        }

        public static readonly DependencyProperty ElapsedTimeProperty =
            DependencyProperty.Register(
                "ElapsedTime",
                typeof (TimeSpan),
                typeof (TimelineControlView));

        private DateTime _mouseDownTime = DateHelper.MinSmallDateTime.ToUniversalTime();

        public TimelineControlView()
        {
            InitializeComponent();
            PreviewMouseUp += TimelineControlView_PreviewMouseUp;
        }

        private void TimelineControlView_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            Keyboard.Focus(hiddenButtonForFocusThatShouldBeHandledInViewModelInstead);
            TimeLineContextMenu.PlacementTarget = this;
            TimeLineContextMenu.IsOpen = true;
        }

        private void ContextMenu_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ((ContextMenu) sender).PlacementTarget.Focus();
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
                    model.HoverTime = panel.GetUtcDateTimeFromPosition(e.GetPosition(panel).X).ToInterval(model.Interval);


                    if (e.RightButton == MouseButtonState.Pressed || e.LeftButton == MouseButtonState.Pressed)
                    {

                        if (model.HoverTime <= _mouseDownTime)
                        {
                            model.SelectedPeriod = new DateTimePeriod(model.HoverTime, _mouseDownTime);
                            ElapsedTime = model.SelectedPeriod.StartDateTime.Subtract(model.SelectedPeriod.EndDateTime).Negate();
                        }
                        else
                        {
                            model.SelectedPeriod = new DateTimePeriod(_mouseDownTime, model.HoverTime);
                            ElapsedTime = model.SelectedPeriod.EndDateTime.Subtract(model.SelectedPeriod.StartDateTime);
                        }
                        model.ShowSelectedPeriod = true;
                        
                    }

                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private void DateTimePeriodPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var panel = sender as DateTimePeriodPanel;
            
            if (panel != null && (e.RightButton == MouseButtonState.Pressed || e.LeftButton == MouseButtonState.Pressed))
            {
                TimelineControlViewModel model = GetModel();
                
                if (model != null)
                {
                    _mouseDownTime = panel.GetUtcDateTimeFromPosition(e.GetPosition(panel).X).ToInterval(model.Interval);

                    model.SelectedPeriod = new DateTimePeriod(_mouseDownTime, _mouseDownTime.AddMilliseconds(1));
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
                    DateTime mouseUpTime = panel.GetUtcDateTimeFromPosition(e.GetPosition(panel).X).ToInterval(model.Interval);
                    if (mouseUpTime > model.SelectedPeriod.StartDateTime && (e.RightButton==MouseButtonState.Pressed || e.LeftButton == MouseButtonState.Pressed))
                    {
                        model.SelectedPeriod = new DateTimePeriod(model.SelectedPeriod.StartDateTime, mouseUpTime.ToInterval(model.Interval));
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
