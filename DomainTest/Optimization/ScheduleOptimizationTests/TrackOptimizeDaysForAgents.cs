using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.ScheduleOptimizationTests
{
	public class TrackOptimizeDaysForAgents : IIntradayOptimizeOneDayCallback
	{
		private readonly IList<trackedDayAndAgent> _tracked = new List<trackedDayAndAgent>();

		public void Optimizing(IPerson person, DateOnly dateOnly)
		{
			_tracked.Add(new trackedDayAndAgent {Agent=person, Date = dateOnly});
		}

		public int NumberOfOptimizationsFor(DateOnly date)
		{
			return _tracked.Count(x => x.Date.Equals(date));
		}

		public int NumberOfOptimizations()
		{
			return _tracked.Count;
		}

		private class trackedDayAndAgent
		{
			public IPerson Agent { get; set; }
			public DateOnly Date { get; set; }
		}
	}
}