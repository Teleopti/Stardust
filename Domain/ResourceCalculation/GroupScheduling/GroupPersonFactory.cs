using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling
{
	public class GroupPersonFactory : IGroupPersonFactory
	{
		public  IGroupPerson CreateGroupPerson(IList<IPerson> persons, DateOnly dateOnly,string name, Guid? guid)
		{
			return new GroupPerson(persons, dateOnly, name, guid);
		}
	}
}