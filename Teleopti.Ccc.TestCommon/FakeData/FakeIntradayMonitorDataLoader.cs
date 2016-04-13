using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeIntradayMonitorDataLoader : IIntradayMonitorDataLoader
	{
		private IList<IncomingIntervalModel> _intervals;

		public IList<IncomingIntervalModel> Load(IList<Guid> skillList, TimeZoneInfo timeZone, DateOnly today)
		{
			return intervals;
		}

		private IList<IncomingIntervalModel> intervals
		{
			get { return _intervals ?? (_intervals = new List<IncomingIntervalModel>()); }
			set { _intervals = value; }
		}

		public void AddInterval(IncomingIntervalModel interval)
		{
			intervals.Add(interval);
		}
	}
}