using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
	public class RequestAllowanceProvider : IRequestAllowanceProvider
	{
		private readonly IScheduleProjectionReadOnlyPersister _scheduleProjectionReadOnlyPersister;
		private readonly ICurrentScenario _scenarioRepository;
		private readonly IBudgetDayRepository _budgetDayRepository;

		public RequestAllowanceProvider(IScheduleProjectionReadOnlyPersister scheduleProjectionReadOnlyPersister, ICurrentScenario scenarioRepository, IBudgetDayRepository budgetDayRepository)
		{
			_scheduleProjectionReadOnlyPersister = scheduleProjectionReadOnlyPersister;
			_scenarioRepository = scenarioRepository;
			_budgetDayRepository = budgetDayRepository;
		}

		public IList<IBudgetAbsenceAllowanceDetail> GetBudgetAbsenceAllowanceDetails(DateOnlyPeriod period, IBudgetGroup selectedBudgetGroup, IEnumerable<IAbsence> absencesInBudgetGroup)
		{
			var budgetAbsenceAllowanceDetails = new List<IBudgetAbsenceAllowanceDetail>();
			var usedAbsences = loadUsedAbsences(selectedBudgetGroup, period).ToDictionary(k => new Tuple<Guid,DateTime>(k.PayloadId,k.BelongsToDate));
			var budgetDays = loadBudgetDays(selectedBudgetGroup, period);
			var fteCollection = budgetDays.Select(b => b.FulltimeEquivalentHours).ToArray();
			var shrinkedAllowanceCollection = budgetDays.Select(b => b.ShrinkedAllowance).ToArray();
			var fullAllowanceCollection = budgetDays.Select(b => b.FullAllowance).ToArray();
			for (var i = 0; i < period.DayCount(); i++)
			{
				var currentDate = period.StartDate.AddDays(i);
				var absenceDict = new Dictionary<IAbsence, double>();
				foreach (var absence in absencesInBudgetGroup)
				{
					PayloadWorkTime payloadWorkTime;
					if (usedAbsences.TryGetValue(new Tuple<Guid, DateTime>(absence.Id.GetValueOrDefault(), currentDate.Date),
						out payloadWorkTime))
					{
						var usedFTEs = fteCollection[i] != 0
							? TimeSpan.FromTicks(payloadWorkTime.TotalContractTime).TotalMinutes * 1d / TimeDefinition.MinutesPerHour /
							  fteCollection[i]
							: 0d;
						absenceDict.Add(absence, usedFTEs);
					}
					else
					{
						absenceDict.Add(absence, 0);
					}
				}
				var shrinkedAllowance = shrinkedAllowanceCollection[i];
				var fullAllowance = fullAllowanceCollection[i];
				var usedTotalAbsences = absenceDict.Sum(a => a.Value);
				var absoluteDiff = shrinkedAllowance - usedTotalAbsences;
				var relativeDiff = new Percent(usedTotalAbsences / shrinkedAllowance);
				var headCounts = getHeadCounts(selectedBudgetGroup, currentDate);

				var detailModel = new BudgetAbsenceAllowanceDetail
				{
					Date = currentDate,
					ShrinkedAllowance = shrinkedAllowance,
					FullAllowance = fullAllowance,
					UsedAbsencesDictionary = absenceDict,
					UsedTotalAbsences = usedTotalAbsences,
					AbsoluteDifference = absoluteDiff,
					RelativeDifference = relativeDiff,
					TotalHeadCounts = headCounts
				};
				budgetAbsenceAllowanceDetails.Add(detailModel);
			}
			return budgetAbsenceAllowanceDetails;
		}

		private List<PayloadWorkTime> loadUsedAbsences(IBudgetGroup selectedBudgetGroup, DateOnlyPeriod period)
		{
			if (isBudgetGroupNullOrEmpty(selectedBudgetGroup)) return new List<PayloadWorkTime>();
			return 
				_scheduleProjectionReadOnlyPersister.AbsenceTimePerBudgetGroup(period, selectedBudgetGroup,
					_scenarioRepository.Current()).ToList();
		}

		private List<IBudgetDay> loadBudgetDays(IBudgetGroup selectedBudgetGroup, DateOnlyPeriod period)
		{
			var budgetDays = new List<IBudgetDay>();
			if (!isBudgetGroupNullOrEmpty(selectedBudgetGroup))
				budgetDays = _budgetDayRepository.Find(_scenarioRepository.Current(), selectedBudgetGroup, period).ToList();
			selectedBudgetGroup = selectedBudgetGroup ?? new BudgetGroup { Name = UserTexts.Resources.Empty };
			budgetDays = addMissingDays(selectedBudgetGroup, budgetDays, period).ToList();
			return budgetDays;
		}

		private IEnumerable<IBudgetDay> addMissingDays(IBudgetGroup selectedBudgetGroup, IEnumerable<IBudgetDay> existingBudgetDays, DateOnlyPeriod period)
		{
			var dayCollection = period.DayCollection();
			var allBudgetDaysForPeriod = new List<IBudgetDay>(dayCollection.Count);

			var existingBudgetDaysForDay = existingBudgetDays.ToDictionary(d => d.Day);
			foreach (var date in dayCollection)
			{
				if (!existingBudgetDaysForDay.TryGetValue(date, out var budgetDay))
				{
					budgetDay = new BudgetDay(selectedBudgetGroup, _scenarioRepository.Current(), date);
					initiateBudgetDayWithDefaultValue(budgetDay);
				}
				allBudgetDaysForPeriod.Add(budgetDay);
			}
			return allBudgetDaysForPeriod;
		}

		private static void initiateBudgetDayWithDefaultValue(IBudgetDay budgetDay)
		{
			budgetDay.AbsenceThreshold = new Percent(1.0);
		}

		private int getHeadCounts(IBudgetGroup selectedBudgetGroup, DateOnly currentDate)
		{
			if (isBudgetGroupNullOrEmpty(selectedBudgetGroup))
				return 0;
			return _scheduleProjectionReadOnlyPersister.GetNumberOfAbsencesPerDayAndBudgetGroup(selectedBudgetGroup.Id.GetValueOrDefault(), currentDate);
		}

		private bool isBudgetGroupNullOrEmpty(IBudgetGroup selectedBudgetGroup)
		{
			return selectedBudgetGroup?.Id == null;
		}
	}
}