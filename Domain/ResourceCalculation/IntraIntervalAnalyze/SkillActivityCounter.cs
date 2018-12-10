using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation.IntraIntervalAnalyze
{
	public interface ISkillActivityCounter
	{
		IList<int> Count(IEnumerable<DateTimePeriod> periods, DateTimePeriod period);
	}

	public class SkillActivityCounter : ISkillActivityCounter
	{
		private const int sampleInterval = 5;
		public IList<int> Count(IEnumerable<DateTimePeriod> periods, DateTimePeriod period)
		{
			var startMinute = period.StartDateTime.AddMinutes(sampleInterval -1);
			var result = new List<int>();

			do
			{
				var count = 0;
				foreach (var dateTimePeriod in periods)
				{
					if (dateTimePeriod.Contains(startMinute))
					{
						count++;
					}
				}
				result.Add(count);

				startMinute = startMinute.AddMinutes(sampleInterval);

			}
			while (startMinute < period.EndDateTime);

			return result;
		}
	}
}
