using System;
using System.Windows;
using Syncfusion.Windows.Forms;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.WpfControls.FileImport.ViewModels;
using Teleopti.Ccc.WinCode.FileImport;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.WpfControls.FileImport
{
    /// <summary>
    /// Interaction logic for ImportWizardWindow.xaml
    /// </summary>
    public partial class ImportWizardWindow : Window
    {
        public ImportWizardWindow()
        {
            InitializeComponent();
        }

        public ImportWizardWindow(string fileName, TimeZoneInfo defaultTimeZone)
        {
            InitializeComponent();
            DataContext = new PrepareTextViewModel(fileName, defaultTimeZone);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext != null)
            {
                try
                {
                    DataContext = ((IModel)DataContext).NextStep();
                }
                catch (FileImportException ex)
                {
                    MessageBoxAdv.Show(ex.Message, UserTexts.Resources.ImportError);
                    Close();
                }
                if (DataContext == null)
                {
                    Close();
                }
            }
            else
            {
                Close();
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}