using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class IntradayDatesToSkip
	{
		private readonly IIntradayOptimizerLimiter _intradayOptimizerLimiter;
		private readonly int _numberOfAgentsInGroup;
		private readonly IList<DateOnly> _skipDates;
		private readonly IDictionary<DateOnly, optimizeCounter> _optimizedPerDay;

		public IntradayDatesToSkip(IIntradayOptimizerLimiter intradayOptimizerLimiter, int numberOfAgentsInGroup)
		{
			_intradayOptimizerLimiter = intradayOptimizerLimiter;
			_numberOfAgentsInGroup = numberOfAgentsInGroup;
			_skipDates = new List<DateOnly>();
			_optimizedPerDay = new Dictionary<DateOnly, optimizeCounter>();
		}

		public IEnumerable<DateOnly> SkipDates { get { return _skipDates; } }

		public void DayWasOptimized(DateOnly dateOnly)
		{
			optimizeCounter optimizeCounter;
			if (!_optimizedPerDay.TryGetValue(dateOnly, out optimizeCounter))
			{
				optimizeCounter = new optimizeCounter();
				_optimizedPerDay[dateOnly] = optimizeCounter;
			}
			optimizeCounter.NumberOfOptimizations++;
			if (_intradayOptimizerLimiter.CanJumpOutEarly(_numberOfAgentsInGroup, optimizeCounter.NumberOfOptimizations))
			{
				_skipDates.Add(dateOnly);
			}
		}
		private class optimizeCounter
		{
			public int NumberOfOptimizations { get; set; }
		}
	}
}