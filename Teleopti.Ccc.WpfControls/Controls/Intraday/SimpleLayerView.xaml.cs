using System.Windows;
using UserControl=System.Windows.Controls.UserControl;

namespace Teleopti.Ccc.WpfControls.Controls.Intraday
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


        private void Border_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(SelectShiftEvent, this));
        }
    }
}