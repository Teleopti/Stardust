namespace Teleopti.Ccc.WinCode.Budgeting.Presenters
{
	public interface IBudgetDaySource : ISelectedBudgetDays
	{
		double? GetFulltimeEquivalentHoursPerDay(double fulltimeEquivalentHours);
	}
}