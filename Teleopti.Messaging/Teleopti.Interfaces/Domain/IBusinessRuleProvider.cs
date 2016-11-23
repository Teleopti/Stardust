namespace Teleopti.Interfaces.Domain
{
	public interface IBusinessRuleProvider
	{
		INewBusinessRuleCollection GetAllBusinessRules(ISchedulingResultStateHolder schedulingResultStateHolder);

		INewBusinessRuleCollection GetBusinessRulesForShiftTradeRequest(
			ISchedulingResultStateHolder schedulingResultStateHolder, bool enableSiteOpenHoursRule);
	}
}