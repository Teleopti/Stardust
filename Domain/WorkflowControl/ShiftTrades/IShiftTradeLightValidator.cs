namespace Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades
{
	public interface IShiftTradeLightValidator
	{
		ShiftTradeRequestValidationResult Validate(ShiftTradeAvailableCheckItem checkItem);
	}
}