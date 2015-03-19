using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public interface IQuickForecastCreator
	{
		void CreateForecastForWorkloads(DateOnlyPeriod futurePeriod, Guid[] workloadIds);
	}
}