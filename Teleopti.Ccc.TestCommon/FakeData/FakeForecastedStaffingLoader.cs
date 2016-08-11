using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Intraday;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeForecastedStaffingLoader : IForecastedStaffingLoader
	{
		private IList<StaffingIntervalModel> _intervals;

		private IList<StaffingIntervalModel> intervals
		{
			get { return _intervals ?? (_intervals = new List<StaffingIntervalModel>()); }
			set { _intervals = value; }
		}

		public void AddInterval(StaffingIntervalModel interval)
		{
			intervals.Add(interval);
		}

		public IList<StaffingIntervalModel> Load(Guid[] skillIdList)
		{
			return intervals;
		}
	}
}