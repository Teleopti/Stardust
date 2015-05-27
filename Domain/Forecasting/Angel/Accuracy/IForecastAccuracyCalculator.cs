using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy
{
	public interface IForecastAccuracyCalculator
	{
		double Accuracy(IList<IForecastingTarget> forecastingForLastYear, ReadOnlyCollection<ITaskOwner> historicalDataForLastYear);
	}
}