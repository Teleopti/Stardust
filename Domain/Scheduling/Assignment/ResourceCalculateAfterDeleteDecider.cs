using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Islands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public class ResourceCalculateAfterDeleteDecider : IResourceCalculateAfterDeleteDecider
	{
		private readonly SkillSetProvider _skillSetProvider;
		private readonly ILimitForNoResourceCalculation _limitForNoResourceCalculation;

		public ResourceCalculateAfterDeleteDecider(SkillSetProvider skillSetProvider, 
							ILimitForNoResourceCalculation limitForNoResourceCalculation)
		{
			_skillSetProvider = skillSetProvider;
			_limitForNoResourceCalculation = limitForNoResourceCalculation;
		}

		public bool DoCalculation(IPerson agent, DateOnly date)
		{
			var skillGroupResult = _skillSetProvider.Fetch();
			return skillGroupResult.NumberOfAgentsInSameSkillSet(agent) < _limitForNoResourceCalculation.NumberOfAgents;
		}
	}
}