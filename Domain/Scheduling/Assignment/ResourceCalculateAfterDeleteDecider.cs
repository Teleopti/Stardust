using System;
using System.Linq;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Interfaces.Domain;

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
			var virtualSkillGroupsCreator = new VirtualSkillGroupsCreator();
			var result = virtualSkillGroupsCreator.GroupOnDate(date, _scheduleResultStateHolder().PersonsInOrganization);
			var persons =  result.GetPersonsForSkillGroupKey(agent.Period(date).PersonSkillCollection.First().Skill.Id.ToString());

			return persons.Count() < _limitForNoResourceCalculation.NumberOfAgents;
		}
	}
}