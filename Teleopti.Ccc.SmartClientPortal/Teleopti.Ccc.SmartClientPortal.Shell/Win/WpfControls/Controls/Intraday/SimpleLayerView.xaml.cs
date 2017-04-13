using System.Windows;
using System.Windows.Input;

namespace Teleopti.Ccc.Win.WpfControls.Controls.Intraday
{
    public partial class SimpleLayerView
    {
        public static readonly RoutedEvent SelectShiftEvent =
            EventManager.RegisterRoutedEvent("SelectShift",
                                             RoutingStrategy.Bubble,
                                             typeof(RoutedEventHandler),
                                             typeof(SimpleLayerView));

        public event RoutedEventHandler SelectShift
        {
            add { AddHandler(SelectShiftEvent, value); }
            remove { RemoveHandler(SelectShiftEvent, value); }
        }

        public SimpleLayerView()
        {
            InitializeComponent();
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(SelectShiftEvent, this));
        }
    }
}