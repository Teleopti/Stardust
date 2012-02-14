using System.Windows.Forms;

namespace Teleopti.Messaging.Management.Views
{
    public partial class EventLogEntryView : Form
    {
        public EventLogEntryView()
        {
            InitializeComponent();
        }

        public DataGridView DataGridView
        {
            get { return dataGridViewLog; }
        }

        public BindingSource BindingSource
        {
            get { return iLogbookEntryBindingSource; }
        }


    }
}
