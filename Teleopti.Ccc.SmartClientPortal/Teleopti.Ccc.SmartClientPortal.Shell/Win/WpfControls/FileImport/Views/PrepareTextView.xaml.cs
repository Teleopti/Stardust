using System.Windows.Controls;
using Teleopti.Ccc.Win.WpfControls.FileImport.ViewModels;

namespace Teleopti.Ccc.Win.WpfControls.FileImport.Views
{
    /// <summary>
    /// Interaction logic for PrepareTextView.xaml
    /// </summary>
    public partial class PrepareTextView : UserControl
    {
        public PrepareTextView()
        {
            InitializeComponent();
        }

        public PrepareTextView(PrepareTextViewModel model)
        {

            InitializeComponent();
            DataContext = model;
        }

        private void combo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext != null && combo.SelectedValue != null)
            {
                ((PrepareTextViewModel)DataContext).UpdateEncoding(combo.SelectedValue);
            }
        }

        private void separator_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext != null)
            {
                ((PrepareTextViewModel)DataContext).Separator = separatorCombo.SelectedValue.ToString();

            }
        }

    }
}
