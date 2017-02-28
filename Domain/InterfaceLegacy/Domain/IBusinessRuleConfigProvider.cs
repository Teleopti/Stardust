using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	public interface IBusinessRuleConfigProvider
	{
		IEnumerable<IShiftTradeBusinessRuleConfig> GetDefaultConfigForShiftTradeRequest();
	}
}