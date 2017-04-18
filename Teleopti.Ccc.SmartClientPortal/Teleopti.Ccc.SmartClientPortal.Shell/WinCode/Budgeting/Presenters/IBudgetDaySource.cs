namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Presenters
{
	public interface IBudgetDaySource : ISelectedBudgetDays
	{
		double? GetFulltimeEquivalentHoursPerDay(double fulltimeEquivalentHours);
	}
}