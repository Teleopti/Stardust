using System;
using System.Collections;
using System.Collections.Generic;
using Teleopti.Ccc.AgentPortalCode.Foundation.StateHandlers;
using Teleopti.Ccc.AgentPortalCode.Helper;
using Teleopti.Ccc.Sdk.Client.SdkServiceReference;

namespace Teleopti.Ccc.AgentPortal.AgentSchedule
{
	internal class AllTeamsSelection : ITeamAndPeopleSelection
	{
		private readonly DateTime _selectedDate;
		private readonly GroupDetailModel _selectedTeam;
		private readonly IEnumerable _allTeams;

		public AllTeamsSelection(DateTime selectedDate, GroupDetailModel selectedTeam, IEnumerable allTeams)
		{
			_selectedDate = selectedDate;
			_selectedTeam = selectedTeam;
			_allTeams = allTeams;
		}

		public IEnumerable<PersonDto> SelectedPeople { get; private set; }

		public ICollection<TeamDto> SelectedTeams { get; private set; }

		public void Initialize(bool filterEnabled)
		{
			var teamDtos = new List<TeamDto>();
			var persons = new List<PersonDto>();

			foreach (GroupDetailModel groupDetailModel in _allTeams)
			{
				if (groupDetailModel == _selectedTeam) continue;

				teamDtos.Add(new TeamDto { Id = groupDetailModel.Id });
                if (!filterEnabled)
                    persons.AddRange(SdkServiceHelper.OrganizationService.GetPersonsByQuery(new GetPeopleByGroupPageGroupQueryDto
                                                                                                {
                                                                                                    GroupPageGroupId = groupDetailModel.Id,
                                                                                                    QueryDate =
                                                                                                        new DateOnlyDto
                                                                                                            {
                                                                                                                DateTime = _selectedDate,
                                                                                                                DateTimeSpecified = true
                                                                                                            }
                                                                                                }));
                else
                    persons.AddRange(SdkServiceHelper.OrganizationService.GetPersonsByQuery(
                        new GetPeopleForShiftTradeByGroupPageGroupQueryDto
                            {
                                GroupPageGroupId = groupDetailModel.Id,
                                PersonId = StateHolder.Instance.State.SessionScopeData.LoggedOnPerson.Id,
                                QueryDate = new DateOnlyDto {DateTime = _selectedDate, DateTimeSpecified = true}
                            }));
            }

			SelectedPeople = persons;
			SelectedTeams = teamDtos;
		}
	}
}