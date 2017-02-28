using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades
{
	public interface ISpecificationChecker
	{
		ShiftTradeRequestValidationResult Check(IList<IShiftTradeSwapDetail> swapDetails);
	}
}