using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.Forecasting.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeForecastResultViewModelFactory : IForecastResultViewModelFactory
	{
		public WorkloadForecastResultViewModel Create(Guid workloadId, DateOnlyPeriod dateOnlyPeriod, IScenario scenario)
		{
			throw new NotImplementedException();
		}
	}
}