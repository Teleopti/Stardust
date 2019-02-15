using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;


namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeLatestStatisticsIntervalIdLoader : ILatestStatisticsIntervalIdLoader
	{
		private int? _intervalId;
		private DateOnly? _intervalDate; 

		public void Has(int? intervalId)
		{
			_intervalId = intervalId;
		}

		public void Has(DateOnly intervalDate)
		{
			_intervalDate = intervalDate;
		}

		public int? Load(Guid[] skillIdList, DateOnly now)
		{
			if (_intervalDate != null && now != _intervalDate)
			{
				return null;
			}
			return _intervalId;
		}
	}
}