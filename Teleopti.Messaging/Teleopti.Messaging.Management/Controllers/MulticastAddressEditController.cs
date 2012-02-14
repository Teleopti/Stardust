using System.Windows.Forms;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Messaging.Management.Model;
using Teleopti.Messaging.Management.Views;

namespace Teleopti.Messaging.Management.Controllers
{
    public class MulticastAddressEditController
    {
        private readonly ManagementViewController _parentController;
        private readonly MulticastAddressEditModel _model;
        private readonly MulticastAdressEdit _view;

        public MulticastAddressEditController(ManagementViewController parentController, MulticastAdressEdit view, MulticastAddressEditModel model)
        {
            _parentController = parentController;
            _view = view;
            _model = model;

            Initialise();
        }

        private void Initialise()
        {
            _view.FormClosing += new FormClosingEventHandler(this.OnClosing);
            _view.DataGridView.UserDeletingRow += new DataGridViewRowCancelEventHandler(this.OnUserDeleting);
            _view.AddressInfoBindingSource.DataSource = _model.CreateBindingList();
        }

        private void OnClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult result = MessageBox.Show("Do you want to save any changes?", "Message Broker", MessageBoxButtons.YesNo);
            if(result == DialogResult.Yes)
                _model.SaveConfigurations();
        }

        private void OnUserDeleting(object sender, DataGridViewRowCancelEventArgs e)
        {
            IMessageInformation item = (IMessageInformation)e.Row.DataBoundItem;
            _model.DeleteConfigurationItem(item);
            _model.RemoveListItem((IMessageInformation)e.Row.DataBoundItem);
            e.Cancel = true;
        }
    }
}
