using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using SdkTestClientWin.Domain;
using SdkTestClientWin.Infrastructure;
using SdkTestClientWin.Sdk;

namespace SdkTestWinGui
{
    public class PersonView
    {
        private readonly ServiceApplication _service;
        private readonly ListView _personListView;
        private readonly ListView _accountListView;
        private readonly DateTime _dateTime;
        private readonly SplitContainer _splitContainer;
        private readonly Organization _organization;
        private Agent _selectedAgent;
  
        public PersonView(ServiceApplication service, ListView personListView, ListView accountListView, DateTime dateTime,
                            SplitContainer splitContainer, Organization organization)
        {
            _personListView = personListView;
            _personListView.ItemSelectionChanged +=_personListView_ItemSelectionChanged;
            _accountListView = accountListView;
            _dateTime = dateTime;
            _splitContainer = splitContainer;
            _service = service;
            _organization = organization;

            var contextMenu = new ContextMenu();
            _accountListView.ContextMenu = contextMenu;
            contextMenu.MenuItems.Add(new MenuItem("Add", addPersonAccount_OnClick));
            contextMenu.MenuItems.Add(new MenuItem("Change", changePersonAccount_OnClick));
            contextMenu.MenuItems.Add(new MenuItem("Delete", deletePersonAccount_OnClick));
            _accountListView.MouseDown += _accountListView_MouseDown;
        }

        private void _accountListView_MouseDown(object sender, MouseEventArgs e)
        {
            if(e.Button.Equals( MouseButtons.Right) && _personListView.SelectedItems.Count!=0)
            {
                _selectedAgent = (Agent) _personListView.SelectedItems[0].Tag;
            }
        }

        private void _personListView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            ListViewItem item = e.Item;
            var agent = (Agent)item.Tag;
            reloadPersonAccounts(agent);
        }

        private void reloadPersonAccounts(Agent agent)
        {
            DateOnlyDto dateOnly = new DateOnlyDto();
            dateOnly.DateTime = _dateTime;
            dateOnly.DateTimeSpecified = true;
            _splitContainer.Enabled = false;
            _accountListView.Items.Clear();
  

            IList<PersonAccountDto> dtos = new List<PersonAccountDto>(_service.OrganizationService.GetPersonAccounts(agent.Dto, dateOnly));
            foreach (PersonAccountDto personAccountDto in dtos)
            {
                ListViewItem newItem = new ListViewItem(personAccountDto.TrackingDescription);
                newItem.SubItems.Add(new ListViewItem.ListViewSubItem(newItem, periodToString(personAccountDto.Period)));
                newItem.SubItems.Add(new ListViewItem.ListViewSubItem(newItem, personAccountDto.BalanceIn.ToString()));
                newItem.SubItems.Add(new ListViewItem.ListViewSubItem(newItem, personAccountDto.Extra.ToString()));
                newItem.SubItems.Add(new ListViewItem.ListViewSubItem(newItem, personAccountDto.Accrued.ToString()));
                newItem.SubItems.Add(new ListViewItem.ListViewSubItem(newItem, personAccountDto.LatestCalculatedBalance.ToString()));
                newItem.SubItems.Add(new ListViewItem.ListViewSubItem(newItem, calculateBalanceOut(personAccountDto).ToString()));
                newItem.Tag = personAccountDto;
                _accountListView.Items.Add(newItem);
            }
            _splitContainer.Enabled = true;
            _personListView.Focus();
        }

        private void addPersonAccount_OnClick(object sender, EventArgs e)
        {
            if (_selectedAgent == null) return;
            using(var updateform = new UpdatePersonAccountForm(_service))
            {
                updateform.Text = @"Add a person account";
                updateform.PersonAccountSaved += updateform_PersonAccountSaved;
                updateform.ShowDialog();
                updateform.PersonAccountSaved -= updateform_PersonAccountSaved;
            }
        }

        private void changePersonAccount_OnClick(object sender, EventArgs e)
        {
            if (_accountListView.SelectedItems.Count == 0 || _selectedAgent == null) return;
            var personAccount = (PersonAccountDto)_accountListView.SelectedItems[0].Tag;
            using (var updateform = new UpdatePersonAccountForm(_service, personAccount))
            {
                updateform.Text = @"Change a person account";
                updateform.PersonAccountSaved += updateform_PersonAccountSaved;
                updateform.ShowDialog();
                updateform.PersonAccountSaved -= updateform_PersonAccountSaved;
            }
        }

        private void deletePersonAccount_OnClick(object sender, EventArgs e)
        {
            if (_accountListView.SelectedItems.Count == 0 || _selectedAgent == null) return;
            var personAccount = (PersonAccountDto)_accountListView.SelectedItems[0].Tag;
            var absencesDtos = _service.SchedulingService.GetAbsences(new AbsenceLoadOptionDto() { LoadDeleted = false, LoadDeletedSpecified = true });
            var absence = absencesDtos.Where(a => a.Name.Equals(personAccount.TrackingDescription)).FirstOrDefault();
            if (absence == null) return;
            var dateOnly = new DateOnlyDto {DateTime = _dateTime, DateTimeSpecified = true};
            var command = new DeletePersonAccountForPersonCommandDto
                              {AbsenceId = absence.Id, DateFrom = dateOnly, PersonId = _selectedAgent.Dto.Id};
            _service.InternalService.ExecuteCommand(command);

            reloadPersonAccounts(_selectedAgent);
        }

        private void updateform_PersonAccountSaved(object sender, PersonAccountSavedEventArgs e)
        {
            var command = new SetPersonAccountForPersonCommandDto
                              {
                                  PersonId = _selectedAgent.Dto.Id,
                                  AbsenceId = e.AbsenceId.ToString(),
                                  DateFrom = e.DateFrom,
                                  Accrued = e.Accrued,
                                  AccruedSpecified = e.Accrued != null,
                                  BalanceIn = e.BalanceIn,
                                  BalanceInSpecified = e.BalanceIn != null,
                                  Extra = e.Extra,
                                  ExtraSpecified = e.Extra != null
                              };
            _service.InternalService.ExecuteCommand(command);

            reloadPersonAccounts(_selectedAgent);
        }

        public void DrawAgentInfo(TreeNode selectedNode)
        {
            _personListView.BeginUpdate();
            _personListView.Items.Clear();
            foreach (Agent agent in _organization.SelectedAgents(selectedNode))
            {
                ListViewItem item = new ListViewItem(agent.Dto.Name);
                item.Tag = agent;
                item.SubItems.Add(new ListViewItem.ListViewSubItem(item, agent.Dto.EmploymentNumber));
                item.SubItems.Add(new ListViewItem.ListViewSubItem(item, agent.Dto.Email));
                string team = string.Concat(agent.Team.Site.Dto.DescriptionName, "/", agent.Team.Dto.Description);
                item.SubItems.Add(new ListViewItem.ListViewSubItem(item, team));
                item.SubItems.Add(new ListViewItem.ListViewSubItem(item, agent.Dto.TimeZoneId));
                string cultureName;
                if (agent.Dto.UICultureLanguageId.HasValue)
                {
                    cultureName = CultureInfo.GetCultureInfo(agent.Dto.UICultureLanguageId.Value).DisplayName;
                    item.SubItems.Add(new ListViewItem.ListViewSubItem(item, cultureName));
                }
                if (agent.Dto.CultureLanguageId.HasValue)
                {
                    cultureName = CultureInfo.GetCultureInfo(agent.Dto.CultureLanguageId.Value).DisplayName;
                    item.SubItems.Add(new ListViewItem.ListViewSubItem(item, cultureName));
                }
                _personListView.Items.Add(item);
            }
            foreach (ColumnHeader header in _personListView.Columns)
            {
                header.AutoResize(ColumnHeaderAutoResizeStyle.HeaderSize);
            }
            _personListView.EndUpdate();
        }

        private static double calculateBalanceOut(PersonAccountDto personAccountDto)
        {
            return personAccountDto.BalanceIn
                   + personAccountDto.Extra
                   + personAccountDto.Accrued
                   - personAccountDto.LatestCalculatedBalance;
        }

        private static string periodToString(DateOnlyPeriodDto period)
        {
            string d1 = period.StartDate.DateTime.ToShortDateString();
            string d2 = period.EndDate.DateTime.ToShortDateString();
            return string.Concat(d1, "--", d2);
        }

    }
}
