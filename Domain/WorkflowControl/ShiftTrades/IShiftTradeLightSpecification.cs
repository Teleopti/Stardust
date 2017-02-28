using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades
{
	public interface IShiftTradeLightSpecification : ISpecification<ShiftTradeAvailableCheckItem>
	{
		string DenyReason { get; }
	}
}