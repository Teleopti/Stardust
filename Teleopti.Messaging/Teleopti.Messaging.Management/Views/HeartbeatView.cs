using System.Windows.Forms;

namespace Teleopti.Messaging.Management.Views
{
    public partial class HeartbeatView : Form
    {
        public HeartbeatView()
        {
            InitializeComponent();
        }

        public BindingSource HeartbeatBindingSource
        {
            get { return iEventHeartbeatBindingSource; }
        }
    }
}
