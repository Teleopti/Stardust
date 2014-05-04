using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using SdkTestClientWin.Domain;
using SdkTestClientWin.Infrastructure;
using SdkTestClientWin.Sdk;

namespace SdkTestWinGui
{
    public class PermissionView
    {
        private readonly ServiceApplication _service;
        private readonly ListView _personListView;
        private readonly ListView _rolesListView;
        private readonly Organization _organization;
	    private readonly ComboBox _comboBoxRoles;

	    public PermissionView(ServiceApplication service, ListView personListView, ListView rolesListView,
		    Organization organization, ComboBox comboBoxRoles)
	    {
		    _personListView = personListView;
		    _rolesListView = rolesListView;
		    _personListView.ItemSelectionChanged += _personListView_ItemSelectionChanged;
		    _service = service;
		    _organization = organization;
		    _comboBoxRoles = comboBoxRoles;
		    DrawAgentInfo();

	    }

	    public void LoadAllRoles()
	    {
		    if(_comboBoxRoles.Items.Count > 0) return;
		    _comboBoxRoles.DisplayMember = "Name";
		    _comboBoxRoles.ValueMember = "Id";
			 var roles = _service.OrganizationService.GetRolesByQuery(new GetAllRolesQueryDto());
		    _comboBoxRoles.DataSource = roles;
	    }

        private void _personListView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            ListViewItem item = e.Item;
            var agent = (Agent)item.Tag;
            ReloadRoles(agent);
        }

        private void ReloadRoles(Agent agent)
        {
            _rolesListView.Items.Clear();
	        IList<RoleDto> dtos =
		        _service.OrganizationService.GetRolesByQuery(new GetPersonRolesQueryDto {PersonId = agent.Dto.Id});
            foreach (var roleDto in dtos)
            {
					var newItem = new ListViewItem(roleDto.Name);
                
                _rolesListView.Items.Add(newItem);
            }
            _personListView.Focus();
        }

        public void GrantRole()
        {
			  var selectedAgent = (Agent)_personListView.SelectedItems[0].Tag;
            if (selectedAgent == null) return;
				if (_comboBoxRoles.SelectedItem == null) return;
				var role = ((RoleDto)_comboBoxRoles.SelectedItem).Id;
				_service.OrganizationService.GrantPersonRole(new GrantPersonRoleCommandDto
				{
					PersonId = selectedAgent.Dto.Id,
					RoleId = role
				});
				ReloadRoles(selectedAgent);
        }

        public void RevokeRole()
        {
			  var selectedAgent = (Agent)_personListView.SelectedItems[0].Tag;
			  if (selectedAgent == null) return;
            
			  if(_comboBoxRoles.SelectedItem == null) return;
	        var role = ((RoleDto) _comboBoxRoles.SelectedItem).Id;
	        _service.OrganizationService.RevokePersonRole(new RevokePersonRoleCommandDto
	        {
		        PersonId = selectedAgent.Dto.Id,
		        RoleId = role
	        });
				ReloadRoles(selectedAgent);
        }

        private void DrawAgentInfo()
        {
            _personListView.BeginUpdate();
            _personListView.Items.Clear();
            foreach (Agent agent in _organization.AgentCollection)
            {
                var item = new ListViewItem(agent.Dto.Name) {Tag = agent};

	            string team = string.Concat(agent.Team.Site.Dto.DescriptionName, "/", agent.Team.Dto.Description);
                item.SubItems.Add(new ListViewItem.ListViewSubItem(item, team));
                
                _personListView.Items.Add(item);
            }
            foreach (ColumnHeader header in _personListView.Columns)
            {
                header.AutoResize(ColumnHeaderAutoResizeStyle.HeaderSize);
            }
            _personListView.EndUpdate();
        }

    }
}
