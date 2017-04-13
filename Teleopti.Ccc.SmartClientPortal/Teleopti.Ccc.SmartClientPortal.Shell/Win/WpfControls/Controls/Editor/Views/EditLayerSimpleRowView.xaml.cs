using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Teleopti.Ccc.Win.WpfControls.Controls.Editor.Views
{
    /// <summary>
    /// Interaction logic for EditLayerSimpleRowView.xaml
    /// </summary>
    public partial class EditLayerSimpleRowView : UserControl
    {
        public EditLayerSimpleRowView()
        {
            InitializeComponent();
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            //Had to do it like this, because the command was executed before the lostfocus event occured for the text boxes. Do you have a better way, Henke? /Robin
            if (e.Key==Key.Return)
            {
                var uiElement = sender as UIElement;
                if (uiElement!=null)
                    uiElement.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
        }
    }
}
