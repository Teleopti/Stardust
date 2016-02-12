using System;
using System.Collections.Generic;
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
			var virtualSkillGroupCreator = new VirtualSkillGroupsCreator();
			var result = virtualSkillGroupCreator.GroupOnDate(date, _scheduleResultStateHolder().PersonsInOrganization);

			return result.GetNumberOfAgentsInSkillGroupFromPerson(agent) < _limitForNoResourceCalculation.NumberOfAgents;
		}
	}
}