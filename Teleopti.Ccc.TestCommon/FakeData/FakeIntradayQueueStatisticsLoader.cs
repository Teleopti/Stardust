using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeIntradayQueueStatisticsLoader : IIntradayQueueStatisticsLoader
	{
		private int? _actualworkloadSeconds;

		public int? LoadActualWorkloadInSeconds(IList<Guid> skillList, TimeZoneInfo timeZone, DateOnly today)
		{
			return (int?)_actualworkloadSeconds;
		}

		public void Has(int? actualworkloadSeconds)
		{
			_actualworkloadSeconds = actualworkloadSeconds;
		}
	}
}