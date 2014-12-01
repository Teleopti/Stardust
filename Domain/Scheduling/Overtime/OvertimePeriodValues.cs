using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Domain.Scheduling.Overtime
{
	public class OvertimePeriodValues
	{
		private readonly IList<IOvertimePeriodValue> _periodValues;

		public OvertimePeriodValues()
		{
			_periodValues = new List<IOvertimePeriodValue>();
		}

		public void Add(IOvertimePeriodValue overtimePeriodValue1)
		{
			_periodValues.Add(overtimePeriodValue1);
		}

		public double TotalValue()
		{
			return _periodValues.Sum(overtimePeriodValue => overtimePeriodValue.Value);
		}
	}
}