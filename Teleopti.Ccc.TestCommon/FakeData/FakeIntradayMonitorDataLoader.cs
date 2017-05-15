using System;
using System.Collections.Generic;
using NHibernate.Util;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeIntradayMonitorDataLoader : IIntradayMonitorDataLoader
	{
		private IList<IncomingIntervalModel> _intervals;
		private IList<Guid> _skills;
		public bool ShouldCompareDate { get; set; }

		public IList<IncomingIntervalModel> Load(IList<Guid> skillList, TimeZoneInfo timeZone, DateOnly today)
		{
			if (ShouldCompareDate && _intervals.Any() && new DateOnly(_intervals[0].IntervalDate) != today)
				return new List<IncomingIntervalModel>();

			Skills = skillList;
			return intervals;
		}

		public IList<Guid> Skills
		{
			get
			{
				return _skills ?? new List<Guid>();
			}
			set { _skills = value; }
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