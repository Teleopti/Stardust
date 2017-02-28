using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades
{
	public interface IShiftTradeLightSpecification : ISpecification<ShiftTradeAvailableCheckItem>
	{
		string DenyReason { get; }
	}
}