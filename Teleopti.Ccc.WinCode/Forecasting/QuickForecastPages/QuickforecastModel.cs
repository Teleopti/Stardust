using System;
using System.Collections.Generic;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Forecasting.QuickForecastPages
{
    public class QuickForecastModel
    {
		public QuickForecastModel()
		{
			SelectedWorkloads = new List<Guid>();
			StatisticPeriod = new DateOnlyPeriodDto
				{
					StartDate = new DateOnlyDto {DateTime = DateTime.Today.AddYears(-2)},
					EndDate = new DateOnlyDto {DateTime = DateTime.Today.AddDays(-1)}
				};
		}

		public List<Guid> SelectedWorkloads { get; private set; }
		
		public Guid ScenarioId { get; set; }

		public DateOnlyPeriodDto StatisticPeriod { get; set; }

		public DateOnlyPeriodDto TargetPeriod { get; set; }

		public bool UpdateStandardTemplates { get; set; }
    }

}
