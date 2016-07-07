using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Rules
{
	public class BusinessRuleProvider : IBusinessRuleProvider
	{
		public INewBusinessRuleCollection GetAllBusinessRules(ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			return NewBusinessRuleCollection.All(schedulingResultStateHolder);
		}
	}
}
