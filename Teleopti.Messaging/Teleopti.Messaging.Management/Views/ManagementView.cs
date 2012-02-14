using System.Windows.Forms;

namespace Teleopti.Messaging.Management.Views
{
    public partial class ManagementView : Form
    {

        public ManagementView()
        {
            InitializeComponent();
        }

        public MenuStrip MenuStrip
        {
            get { return menuStrip; }
        }

        public ToolStripMenuItem FileToolStripMenuItem
        {
            get { return fileToolStripMenuItem; }
        }

        public ToolStripMenuItem ExitToolStripMenuItem
        {
            get { return exitToolStripMenuItem; }
        }

        public ToolStripMenuItem EditToolStripMenuItem
        {
            get { return editToolStripMenuItem; }
        }

        public ToolStripMenuItem ViewToolStripMenuItem
        {
            get { return viewToolStripMenuItem; }
        }

        public ToolStripMenuItem ConfigurationToolStripMenuItem
        {
            get { return configurationToolStripMenuItem; }
        }

        public ToolStripMenuItem MulticastAddressEditToolStripMenuItem
        {
            get { return multicastAddressEditToolStripMenuItem; }
        }

        public ToolStripMenuItem MessageViewToolStripMenuItem
        {
            get { return messageViewToolStripMenuItem; }
        }

        public ToolStripMenuItem LogViewToolStripMenuItem
        {
            get { return logViewToolStripMenuItem; }
        }

        public ToolStripMenuItem UserViewToolStripMenuItem
        {
            get { return userViewToolStripMenuItem; }
        }

        public ToolStripMenuItem SubscriberViewToolStripMenuItem
        {
            get { return subscriberViewToolStripMenuItem; }
        }

        public ToolStripMenuItem ToolsToolStripMenuItem
        {
            get { return toolsToolStripMenuItem; }
        }

        public ToolStripMenuItem AboutToolStripMenuItem
        {
            get { return aboutToolStripMenuItem; }
        }

        public ToolStripMenuItem AboutMessageBrokerToolStripMenuItem
        {
            get { return aboutMessageBrokerToolStripMenuItem; }
        }

        public ToolStripMenuItem ServiceToolStripMenuItem
        {
            get { return installServiceToolStripMenuItem; }
        }

        public ToolStripMenuItem HeartbeatsToolStripMenuItem
        {
            get { return heartbeatsToolStripMenuItem; }
        }

    }
}