using System;
using System.Collections.Generic;
using Teleopti.Ccc.AgentPortalCode.Helper;
using Teleopti.Ccc.Sdk.Client.SdkServiceReference;

namespace Teleopti.Ccc.AgentPortal.AgentSchedule
{
	internal class BasicSelection : ITeamAndPeopleSelection
	{
		private readonly DateTime _selectedDate;
		private readonly GroupDetailModel _selectedTeam;
		private readonly GroupPageDto _selectedPage;

		public BasicSelection(DateTime selectedDate, GroupDetailModel selectedTeam, GroupPageDto selectedPage)
		{
			_selectedDate = selectedDate;
			_selectedTeam = selectedTeam;
			_selectedPage = selectedPage;
		}

		public IEnumerable<PersonDto> SelectedPeople { get; private set; }

		public ICollection<TeamDto> SelectedTeams { get; private set; }

		public void Initialize(bool filterEnabled)
		{
			var teamDtos = new List<TeamDto>();
			var persons = new List<PersonDto>();

			if (_selectedPage.Id.ToUpperInvariant() == ScheduleTeamView.PageMain)
			{
				teamDtos.Add(new TeamDto { Id = _selectedTeam.Id });
			}
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
                persons.AddRange(SdkServiceHelper.OrganizationService.GetPeopleForShiftTradeByQuery(
                       new GetPeopleForShiftTradeByGroupPageGroupQueryDto
                       {
                           GroupPageGroupId = _selectedTeam.Id,
                           PersonId = SdkServiceHelper.LogOnServiceClient.GetLoggedOnPerson().Id,
                           QueryDate = new DateOnlyDto { DateTime = _selectedDate, DateTimeSpecified = true }
                       }));
			SelectedPeople = persons;
			SelectedTeams = teamDtos;
		}
	}
}