using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades
{
	public interface ISpecificationChecker
	{
		ShiftTradeRequestValidationResult Check(IList<IShiftTradeSwapDetail> swapDetails);
	}
}