using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling
{
	public interface IGroupPersonFactory
	{
		IGroupPerson CreateGroupPerson(IList<IPerson> persons, DateOnly dateOnly, string name);
	}
}