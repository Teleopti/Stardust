using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.Domain.Forecasting.Models;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
	public class IntradayPatternViewModelFactory : IIntradayPatternViewModelFactory
	{
		private readonly IIntradayForecaster _intradayForecaster;
		private readonly IWorkloadRepository _workloadRepository;
		private readonly IHistoricalPeriodProvider _historicalPeriodProvider;
		private readonly IUserCulture _userCulture;

		public IntradayPatternViewModelFactory(IIntradayForecaster intradayForecaster, IWorkloadRepository workloadRepository,
			IHistoricalPeriodProvider historicalPeriodProvider, IUserCulture userCulture)
		{
			_intradayForecaster = intradayForecaster;
			_workloadRepository = workloadRepository;
			_historicalPeriodProvider = historicalPeriodProvider;
			_userCulture = userCulture;
		}

		public IntradayPatternViewModel Create(Guid workloadId)
		{
			var workload = _workloadRepository.Get(workloadId);
			var availableIntradayTemplatePeriod = _historicalPeriodProvider.AvailableIntradayTemplatePeriod(workload);
			var intradayPatternViewModel = new IntradayPatternViewModel
			{
				WorkloadId = workload.Id.Value,
				WorkloadName = workload.Name,
				WeekDays = new IntradayPatternDayViewModel[] {}
			};
			if (!availableIntradayTemplatePeriod.HasValue)
				return intradayPatternViewModel;
			var pattern = _intradayForecaster.CalculatePattern(workload, availableIntradayTemplatePeriod.Value);
			var weekdays = new List<IntradayPatternDayViewModel>();
			var timeZone = workload.Skill.TimeZone;

			var firstDayOfWeek = _userCulture.GetCulture().DateTimeFormat.FirstDayOfWeek;
			var dayOfWeekList = new List<DayOfWeek> {firstDayOfWeek};

			for (int i = 1; i < 7; i++)
			{
				dayOfWeekList.Add((DayOfWeek) Enum.ToObject(typeof (DayOfWeek), ((int) firstDayOfWeek + i)%7));
			}

			foreach (var day in dayOfWeekList)
			{
				var intradayPatternDayViewModel = new IntradayPatternDayViewModel
				{
					DayOfWeek = day,
					DayOfWeekDisplayName = _userCulture.GetCulture().DateTimeFormat.GetDayName(day)
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