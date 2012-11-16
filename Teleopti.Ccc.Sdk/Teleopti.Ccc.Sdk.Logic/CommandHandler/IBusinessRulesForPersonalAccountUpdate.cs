using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.Logic.CommandHandler
{
	public interface IBusinessRulesForPersonalAccountUpdate
	{
		INewBusinessRuleCollection FromScheduleRange(IScheduleRange scheduleRange);
	}
}