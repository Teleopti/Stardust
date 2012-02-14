using System.Windows.Forms;

namespace Teleopti.Messaging.Management.Views
{
    public partial class ConfigurationEdit : Form
    {
        public ConfigurationEdit()
        {
            InitializeComponent();
        }

        public BindingSource ConfigurationInfoBindingSource
        {
            get { return iConfigurationInfoBindingSource; }
            set { iConfigurationInfoBindingSource = value; }
        }

        public DataGridView DataGridView
        {
            get { return dataGridView; }
        }

    }
}
