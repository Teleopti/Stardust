using System.Windows.Forms;

namespace Teleopti.Messaging.Management.Views
{
    public partial class MulticastAdressEdit : Form
    {
        public MulticastAdressEdit()
        {
            InitializeComponent();
        }

        public DataGridView DataGridView
        {
            get { return dataGridViewAddress; }
        }

        public BindingSource AddressInfoBindingSource
        {
            get { return iMulticastAddressInfoBindingSource; }
        }

    }
}
