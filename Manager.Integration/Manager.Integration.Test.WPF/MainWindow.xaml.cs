using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Manager.Integration.Test.WPF
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
    }
}