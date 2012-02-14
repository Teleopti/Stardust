using System.Windows;
using Teleopti.Ccc.WpfControls.Controls.ScheduleViewer.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WpfControls.Controls.ScheduleViewer
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
