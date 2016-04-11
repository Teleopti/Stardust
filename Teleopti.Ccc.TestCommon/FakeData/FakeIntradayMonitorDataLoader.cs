using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeIntradayMonitorDataLoader : IIntradayMonitorDataLoader
	{
		public IList<IncomingIntervalModel> Load(IList<Guid> skillList, TimeZoneInfo timeZone, DateOnly today)
		{
			return Intervals;
		}
		
		private IList<IncomingIntervalModel> Intervals { get; set; }

		public void AddInterval(IncomingIntervalModel interval)
		{
			if (Intervals == null)
				Intervals = new List<IncomingIntervalModel>();
			Intervals.Add(interval);
		}
	}
}