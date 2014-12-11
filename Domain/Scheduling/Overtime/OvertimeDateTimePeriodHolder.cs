using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Overtime
{
	public interface IOvertimeDateTimePeriodHolder
	{
		IList<DateTimePeriod> DateTimePeriods { get; }
		void Add(DateTimePeriod dateTimePeriod);
	}

	public class OvertimeDateTimePeriodHolder : IOvertimeDateTimePeriodHolder
	{
		public IList<DateTimePeriod> DateTimePeriods { get; private set; }

		public OvertimeDateTimePeriodHolder()
		{
			DateTimePeriods = new List<DateTimePeriod>();
		}

		public void Add(DateTimePeriod dateTimePeriod)
		{
			DateTimePeriods.Add(dateTimePeriod);
		}
	}
}
