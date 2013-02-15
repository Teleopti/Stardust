using System;
using System.Collections.Generic;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.WinCode.Forecasting.QuickForecastPages
{
    public class QuickForecastModel
    {
		public QuickForecastModel()
		{
			SelectedWorkloads = new List<Guid>();
			SmoothingStyle = 5;
			StatisticPeriod = new DateOnlyPeriodDto
				{
					StartDate = new DateOnlyDto {DateTime = DateTime.Today.AddYears(-2)},
					EndDate = new DateOnlyDto {DateTime = DateTime.Today.AddDays(-1)}
				};
			var start = DateTime.Today.AddMonths(1);
			start = new DateTime(start.Year,start.Month,1);
			var end = start.AddMonths(3).AddDays(-1);
			TargetPeriod = new DateOnlyPeriodDto
			{
				StartDate = new DateOnlyDto { DateTime = start },
				EndDate = new DateOnlyDto { DateTime = end }
			};

			TemplatePeriod = new DateOnlyPeriodDto
			{
				StartDate = new DateOnlyDto { DateTime = DateTime.Today.AddMonths(-3).AddDays(-1) },
				EndDate = new DateOnlyDto { DateTime = DateTime.Today.AddDays(-1) }
			};
		}

		public List<Guid> SelectedWorkloads { get; private set; }
		
		public Guid ScenarioId { get; set; }

		public DateOnlyPeriodDto StatisticPeriod { get; set; }

		public DateOnlyPeriodDto TargetPeriod { get; set; }

		public DateOnlyPeriodDto TemplatePeriod { get; set; }

		public int SmoothingStyle { get; set; }
    }

}
