using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public class PersonWorkDay
	{
		public PersonWorkDay(DateOnly date, bool workDay = false)
			: this(date,new Lazy<TimeSpan>(()=>TimeSpan.Zero), WorkTimeSource.FromSchedulePeriod, new Percent(1), workDay)
		{
		}

		public PersonWorkDay(DateOnly date, Lazy<TimeSpan> averageWorkTime, WorkTimeSource workTimeSource, Percent partTimePercentage, bool isWorkDay = false)
		{
			IsWorkDay = isWorkDay;
			AverageWorkTime = averageWorkTime;
			WorkTimeSource = workTimeSource;
			Date = date;
			PartTimePercentage = partTimePercentage;
		}

		public DateOnly Date { get; private set; }
		public Lazy<TimeSpan> AverageWorkTime { get; private set; }
		public WorkTimeSource WorkTimeSource { get; private set; }
		public Percent PartTimePercentage { get; private set; }
		public bool IsWorkDay { get; private set; }
	}
}