using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Manager.IntegrationTest.WPF.ViewModels;

namespace Manager.IntegrationTest.WPF
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        void RestoreScalingFactor(object sender,
                                  MouseButtonEventArgs args)

        {
            ((Slider) sender).Value = 1.0;
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs args)

        {
            base.OnPreviewMouseDown(args);

            if (Keyboard.IsKeyDown(Key.LeftCtrl) ||
                Keyboard.IsKeyDown(Key.RightCtrl))

            {
                if (args.MiddleButton == MouseButtonState.Pressed)

                {
                    RestoreScalingFactor(UiScaleSlider,
                                         args);
                }
            }
        }

        protected override void OnPreviewMouseWheel(MouseWheelEventArgs args)

        {
            base.OnPreviewMouseWheel(args);

            if (Keyboard.IsKeyDown(Key.LeftCtrl) ||
                Keyboard.IsKeyDown(Key.RightCtrl))

            {
                UiScaleSlider.Value += (args.Delta > 0) ? 0.1 : -0.1;
            }
        }

        private void Expander_OnExpanded(object sender,
                                         RoutedEventArgs e)
        {
            for (var vis = sender as Visual; vis != null; vis = VisualTreeHelper.GetParent(vis) as Visual)
            {
                if (vis is DataGridRow)
                {
                    var row = (DataGridRow) vis;
                    row.DetailsVisibility = row.DetailsVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
                    break;
                }
            }
        }

        private void Expander_OnCollapsed(object sender,
                                          RoutedEventArgs e)
        {
            for (var vis = sender as Visual; vis != null; vis = VisualTreeHelper.GetParent(vis) as Visual)
            {
                if (vis is DataGridRow)
                {
                    var row = (DataGridRow) vis;
                    row.DetailsVisibility = row.DetailsVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
                    break;
                }
            }
        }

		private void Window_Closed(object sender, System.EventArgs e)
		{
			var mainWindowViewModel = this.DataContext as MainWindowViewModel;

			if (mainWindowViewModel != null)
			{
				mainWindowViewModel.Dispose();
			}
		}
    }
}