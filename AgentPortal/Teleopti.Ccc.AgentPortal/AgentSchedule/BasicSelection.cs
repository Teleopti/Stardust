using System;
using System.Collections.Generic;
using Teleopti.Ccc.AgentPortalCode.Foundation.StateHandlers;
using Teleopti.Ccc.AgentPortalCode.Helper;
using Teleopti.Ccc.Sdk.Client.SdkServiceReference;

namespace Teleopti.Ccc.AgentPortal.AgentSchedule
{
	internal class BasicSelection : ITeamAndPeopleSelection
	{
		private readonly DateTime _selectedDate;
		private readonly GroupDetailModel _selectedTeam;

		public BasicSelection(DateTime selectedDate, GroupDetailModel selectedTeam)
		{
			_selectedDate = selectedDate;
			_selectedTeam = selectedTeam;
		}

		public IEnumerable<PersonDto> SelectedPeople { get; private set; }

		public ICollection<GroupForPeople> SelectedTeams { get; private set; }

		public void Initialize(bool filterEnabled)
		{
			var teamDtos = new List<GroupForPeople>();
			var persons = new List<PersonDto>();

				teamDtos.Add(new GroupForPeople { GroupId = _selectedTeam.Id });
			
			if(!filterEnabled)
			persons.AddRange(SdkServiceHelper.OrganizationService.GetPersonsByQuery(new GetPeopleByGroupPageGroupQueryDto
			                                                                        	{
			                                                                        		GroupPageGroupId = _selectedTeam.Id,
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
                           GroupPageGroupId = _selectedTeam.Id,
                           PersonId = StateHolder.Instance.State.SessionScopeData.LoggedOnPerson.Id,
                           QueryDate = new DateOnlyDto { DateTime = _selectedDate, DateTimeSpecified = true }
                       }));
			SelectedPeople = persons;
			SelectedTeams = teamDtos;
		}
	}

	internal class GroupForPeople
	{
		public string GroupId { get; set; }
	}
}