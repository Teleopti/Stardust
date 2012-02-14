using System;
using System.Windows.Forms;
using Teleopti.Messaging.Management.Model;
using Teleopti.Messaging.Management.Views;

namespace Teleopti.Messaging.Management.Controllers
{
    public class ManagementViewController
    {
        private readonly ManagementView _view;
        private readonly ManagementModel _model;
        private string _connectionString;


        public ManagementViewController(ManagementView view, ManagementModel model)
        {

            _view = view;
            _model = model;

            Initialise();

        }

        private void Initialise()
        {
            _view.ExitToolStripMenuItem.Click += new EventHandler(this.exitToolStripMenuItem_Click);
            _view.ConfigurationToolStripMenuItem.Click += new EventHandler(this.configurationToolStripMenuItem_Click);
            _view.SubscriberViewToolStripMenuItem.Click += new EventHandler(this.subscriberViewToolStripMenuItem_Click);
            _view.MulticastAddressEditToolStripMenuItem.Click += new EventHandler(multicastAddressEditToolStripMenuItem_Click);
            _view.MessageViewToolStripMenuItem.Click += new EventHandler(this.messageViewToolStripMenuItem_Click);
            _view.LogViewToolStripMenuItem.Click += new EventHandler(this.logViewToolStripMenuItem_Click);
            _view.UserViewToolStripMenuItem.Click += new EventHandler(this.userViewToolStripMenuItem_Click);
            _view.ServiceToolStripMenuItem.Click += new EventHandler(ServiceManagerToolStripMenuItem_Click);
            _view.AboutMessageBrokerToolStripMenuItem.Click += new EventHandler(AboutMessageBrokerToolStripMenuItem_Click);
            _view.HeartbeatsToolStripMenuItem.Click += new EventHandler(HeartbeatsToolStripMenuItem_Click);
        }

        private void HeartbeatsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HeartbeatView beats = new HeartbeatView();
            beats.MdiParent = _view;
            HeartbeatModel heartbeatModel = new HeartbeatModel(ConnectionString);
            heartbeatModel.Controller = new HeartbeatController(this, beats, heartbeatModel);
            beats.Show();
            beats.Visible = true;            
        }

        private void AboutMessageBrokerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBoxMessageBroker about = new AboutBoxMessageBroker();
            about.ShowDialog(_view);
        }

        private void ServiceManagerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ServiceManagerView services = new ServiceManagerView();
            services.MdiParent = _view;
            ServiceManagerModel servicesModel = new ServiceManagerModel();
            servicesModel.Controller = new ServiceManagerController(this, services, servicesModel);
            services.Show();
            services.Visible = true;
        }

        private void messageViewToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void configurationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ConfigurationEdit editScreen = new ConfigurationEdit();
            editScreen.MdiParent = _view;
            ConfigurationEditModel configurationModel = new ConfigurationEditModel(ConnectionString);
            configurationModel.Controller = new ConfigurationEditController(this, editScreen, configurationModel);
            editScreen.Show();
            editScreen.Visible = true;
        }

        private void multicastAddressEditToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MulticastAdressEdit editScreen = new MulticastAdressEdit();
            editScreen.MdiParent = _view;
            MulticastAddressEditModel model = new MulticastAddressEditModel(ConnectionString);
            model.Controller = new MulticastAddressEditController(this, editScreen, model);
            editScreen.Show();
            editScreen.Visible = true;
        }

        private void logViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EventLogEntryView logScreen = new EventLogEntryView();
            logScreen.MdiParent = _view;
            EventLogEntryModel model = new EventLogEntryModel(ConnectionString);
            model.Controller = new EventLogEntryController(this, logScreen, model);
            logScreen.Show();
            logScreen.Visible = true;
        }

        private void userViewToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void subscriberViewToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        public void UpdateStatusBarMessage(string message)
        {
            
        }

        public void UpdateProgressbar(double value)
        {
            
        }


        public string ConnectionString
        {
            get { return _connectionString; }
            set { _connectionString = value; }
        }
    }
}
