using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	public class PeopleSelection
	{
		public PeopleSelection(IList<IPerson> allPeople, IList<IPerson> selectedPeople)
		{
			SelectedPeople = selectedPeople;
			AllPeople = allPeople;
		}

		public IList<IPerson> AllPeople { get; private set; } 
		public IList<IPerson> SelectedPeople { get; private set; } 
	}
}