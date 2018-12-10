using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;

using UserControl = System.Windows.Controls.UserControl;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.WpfControls
{
	/// <summary>
	/// Interaction logic for LayerView.xaml
	/// Does the timecalculation on dragging
	/// This could be moved directly to the viewmodel with a LOT of effort
	/// interaction is "glued" to the viewmodel here instead
	/// </summary>
	/// <remarks>
	/// The changed events are handled here instead of in the viewmodel. 
	/// Could be done with some sort of delegate-events in the future (so the gui isnt dependent on this class, just the viewmodel)
	/// </remarks>
	public partial class LayerView : UserControl
	{
		private DateTimePeriod _initialPeriod;

		public static RoutedEvent LayerChangedEvent = EventManager.RegisterRoutedEvent(
			"LayerChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(LayerView));

		public event RoutedEventHandler LayerChanged
		{
			add { AddHandler(LayerChangedEvent, value); }
			remove { RemoveHandler(LayerChangedEvent, value); }
		}

		public static RoutedEvent PreviewLayerSelectedEvent = EventManager.RegisterRoutedEvent(
			"PreviewLayerSelected", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(LayerView));

		private double _originalWidth;


		public event RoutedEventHandler PreviewLayerSelected
		{
			add { AddHandler(PreviewLayerSelectedEvent, value); }
			remove { RemoveHandler(PreviewLayerSelectedEvent, value); }
		}

		public LayerView()
		{
			InitializeComponent();
			PreviewMouseDown += LayerView_PreviewMouseDown;
			PreviewMouseUp += LayerView_PreviewMouseUp;
		}

		void LayerView_PreviewMouseUp(object sender, MouseButtonEventArgs e)
		{
			Keyboard.Focus(hiddenButtonForFocusThatShouldBeHandledInViewModelInstead);
		}

		void LayerView_PreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			ILayerViewModel model = DataContext as LayerViewModel;
			if (model != null)
				RaiseEvent(new RoutedEventArgs(PreviewLayerSelectedEvent, model));
		}


		private void Thumb_Start_DragDelta(object sender, DragDeltaEventArgs e)
		{
			var item = VisualTreeFinder.FindVisualParent<ContentPresenter>(this);
			var panel = VisualTreeFinder.FindVisualParent<DateTimePeriodPanel>(this);
			ILayerViewModel model = DataContext as LayerViewModel;
			if (item != null && model != null)
				model.StartTimeChanged(panel, e.HorizontalChange);
		}

		#region DragDelta
		private void Thumb_Move_DragDelta(object sender, DragDeltaEventArgs e)
		{
			var item = VisualTreeFinder.FindVisualParent<ContentPresenter>(this);
			var panel = VisualTreeFinder.FindVisualParent<DateTimePeriodPanel>(this);
			var model = DataContext as LayerViewModel;

			var deltaWidth = _originalWidth - ActualWidth;
			if (item != null && model != null && panel != null)
				model.TimeChanged(panel, e.HorizontalChange + deltaWidth);
		}

		private void Thumb_End_DragDelta(object sender, DragDeltaEventArgs e)
		{
			var item = VisualTreeFinder.FindVisualParent<ContentPresenter>(this);
			var panel = VisualTreeFinder.FindVisualParent<DateTimePeriodPanel>(this);
			ILayerViewModel model = DataContext as LayerViewModel;
			if (item != null && model != null)
				model.EndTimeChanged(panel, e.HorizontalChange);
		}

		private void Thumb_Drag_Completed(object sender, DragCompletedEventArgs e)
		{
			ILayerViewModel model = DataContext as LayerViewModel;
			if (model != null)
			{
				if (model.Period.StartDateTime != _initialPeriod.StartDateTime ||
				    model.Period.EndDateTime != _initialPeriod.EndDateTime)
				{
					model.UpdatePeriod();
					RaiseEvent(new RoutedEventArgs(LayerChangedEvent));
				}
			}
		}
		#endregion

		private void Thumb_Drag_Started(object sender, DragStartedEventArgs e)
		{
			ILayerViewModel model = DataContext as LayerViewModel;
			if (model != null) _initialPeriod = model.Period;
			_originalWidth = ActualWidth;
		}
	}
}
