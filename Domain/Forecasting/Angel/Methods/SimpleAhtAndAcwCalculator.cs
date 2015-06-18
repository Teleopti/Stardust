using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Methods
{
	public class SimpleAhtAndAcwCalculator : IAhtAndAcwCalculator
	{
		public AhtAndAcw Recent3MonthsAverage(ITaskOwnerPeriod historicalData)
		{
			var recent3Months = historicalData.EndDate.Date.AddMonths(-3);
			var recent3MonthsData = new TaskOwnerPeriod(DateOnly.MinValue,
				historicalData.TaskOwnerDayCollection.Where(x => x.CurrentDate > new DateOnly(recent3Months)),
				TaskOwnerPeriodType.Other);
			return new AhtAndAcw
			{
				Aht = recent3MonthsData.TotalStatisticAverageTaskTime,
				Acw = recent3MonthsData.TotalStatisticAverageAfterTaskTime
			};
		}
	}
}