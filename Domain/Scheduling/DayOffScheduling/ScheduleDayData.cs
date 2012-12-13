using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.DayOffScheduling
{
	public interface IScheduleDayData
	{
		DateOnly Date { get; }
		bool IsDayOff { get; set; }
		bool IsContractDayOff { get; set; }
		bool IsScheduled { get; set; }
		bool HaveRestriction { get; set; }
	}

	public class ScheduleDayData : IScheduleDayData
	{
		public DateOnly Date { get; private set; }
		public bool IsDayOff { get; set; }
		public bool IsContractDayOff { get; set; }
		public bool IsScheduled { get; set; }
		public bool HaveRestriction { get; set; }

		public ScheduleDayData(DateOnly date)
		{
			Date = date;
		}
	}
}