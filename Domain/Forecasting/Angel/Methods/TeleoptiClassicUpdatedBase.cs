using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Methods
{
	public abstract class TeleoptiClassicUpdatedBase : TeleoptiClassicBase
	{
		private readonly IAhtAndAcwCalculator _ahtAndAcwCalculator;

		protected TeleoptiClassicUpdatedBase(IIndexVolumes indexVolumes, IAhtAndAcwCalculator ahtAndAcwCalculator)
			: base(indexVolumes)
		{
			_ahtAndAcwCalculator = ahtAndAcwCalculator;
		}

		public override IList<IForecastingTarget> Forecast(ITaskOwnerPeriod historicalData, DateOnlyPeriod futurePeriod)
		{
			var dateAndTaskList = ForecastNumberOfTasks(historicalData, futurePeriod);
			var ahtAndAcw = _ahtAndAcwCalculator.Recent3MonthsAverage(historicalData);

			var targetForecastingList =
				dateAndTaskList.Select(dateAndTask => new ForecastingTarget(dateAndTask.Date, new OpenForWork(true, true))
				{
					Tasks = dateAndTask.Tasks,
					AverageTaskTime = ahtAndAcw.Aht,
					AverageAfterTaskTime = ahtAndAcw.Acw
				}).ToList<IForecastingTarget>();
			return targetForecastingList;
		}
	}
}