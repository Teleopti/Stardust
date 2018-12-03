using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.DayOffScheduling
{
	public interface IScheduleDayData
	{
		DateOnly DateOnly { get; }
		bool IsDayOff { get; set; }
		bool IsContractDayOff { get; set; }
		bool IsScheduled { get; set; }
		bool HaveRestriction { get; set; }
	}

	public class ScheduleDayData : IScheduleDayData
	{
		private bool _isDayOff;
		public DateOnly DateOnly { get; private set; }

		public bool IsDayOff
		{
			get { return _isDayOff; }
			set
			{
				_isDayOff = value;
				if (value)
					IsScheduled = true;
			}
		}

		public bool IsContractDayOff { get; set; }
		public bool IsScheduled { get; set; }
		public bool HaveRestriction { get; set; }

		public ScheduleDayData(DateOnly dateOnly)
		{
			DateOnly = dateOnly;
		}
	}
}