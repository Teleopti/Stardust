namespace Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades
{
	public interface IShiftTradeLightValidator
	{
		bool Validate(ShiftTradeAvailableCheckItem checkItem);
	}
}