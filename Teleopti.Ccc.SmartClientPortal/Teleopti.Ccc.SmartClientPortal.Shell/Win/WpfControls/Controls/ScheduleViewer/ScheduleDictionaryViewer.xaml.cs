using System.Windows;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Win.WpfControls.Controls.ScheduleViewer.Models;

namespace Teleopti.Ccc.Win.WpfControls.Controls.ScheduleViewer
{
    /// <summary>
    /// Interaction logic for ScheduleViewer.xaml
    /// </summary>
    public partial class ScheduleDictionaryViewer : Window
    {
        public ScheduleDictionaryViewer(IScheduleDictionary dictionary)
        {
            InitializeComponent();
            this.DataContext = new ScheduleViewerViewModel(dictionary);
        }
    }

    

    

   

   

}
