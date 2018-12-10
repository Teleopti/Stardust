using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday.Domain;
using Teleopti.Ccc.Domain.Repositories;


namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeIntradayMonitorDataLoader : IIntradayMonitorDataLoader
	{
		private IList<IncomingIntervalModel> _intervals;
		private IList<Guid> _skills;
		public bool ShouldCompareDate { get; set; }

		public IList<IncomingIntervalModel> Load(IList<Guid> skillList, TimeZoneInfo timeZone, DateOnly today)
		{
			Skills = skillList;
			return intervals.Where(x => x.IntervalDate.Date == today.Date).ToList();
		}

		public IList<Guid> Skills
		{
			get => _skills ?? new List<Guid>();
			set => _skills = value;
		}

		private IList<IncomingIntervalModel> intervals => _intervals ?? (_intervals = new List<IncomingIntervalModel>());

		public void AddInterval(IncomingIntervalModel interval)
		{
			intervals.Add(interval);
		}
	}
}