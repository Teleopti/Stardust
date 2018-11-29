using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation.IntraIntervalAnalyze
{
	public interface ISkillActivityCountCollector
	{
		IList<int> Collect(IEnumerable<DateTimePeriod> periods, DateTimePeriod period);
	}

	public class SkillActivityCountCollector : ISkillActivityCountCollector
	{
		private readonly ISkillActivityCounter _counter;

		public SkillActivityCountCollector(ISkillActivityCounter counter)
		{
			_counter = counter;
		}

		public IList<int> Collect(IEnumerable<DateTimePeriod> periods, DateTimePeriod period)
		{
			var result = _counter.Count(periods, period);
			return result;
		}
	}
}
