using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.Forecasting.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Core;

namespace Teleopti.Ccc.Web.Areas.Forecasting.Core
{
	public class IntradayPatternViewModelFactory : IIntradayPatternViewModelFactory
	{
		private readonly IIntradayForecaster _intradayForecaster;
		private readonly IWorkloadRepository _workloadRepository;
		private readonly IHistoricalPeriodProvider _historicalPeriodProvider;

		public IntradayPatternViewModelFactory(IIntradayForecaster intradayForecaster, IWorkloadRepository workloadRepository, IHistoricalPeriodProvider historicalPeriodProvider)
		{
			_intradayForecaster = intradayForecaster;
			_workloadRepository = workloadRepository;
			_historicalPeriodProvider = historicalPeriodProvider;
		}

		public IntradayPatternViewModel Create(IntradayPatternInput input)
		{
			var workload = _workloadRepository.Get(input.WorkloadId);
			var availableIntradayTemplatePeriod = _historicalPeriodProvider.AvailableIntradayTemplatePeriod(workload);
			var intradayPatternViewModel = new IntradayPatternViewModel
			{
				WorkloadId = workload.Id.Value,
				WeekDays = new IntradayPatternDayViewModel[] {}
			};
			if (!availableIntradayTemplatePeriod.HasValue)
				return intradayPatternViewModel;
			var pattern = _intradayForecaster.CalculatePattern(workload, availableIntradayTemplatePeriod.Value);
			var weekdays = new List<IntradayPatternDayViewModel>();
			var timeZone = workload.Skill.TimeZone;
			var culture = new System.Globalization.CultureInfo("en-US");

			foreach (DayOfWeek day in Enum.GetValues(typeof (DayOfWeek)))
			{
				var intradayPatternDayViewModel = new IntradayPatternDayViewModel
				{
					DayOfWeek = day,
					DayOfWeekDisplayName = culture.DateTimeFormat.GetDayName(day)
				};
				var tasksList = new List<double>();
				var periodList = new List<string>();
				foreach (var templateTaskPeriod in pattern[day])
				{
					var localPeriod=templateTaskPeriod.Period.TimePeriod(timeZone);
					tasksList.Add(Math.Round(templateTaskPeriod.Tasks, 3));
					periodList.Add(localPeriod.ToShortTimeString());
				}

				intradayPatternDayViewModel.Tasks = tasksList;
				intradayPatternDayViewModel.Periods = periodList;
				weekdays.Add(intradayPatternDayViewModel);
			}
			intradayPatternViewModel.WeekDays = weekdays;
			return intradayPatternViewModel;
		}
	}
}