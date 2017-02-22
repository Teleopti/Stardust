﻿using Teleopti.Ccc.Domain.Islands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public class ResourceCalculateAfterDeleteDecider : IResourceCalculateAfterDeleteDecider
	{
		private readonly SkillGroupInfoProvider _skillGroupInfoProvider;
		private readonly ILimitForNoResourceCalculation _limitForNoResourceCalculation;

		public ResourceCalculateAfterDeleteDecider(SkillGroupInfoProvider skillGroupInfoProvider, 
							ILimitForNoResourceCalculation limitForNoResourceCalculation)
		{
			_skillGroupInfoProvider = skillGroupInfoProvider;
			_limitForNoResourceCalculation = limitForNoResourceCalculation;
		}

		public bool DoCalculation(IPerson agent, DateOnly date)
		{
			var skillGroupResult = _skillGroupInfoProvider.Fetch();
			return skillGroupResult.NumberOfAgentsInSameSkillGroup(agent) < _limitForNoResourceCalculation.NumberOfAgents;
		}
	}
}