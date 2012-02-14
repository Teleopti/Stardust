using System.Windows.Forms;

namespace Teleopti.Messaging.Management.Views
{
    public partial class ServiceManagerView : Form
    {
        public ServiceManagerView()
        {
            InitializeComponent();
        }

        public MenuStrip MenuStripServiceManager
        {
            get { return menuStripServiceManager; }
        }

        public ToolStripMenuItem ExitToolStripMenuItem
        {
            get { return exitToolStripMenuItem; }
        }

        public ToolStripMenuItem ActionsToolStripMenuItem
        {
            get { return actionsToolStripMenuItem; }
        }

        public ToolStripMenuItem StartServiceToolStripMenuItem
        {
            get { return startServiceToolStripMenuItem; }
        }

        public ToolStripMenuItem StopServiceToolStripMenuItem
        {
            get { return stopServiceToolStripMenuItem; }
        }

        public ToolStripMenuItem InstallServiceToolStripMenuItem
        {
            get { return installServiceToolStripMenuItem; }
        }

        public ToolStripMenuItem UninstallServiceToolStripMenuItem
        {
            get { return uninstallServiceToolStripMenuItem; }
        }

        public string SvcNameText
        {
            get { return svcNameTextBox.Text; }
        }

        public string SvcPathText
        {
            get { return svcPathTextBox.Text; }
            set { svcPathTextBox.Text = value; }
        }

        public string SvcDisplayNameText
        {
            get { return svcDisplayNameTextBox.Text; }
            set { svcDisplayNameTextBox.Text = value; }
        }

        public Button ButtonServices
        {
            get { return buttonServices; }
        }

        public Button ButtonStop
        {
            get { return buttonStop; }
        }

        public Button ButtonStart
        {
            get { return buttonStar; }
        }

        public Button ButtonInstall
        {
            get { return buttonInstall; }
        }

        public Button ButtonUninstall
        {
            get { return buttonUninstall; }
        }

        public TextBox TextBoxStatus
        {
            get { return textBoxStatus; }
        }

        public Button ButtonMMC
        {
            get { return buttonMMC; }
        }

        public Button ButtonSendMessage
        {
            get { return buttonSendMessage; }
            set { buttonSendMessage = value; }
        }

        public Button ButtonMessageBrokerStart
        {
            get { return buttonMessageBrokerStart; }
            set { buttonMessageBrokerStart = value; }
        }

        public Button ButtonMessageBrokerStop
        {
            get { return buttonMessageBrokerStop; }
            set { buttonMessageBrokerStop = value; }
        }

    }
}
