using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public class ResourceCalculateAfterDeleteDecider : IResourceCalculateAfterDeleteDecider
	{
		private readonly VirtualSkillGroupsResultProvider _virtualSkillGroupsResultProvider;
		private readonly ILimitForNoResourceCalculation _limitForNoResourceCalculation;

		public ResourceCalculateAfterDeleteDecider(VirtualSkillGroupsResultProvider virtualSkillGroupsResultProvider, 
							ILimitForNoResourceCalculation limitForNoResourceCalculation)
		{
			_virtualSkillGroupsResultProvider = virtualSkillGroupsResultProvider;
			_limitForNoResourceCalculation = limitForNoResourceCalculation;
		}

		public bool DoCalculation(IPerson agent, DateOnly date)
		{
			var skillGroupResult = _virtualSkillGroupsResultProvider.Fetch(date);
			return skillGroupResult.GetNumberOfAgentsInSkillGroupFromPerson(agent) < _limitForNoResourceCalculation.NumberOfAgents;
		}
	}
}