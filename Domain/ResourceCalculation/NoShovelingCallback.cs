using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class NoShovelingCallback : IShovelingCallback
	{
		public void ResourcesWasMovedTo(ISkill skillToMoveTo, DateTimePeriod interval, IEnumerable<ISkill> primarySkillsMovedFrom, double resources)
		{
		}

		public void ResourcesWasRemovedFrom(ISkill primarySkill, DateTimePeriod interval, double resources)
		{
		}

		public void BeforeShoveling(IShovelResourceData shovelResourceData)
		{
		}
	}
}