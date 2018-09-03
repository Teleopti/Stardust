using System.Collections.Generic;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Angel.Historical;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting.Angel.Accuracy
{
	public class FakeHistoricalDataWithCounter : IHistoricalData
	{
		public int FetchCount = 0;

		public TaskOwnerPeriod Fetch(IWorkload workload, DateOnlyPeriod period)
		{
			FetchCount++;
			return new TaskOwnerPeriod(DateOnly.MinValue, new List<ITaskOwner>(), TaskOwnerPeriodType.Other);
		}
	}
}