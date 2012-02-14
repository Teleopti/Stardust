using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Microsoft.Windows.Controls;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Converters;
using Teleopti.Ccc.WpfControls.Controls.Intraday.Models;
using Teleopti.Ccc.WpfControls.Controls.Layers;
using Teleopti.Ccc.WpfControls.Controls.Time.Timeline;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WpfControls.Controls.Intraday.Views
{
    /// <summary>
    /// Interaction logic for IntradayLayerView.xaml
    /// </summary>
    /// <remarks>
    /// Henrik 2009-07-20: Move as much logic to the viewmodel as possible
    /// </remarks>
    public partial class RealTimeScheduleView : UserControl
    {
        public RealTimeScheduleView()
        {
            InitializeComponent();
            DataContextChanged += (RealTimeScheduleView_DataContextChanged);
           
        }

        private void RealTimeScheduleView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            //Set the binding in code since its hard to find the element generated in XAML
            Binding zoomBinding = new Binding("PanelWidth");
            zoomBinding.Source = Model.TimelineModel.TimeZoom;
            SetBinding(ZoomWidthProperty, zoomBinding);
            BindMultipleHeaders();

        }

        public void BindMultipleHeaders()
        {
            bindAgentColumn();
            bindActivityColumn();
            bindAlarmColumn();
        }

        private void bindAlarmColumn()
        {
            MultiBinding alarmBinding = new MultiBinding();
            alarmBinding.Converter = new ColumnWidthConverter();
            Binding binding = new Binding("ActualWidth");
            binding.Source = realTimeDataGrid.Columns[6];
            Binding binding1 = new Binding("ActualWidth");
            binding1.Source = realTimeDataGrid.Columns[7];
            Binding binding2 = new Binding("ActualWidth");
            binding2.Source = realTimeDataGrid.Columns[8];

            alarmBinding.Bindings.Add(binding);
            alarmBinding.Bindings.Add(binding1);
            alarmBinding.Bindings.Add(binding2);

            cc.SetBinding(WidthProperty, alarmBinding);
        }

        private void bindActivityColumn()
        {
            MultiBinding activityBinding = new MultiBinding();
            activityBinding.Converter = new ColumnWidthConverter();
            Binding binding = new Binding("ActualWidth");
            binding.Source = realTimeDataGrid.Columns[3];
            Binding binding1 = new Binding("ActualWidth");
            binding1.Source = realTimeDataGrid.Columns[4];
            Binding binding2 = new Binding("ActualWidth");
            binding2.Source = realTimeDataGrid.Columns[5];

            activityBinding.Bindings.Add(binding);
            activityBinding.Bindings.Add(binding1);
            activityBinding.Bindings.Add(binding2);

            bb.SetBinding(WidthProperty, activityBinding);
        }

        private void bindAgentColumn()
        {
            MultiBinding agentBinding = new MultiBinding();
            agentBinding.Converter = new ColumnWidthConverter();
            Binding binding = new Binding("ActualWidth");
            binding.Source = realTimeDataGrid.Columns[0];
            Binding binding1 = new Binding("ActualWidth");
            binding1.Source = realTimeDataGrid.Columns[1];
            Binding binding2 = new Binding("ActualWidth");
            binding2.Source = realTimeDataGrid.Columns[2];

            agentBinding.Bindings.Add(binding);
            agentBinding.Bindings.Add(binding1);
            agentBinding.Bindings.Add(binding2);
            
            aa.SetBinding(WidthProperty, agentBinding);
        }

        #region remove
        //Need to do this to get to the generated elements. Should be refactored
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private void TimelineControl_Loaded(object sender, RoutedEventArgs e)
        {
           TimelineControl control = (TimelineControl) sender;
            if (Model != null)
            {
                Model.TimelineModel.ShowDate = true;
                Model.TimelineModel.ShowNowPeriod = true;
                Model.TimelineModel.ShowHoverTime = false;
                control.DataContext = Model.TimelineModel;
                ScrollToUtcNow();
            }
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private void LayerListBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (Model!=null)
            {
                LayerListBox listBox = (LayerListBox) sender;
                Binding nameTextBinding = new Binding("Period");
                nameTextBinding.Source = Model;
                listBox.SetBinding(LayerListBox.PeriodProperty, nameTextBinding);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private RealTimeScheduleViewModel Model
        {
            get { return DataContext as RealTimeScheduleViewModel; }
        }

        private DataGridColumn ScheduleColumn
        {
            get
            {
                return realTimeDataGrid != null ? realTimeDataGrid.Columns.Last() : null;
            }
        }

        public double ZoomWidth
        {
            get { return (double) GetValue(ZoomWidthProperty); }
            set { SetValue(ZoomWidthProperty, value); }
        }

        public static readonly DependencyProperty ZoomWidthProperty =
            DependencyProperty.Register("ZoomWidth", typeof (double), typeof (RealTimeScheduleView), new UIPropertyMetadata(2000d,ZoomWidthChanged));

        private static void ZoomWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RealTimeScheduleView view = (RealTimeScheduleView) d;
            if(view.ScheduleColumn!=null)
            {
                view.ScheduleColumn.Width = (double) e.NewValue;
            }
        }

       
        /// <summary>
        /// Finds the scrollviewer and if the timeline is within UtCNow, it scrolls to that position
        /// </summary>
        /// <remarks>
        /// Must be called on GUI-thread
        /// Henrika: TODO:This should be handled by the ZoomeViewModel instead
        /// </remarks>
        private void ScrollToUtcNow()
        {
            VisualTreeFinder treeFinder = new VisualTreeFinder();
            ScrollViewer scrollViewer = treeFinder.GetDescendantByType(this, typeof(ScrollViewer)) as ScrollViewer;

            if (scrollViewer != null && Model.TimelineModel.Period.Contains(DateTime.UtcNow))
            {
                TimeSpan startToNow =
                    new DateTimePeriod(Model.TimelineModel.Period.StartDateTime, DateTime.UtcNow).ElapsedTime();

                var d = (startToNow.TotalSeconds / Model.TimelineModel.Period.ElapsedTime().TotalSeconds) *
                        scrollViewer.ScrollableWidth;
                scrollViewer.ScrollToHorizontalOffset(d);
            }
        }


        #endregion
    }
}