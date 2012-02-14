using System.Windows.Forms;
using SdkTestClientWin.Domain;
using SdkTestClientWin.Infrastructure;
using SdkTestClientWin.Sdk;

namespace SdkTestWinGui
{
    public class WriteProtectionView
    {
        private readonly ServiceApplication _service;
        private readonly ListView _writeProtectionListView;
        private readonly DateTimePicker _dateTimePicker;
        private readonly Organization _organization;
        private TreeNode _selectedNode;

        public WriteProtectionView(ServiceApplication service, ListView writeProtectionListView, DateTimePicker dateTimePicker, Organization organization)
        {
            _service = service;
            _writeProtectionListView = writeProtectionListView;
            _dateTimePicker = dateTimePicker;
            _organization = organization;
        }

        private void Redraw()
        {
            if(_selectedNode == null)
                return;
            RedrawListView(_selectedNode);
        }

        public void RedrawListView(TreeNode selectedNode)
        {
            _writeProtectionListView.BeginUpdate();
            _writeProtectionListView.Items.Clear();
            _selectedNode = selectedNode;
            foreach (Agent agent in _organization.SelectedAgents(selectedNode))
            {
                var item = new ListViewItem(agent.Dto.Name);
                var dto = _service.OrganizationService.GetWriteProtectionDateOnPerson(agent.Dto);
                item.Tag = agent;
                var sub = new ListViewItem.ListViewSubItem();
                if(dto.WriteProtectedToDate != null)
                    sub.Text = dto.WriteProtectedToDate.DateTime.ToShortDateString();
                item.SubItems.Add(sub);

                _writeProtectionListView.Items.Add(item);
            }
            _writeProtectionListView.EndUpdate();
        }


        public void SetDateOnPersons()
        {
            if(_selectedNode == null)
                return;
            foreach (Agent agent in _organization.SelectedAgents(_selectedNode))
            {
                var dto = _service.OrganizationService.GetWriteProtectionDateOnPerson(agent.Dto);
                var dateOnlyDto = new DateOnlyDto {DateTime = _dateTimePicker.Value, DateTimeSpecified = true};
                dto.WriteProtectedToDate = dateOnlyDto;
                _service.OrganizationService.SetWriteProtectionDateOnPerson(dto);
            }

            Redraw();
        }
    }
}