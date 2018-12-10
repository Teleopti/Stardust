using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.WpfControls.Controls.Intraday.Models;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.WpfControls.Controls.Layers;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.WpfControls.Controls.Time.Timeline;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Converters;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Intraday;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.WpfControls.Controls.Intraday.Views
{
	/// <summary>
	/// Interaction logic for IntradayLayerView.xaml
	/// </summary>
	/// <remarks>
	/// Henrik 2009-07-20: Move as much logic to the viewmodel as possible
	/// </remarks>
	public partial class RealTimeScheduleView
	{
		public RealTimeScheduleView()
		{
			InitializeComponent();
			DataContextChanged += (RealTimeScheduleView_DataContextChanged);
		}

		static RealTimeScheduleView()
		{
			Timeline.DesiredFrameRateProperty.OverrideMetadata(typeof(Timeline),
															   new FrameworkPropertyMetadata { DefaultValue = 30 });
		}

		private void RealTimeScheduleView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			//Set the binding in code since its hard to find the element generated in XAML
			var zoomBinding = new Binding("PanelWidth") { Source = Model.TimelineModel.TimeZoom };
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
			var alarmBinding = new MultiBinding { Converter = new ColumnWidthConverter() };
			var binding = new Binding("ActualWidth") { Source = RealTimeDataGrid.Columns[6] };
			var binding1 = new Binding("ActualWidth") { Source = RealTimeDataGrid.Columns[7] };
			var binding2 = new Binding("ActualWidth") { Source = RealTimeDataGrid.Columns[8] };

			alarmBinding.Bindings.Add(binding);
			alarmBinding.Bindings.Add(binding1);
			alarmBinding.Bindings.Add(binding2);

			Cc.SetBinding(WidthProperty, alarmBinding);
		}

		private void bindActivityColumn()
		{
			var activityBinding = new MultiBinding { Converter = new ColumnWidthConverter() };
			var binding = new Binding("ActualWidth") { Source = RealTimeDataGrid.Columns[3] };
			var binding1 = new Binding("ActualWidth") { Source = RealTimeDataGrid.Columns[4] };
			var binding2 = new Binding("ActualWidth") { Source = RealTimeDataGrid.Columns[5] };

			activityBinding.Bindings.Add(binding);
			activityBinding.Bindings.Add(binding1);
			activityBinding.Bindings.Add(binding2);

			Bb.SetBinding(WidthProperty, activityBinding);
		}

		private void bindAgentColumn()
		{
			var agentBinding = new MultiBinding { Converter = new ColumnWidthConverter() };
			var binding = new Binding("ActualWidth") { Source = RealTimeDataGrid.Columns[0] };
			var binding1 = new Binding("ActualWidth") { Source = RealTimeDataGrid.Columns[1] };
			var binding2 = new Binding("ActualWidth") { Source = RealTimeDataGrid.Columns[2] };

			agentBinding.Bindings.Add(binding);
			agentBinding.Bindings.Add(binding1);
			agentBinding.Bindings.Add(binding2);

			Aa.SetBinding(WidthProperty, agentBinding);
		}

		#region remove

		//Need to do this to get to the generated elements. Should be refactored
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		private void TimelineControl_Loaded(object sender, RoutedEventArgs e)
		{
			var control = (TimelineControl)sender;
			if (Model == null) return;
			Model.TimelineModel.ShowDate = true;
			Model.TimelineModel.ShowNowPeriod = true;
			Model.TimelineModel.ShowHoverTime = false;
			Model.TimelineModel.NowTimeOpacity = 0d;
			control.DataContext = Model.TimelineModel;
			ScrollToUtcNow();
		}


		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		private void LayerListBox_Loaded(object sender, RoutedEventArgs e)
		{
			if (Model == null) return;
			var listBox = (LayerListBox)sender;
			var nameTextBinding = new Binding("Period") { Source = Model };
			listBox.SetBinding(LayerListBox.PeriodProperty, nameTextBinding);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		private RealTimeScheduleViewModel Model
		{
			get { return DataContext as RealTimeScheduleViewModel; }
		}

		private DataGridColumn ScheduleColumn
		{
			get { return RealTimeDataGrid != null ? RealTimeDataGrid.Columns.Last() : null; }
		}

		public double ZoomWidth
		{
			get { return (double)GetValue(ZoomWidthProperty); }
			set { SetValue(ZoomWidthProperty, value); }
		}

		public static readonly DependencyProperty ZoomWidthProperty =
			DependencyProperty.Register("ZoomWidth", typeof(double), typeof(RealTimeScheduleView),
										new UIPropertyMetadata(2000d, ZoomWidthChanged));

		private static void ZoomWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var view = (RealTimeScheduleView)d;
			if (view.ScheduleColumn != null)
				view.ScheduleColumn.Width = (double)e.NewValue;
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
			var treeFinder = new VisualTreeFinder();
			var scrollViewer = treeFinder.GetDescendantByType(this, typeof(ScrollViewer)) as ScrollViewer;

			if (scrollViewer == null || !Model.TimelineModel.Period.Contains(DateTime.UtcNow)) return;
			var startToNow =
				new DateTimePeriod(Model.TimelineModel.Period.StartDateTime, DateTime.UtcNow).ElapsedTime();

			var d = (startToNow.TotalSeconds / Model.TimelineModel.Period.ElapsedTime().TotalSeconds) *
					scrollViewer.ScrollableWidth;
			scrollViewer.ScrollToHorizontalOffset(d);
		}

		#endregion

		private void CheckBox_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			var checkbox = sender as CheckBox;
			if (checkbox != null)
				checkbox.IsChecked = !checkbox.IsChecked;
			e.Handled = true;
		}

		private void ContextMenu_OnClick(object sender, RoutedEventArgs e)
		{
			var selectedRow = RealTimeDataGrid.SelectedIndex;
			RealTimeDataGrid.SelectionMode = DataGridSelectionMode.Extended;
			RealTimeDataGrid.SelectAllCells();
			RealTimeDataGrid.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
			ApplicationCommands.Copy.Execute(null, RealTimeDataGrid);
			RealTimeDataGrid.UnselectAllCells();
			RealTimeDataGrid.SelectionMode = DataGridSelectionMode.Single;
			RealTimeDataGrid.SelectedIndex = selectedRow;
		}

		private void layer_Clicked(object sender, MouseButtonEventArgs e)
		{
			var uiElement = sender as UIElement;
			if (uiElement == null) return;

			var clickedRow = VisualTreeFinder.FindVisualParent<DataGridRow>(uiElement);
			if (clickedRow != null)
				RealTimeDataGrid.SelectedIndex = clickedRow.GetIndex();
		}

		private void NextActivityStartDateTimeColumn_OnCopyingCellClipboardContent(object sender, DataGridCellClipboardEventArgs e)
		{
			var utcTime = ((DayLayerModel) e.Item).NextActivityStartDateTime;

			if (utcTime < DateHelper.MinSmallDateTime)
				e.Content = "";
			else
				e.Content =
					TimeZoneHelper.ConvertFromUtc(utcTime, TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone)
								.ToString(TeleoptiPrincipal.CurrentPrincipal.Regional.Culture);
		}

		private void PinnedColumn_OnCopyingCellClipboardContent(object sender, DataGridCellClipboardEventArgs e)
		{
			if (((DayLayerModel) e.Item).IsPinned)
				e.Content = 1;
			else
				e.Content = "";
		}
	}

}