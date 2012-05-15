using System.Collections.Generic;
using Teleopti.Ccc.Sdk.Client.SdkServiceReference;

namespace Teleopti.Ccc.AgentPortal.AgentSchedule
{
	internal interface ITeamAndPeopleSelection
	{
		IEnumerable<PersonDto> SelectedPeople { get; }
		ICollection<GroupForPeople> SelectedTeams { get; }
		void Initialize(bool filterEnabled);
	}
}