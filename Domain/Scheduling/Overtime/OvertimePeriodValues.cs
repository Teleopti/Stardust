using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Domain.Scheduling.Overtime
{
	public interface IOvertimePeriodValues
	{
		void Add(IOvertimePeriodValue overtimePeriodValue1);
		IList<IOvertimePeriodValue> PeriodValues { get; }
		double TotalValue();
	}

	public class OvertimePeriodValues : IOvertimePeriodValues
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

		public IList<IOvertimePeriodValue> PeriodValues
		{
			get { return _periodValues; }
		}

		public double TotalValue()
		{
			return _periodValues.Sum(overtimePeriodValue => overtimePeriodValue.Value);
		}
	}
}