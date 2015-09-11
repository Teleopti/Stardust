using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo
{
	public class PeopleSelection
	{
		public PeopleSelection(IList<IPerson> allPeople, IList<IPerson> fixedStaffPeople)
		{
			FixedStaffPeople = fixedStaffPeople;
			AllPeople = allPeople;
		}

		public IList<IPerson> AllPeople { get; private set; } 
		public IList<IPerson> FixedStaffPeople { get; private set; } 
	}
}