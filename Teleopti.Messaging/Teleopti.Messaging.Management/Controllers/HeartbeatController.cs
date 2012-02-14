using Teleopti.Messaging.Management.Model;
using Teleopti.Messaging.Management.Views;

namespace Teleopti.Messaging.Management.Controllers
{
    public class HeartbeatController
    {
        private readonly ManagementViewController _parentController;
        private readonly HeartbeatModel _model;
        private readonly HeartbeatView _view;

        public HeartbeatController(ManagementViewController parentController, HeartbeatView view, HeartbeatModel model)
        {
            _parentController = parentController;
            _view = view;
            _model = model;

            Initialise();
        }

        private void Initialise()
        {
            _view.HeartbeatBindingSource.DataSource = _model.CreateBindingList();
        }

    }
}
