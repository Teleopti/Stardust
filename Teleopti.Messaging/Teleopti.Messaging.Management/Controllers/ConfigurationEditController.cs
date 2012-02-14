using System.Windows.Forms;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Messaging.Management.Model;
using Teleopti.Messaging.Management.Views;

namespace Teleopti.Messaging.Management.Controllers
{
    public class ConfigurationEditController
    {
        private readonly ManagementViewController _parentController;
        private readonly ConfigurationEditModel _model;
        private readonly ConfigurationEdit _view;

        public ConfigurationEditController(ManagementViewController parentController, ConfigurationEdit view, ConfigurationEditModel model)
        {
            _parentController = parentController;
            _view = view;
            _model = model;

            Initialise();
        }

        private void Initialise()
        {
            _view.FormClosing += OnClosing;
            _view.DataGridView.UserDeletingRow += OnUserDeleting;
            _view.ConfigurationInfoBindingSource.DataSource = _model.CreateBindingList();
        }

        private void OnClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult result = MessageBox.Show("Do you want to save any changes?", "Message Broker", MessageBoxButtons.YesNo);
            if(result == DialogResult.Yes)
                _model.SaveConfigurations();
        }

        private void OnUserDeleting(object sender, DataGridViewRowCancelEventArgs e)
        {
            IConfigurationInfo item = (IConfigurationInfo)e.Row.DataBoundItem;
            _model.DeleteConfigurationItem(item);
            _model.RemoveListItem((IConfigurationInfo)e.Row.DataBoundItem);
            e.Cancel = true;
        }

    }
}
