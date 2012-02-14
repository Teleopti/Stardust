using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Messaging.Client;
using Teleopti.Messaging.Management.Model;
using Teleopti.Messaging.Management.Views;

namespace Teleopti.Messaging.Management.Controllers
{
    public class ServiceManagerController
    {
        private readonly ServiceManagerModel _model;
        private readonly ServiceManagerView _view;
        private readonly ManagementViewController _parentController;

        public ServiceManagerController(ManagementViewController parentController, ServiceManagerView view, ServiceManagerModel model)
        {
            _parentController = parentController;
            _view = view;
            _model = model;
            Initialise();
        }

        public string ConnectionString
        {
            get { return _parentController.ConnectionString; }
            set
            {
                _parentController.ConnectionString = value;
            }
        }

        private void Initialise()
        {
            _view.SvcDisplayNameText = "Teleopti Message Broker";
            _view.InstallServiceToolStripMenuItem.Click += InstallServiceToolStripMenuItem_Click;
            _view.UninstallServiceToolStripMenuItem.Click += UninstallServiceToolStripMenuItem_Click;
            _view.StopServiceToolStripMenuItem.Click += StopServiceToolStripMenuItem_Click;
            _view.StartServiceToolStripMenuItem.Click += StartServiceToolStripMenuItem_Click;
            _view.ExitToolStripMenuItem.Click += ExitToolStripMenuItem_Click;
            _view.ButtonStop.Click += buttonStop_Click;
            _view.ButtonStart.Click += buttonStart_Click;
            _view.ButtonInstall.Click += buttonInstall_Click;
            _view.ButtonServices.Click += buttonServices_Click;
            _view.ButtonUninstall.Click += buttonUninstall_Click;
            _view.SvcPathText = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase).Remove(0, 6);
            _view.ButtonMMC.Click += buttonMMC_Click;
            _view.ButtonMessageBrokerStart.Click += buttonMessageBrokerStart_Click;
            _view.ButtonMessageBrokerStop.Click += buttonMessageBrokerStop_Click;
            _view.ButtonSendMessage.Click += buttonSendMessage_Click;

        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _view.Hide();
        }

        private void StartServiceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _model.StartService(_view.SvcNameText);
        }

        private void StopServiceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _model.StopService(_view.SvcNameText);
        }

        private void UninstallServiceToolStripMenuItem_Click(object sender, EventArgs e)
        {

            _model.InstallService(_view.SvcPathText, _view.SvcNameText, _view.SvcDisplayNameText, "Uninstall");
        }

        private void InstallServiceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _model.InstallService(_view.SvcPathText, _view.SvcNameText, _view.SvcDisplayNameText, "Install");     
        }

        private void buttonServices_Click(object sender, EventArgs e)
        {
            _model.QueryStatus("TeleoptiBrokerService");
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            _model.StartService(_view.SvcNameText);
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            _model.StopService(_view.SvcNameText);
        }

        private void buttonInstall_Click(object sender, EventArgs e)
        {
            _model.InstallService(_view.SvcPathText, _view.SvcNameText, _view.SvcDisplayNameText, "Install");             
        }

        private void buttonUninstall_Click(object sender, EventArgs e)
        {
            _model.InstallService(_view.SvcPathText, _view.SvcNameText, _view.SvcDisplayNameText, "Uninstall");             
        }

        private void buttonMMC_Click(object sender, EventArgs e)
        {
            _model.LaunchMMC();
        }

        private void buttonMessageBrokerStart_Click(object sender, EventArgs e)
        {
            _model.MessageBrokerStart();
        }

        private void buttonMessageBrokerStop_Click(object sender, EventArgs e)
        {
            _model.MessageBrokerStop();
        }

        private void buttonSendMessage_Click(object sender, EventArgs e)
        {
            using(EventMessageView emv = new EventMessageView())
            {
                emv.ShowDialog(_view);
                if (emv.DialogResult == DialogResult.OK)
                {
                    string message = emv.Message;
                    IMessageSender messageSender = MessageSenderFactory.CreateMessageSender(ConfigurationManager.AppSettings["MessageBroker"]);
                    messageSender.InstantiateBrokerService();
                    messageSender.SendRtaData(Guid.Empty, new ExternalAgentState(Environment.UserName, message, new TimeSpan(1, 0, 0), DateTime.Now, Guid.Empty, 0, DateTime.Now, true));
                }
                
            }
        }

        public void UpdateProgressBarMessage(string message)
        {
            _parentController.UpdateStatusBarMessage(message);
        }

        public void UpdateProgressBar(double value)
        {
            _parentController.UpdateProgressbar(value);
        }

        private delegate void UpdateStringValueHandler(string status);
        public void UpdateStatus(string output)
        {
            if (_view.TextBoxStatus.InvokeRequired)
            {
                UpdateStringValueHandler handler = UpdateStatus;
                _view.TextBoxStatus.Invoke(handler, new object[] { output });
            }
            else
            {
                _view.TextBoxStatus.Text = output;
            }
        }

        public void AppendStatus(string output)
        {
            if (_view.TextBoxStatus.InvokeRequired)
            {
                UpdateStringValueHandler handler = AppendStatus;
                _view.TextBoxStatus.Invoke(handler, new object[] { output });
            }
            else
            {
                _view.TextBoxStatus.Text = _view.TextBoxStatus.Text + Environment.NewLine + output;
            }
        }
    }
}
