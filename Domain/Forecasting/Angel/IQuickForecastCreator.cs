using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public class ForecastWorkloadInput
	{
		public Guid WorkloadId { get; set; }
		public ForecastMethodType ForecastMethodId { get; set; }
	}

	public interface IQuickForecastCreator
	{
		void CreateForecastForWorkloads(DateOnlyPeriod futurePeriod, ForecastWorkloadInput[] workloads);
		void CreateForecastForAll(DateOnlyPeriod futurePeriod);
	}
}