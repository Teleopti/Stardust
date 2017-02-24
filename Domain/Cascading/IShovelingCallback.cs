using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public interface IShovelingCallback
	{
		void ResourcesWasMovedTo(ISkill skillToMoveTo, DateTimePeriod interval, IEnumerable<ISkill> primarySkillsMovedFrom, double resources);
		void ResourcesWasRemovedFrom(ISkill primarySkill, DateTimePeriod interval, double resources);
		void BeforeShoveling(IShovelResourceData shovelResourceData);
	}
}