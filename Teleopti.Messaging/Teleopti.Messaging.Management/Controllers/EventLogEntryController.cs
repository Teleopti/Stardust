using System.Collections.Generic;
using Teleopti.Messaging.Management.BindingLists;
using Teleopti.Messaging.Management.Model;
using Teleopti.Messaging.Management.Views;

namespace Teleopti.Messaging.Management.Controllers
{
    public class EventLogEntryController
    {
        private readonly ManagementViewController _parentController;
        private readonly EventLogEntryModel _model;
        private readonly EventLogEntryView _view;

        public EventLogEntryController(ManagementViewController parentController, EventLogEntryView view, EventLogEntryModel model)
        {

            _parentController = parentController;
            _view = view;
            _model = model;

            Initialise();

        }

        private void Initialise()
        {
            _model.CreateBindingListAsync(_model.CreateBindingList);
        }

        private delegate void SetBindingListHandler(LogbookEntryBindingList list);
        public void SetBindingList(LogbookEntryBindingList list)
        {
            if(_view.DataGridView.InvokeRequired)
            {
                _view.DataGridView.Invoke(new SetBindingListHandler(SetBindingList), new object[] {list});
            }
            else
            {
                _view.BindingSource.DataSource = list;
            }
        }

        private delegate void AutoCompleteHandler(List<string> users);
        public void SetAutoCompleteSourceUsers(List<string> users)
        {
            if (_view.DataGridView.InvokeRequired)
            {
                _view.DataGridView.Invoke(new AutoCompleteHandler(SetAutoCompleteSourceUsers), new object[] { users });
            }
            else
            {
                _view.BindingSource.DataSource = users;
            }            
        }

    }
}
