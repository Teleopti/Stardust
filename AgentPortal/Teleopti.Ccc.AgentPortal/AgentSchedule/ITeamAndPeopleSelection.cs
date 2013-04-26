using System.Collections.Generic;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.AgentPortal.AgentSchedule
{
	internal interface ITeamAndPeopleSelection
	{
		IEnumerable<PersonDto> SelectedPeople { get; }
		ICollection<GroupForPeople> SelectedTeams { get; }
		void Initialize(bool filterEnabled);
	}
}