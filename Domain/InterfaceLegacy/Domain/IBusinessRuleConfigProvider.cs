using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IBusinessRuleConfigProvider
	{
		IEnumerable<IShiftTradeBusinessRuleConfig> GetDefaultConfigForShiftTradeRequest();
	}
}