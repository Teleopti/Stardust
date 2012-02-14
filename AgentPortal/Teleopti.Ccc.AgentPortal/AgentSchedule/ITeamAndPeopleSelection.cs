using System.Collections.Generic;
using Teleopti.Ccc.Sdk.Client.SdkServiceReference;

namespace Teleopti.Ccc.AgentPortal.AgentSchedule
{
	internal interface ITeamAndPeopleSelection
	{
		IEnumerable<PersonDto> SelectedPeople { get; }
		ICollection<TeamDto> SelectedTeams { get; }
		void Initialize();
	}
}