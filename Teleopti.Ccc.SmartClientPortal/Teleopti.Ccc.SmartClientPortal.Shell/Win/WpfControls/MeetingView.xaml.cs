using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Teleopti.Ccc.WinCode.Common;

namespace Teleopti.Ccc.Win.WpfControls
{
    /// <summary>
    /// Interaction logic for MeetingView.xaml
    /// </summary>
    public partial class MeetingView : UserControl
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible")]
        public static RoutedEvent PreviewLayerSelectedEvent = EventManager.RegisterRoutedEvent(
            "PreviewLayerSelected", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MeetingView));

        public event RoutedEventHandler PreviewLayerSelected
        {
            add { AddHandler(PreviewLayerSelectedEvent, value); }
            remove { RemoveHandler(PreviewLayerSelectedEvent, value); }
        }

        public MeetingView()
        {
            InitializeComponent();
        }

        private void EditMeeting_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ILayerViewModel model = DataContext as LayerViewModel;
            if (model != null)
            {
                RaiseEvent(new RoutedEventArgs(PreviewLayerSelectedEvent, model));
            }
        }

        private void EditMeeting_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            MeetingLayerViewModel model = DataContext as MeetingLayerViewModel;
            e.CanExecute = model!=null;
        }

        //Change to use command instead
        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount==2)
            {
                ILayerViewModel model = DataContext as LayerViewModel;
                if (model != null)
                {
                    RaiseEvent(new RoutedEventArgs(PreviewLayerSelectedEvent, model));
                }
            }
        }
    }
}
