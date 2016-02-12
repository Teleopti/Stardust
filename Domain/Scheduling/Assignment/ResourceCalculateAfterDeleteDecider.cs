using System;
using System.Linq;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public class ResourceCalculateAfterDeleteDecider : IResourceCalculateAfterDeleteDecider
	{
		private readonly ILimitForNoResourceCalculation _limitForNoResourceCalculation;
		private readonly Func<ISchedulingResultStateHolder> _scheduleResultStateHolder;

		public ResourceCalculateAfterDeleteDecider(ILimitForNoResourceCalculation limitForNoResourceCalculation, Func<ISchedulingResultStateHolder> scheduleResultStateHolder)
		{
			_limitForNoResourceCalculation = limitForNoResourceCalculation;
			_scheduleResultStateHolder = scheduleResultStateHolder;
		}

		public bool DoCalculation(IPerson agent, DateOnly date)
		{
			var count = 0;
			var agentPeriod = agent.Period(date);
			if (agentPeriod == null)
				return false;

			foreach (var person in _scheduleResultStateHolder().PersonsInOrganization)
			{
				var otherPeriod = person.Period(date);
				if(otherPeriod == null) continue;
				if (hasSameSkills(otherPeriod, agentPeriod))
					count++;
			}

			return count < _limitForNoResourceCalculation.NumberOfAgents;
		}

		private bool hasSameSkills(IPersonPeriod personPeriod1, IPersonPeriod personPeriod2)
		{
			var skills1 = personPeriod1.PersonSkillCollection.Where(x => x.Active).Select(x => x.Skill).Where(x => !((IDeleteTag)x).IsDeleted);
			var skills2 = personPeriod2.PersonSkillCollection.Where(x => x.Active).Select(x => x.Skill).Where(x => !((IDeleteTag)x).IsDeleted);
			if (skills1.Count() != skills2.Count())
				return false;
			foreach (var skill in skills1)
			{
				if (!skills2.Contains(skill))
					return false;
			}
			return true;
		}
	}
}