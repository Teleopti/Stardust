using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.DataProvider
{
	
	public class AbsenceRequestProbabilityProviderNew : IAbsenceRequestProbabilityProvider
	{
		private readonly IBudgetDayRepository _budgetDayRepository;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IExtractBudgetGroupPeriods _extractBudgetGroupPeriods;
		private readonly INow _now;
		private readonly IScheduleProjectionReadOnlyRepository _scheduleProjectionReadOnlyRepository;

		readonly string[] texts = {UserTexts.Resources.Poor, UserTexts.Resources.Fair, UserTexts.Resources.Good};
		readonly string[] cssClass = {"red", "yellow", "green"};

		public AbsenceRequestProbabilityProviderNew(IBudgetDayRepository budgetDayRepository, ILoggedOnUser loggedOnUser, 
			IScenarioRepository scenarioRepository, IExtractBudgetGroupPeriods extractBudgetGroupPeriods, INow now,
			IScheduleProjectionReadOnlyRepository scheduleProjectionReadOnlyRepository)
		{
			_budgetDayRepository = budgetDayRepository;
			_loggedOnUser = loggedOnUser;
			_scenarioRepository = scenarioRepository;
			_extractBudgetGroupPeriods = extractBudgetGroupPeriods;
			_now = now;
			_scheduleProjectionReadOnlyRepository = scheduleProjectionReadOnlyRepository;
		}

		public List<Tuple<DateOnly, string, string>> GetAbsenceRequestProbabilityForPeriod(DateOnlyPeriod period)
		{
			var person = _loggedOnUser.CurrentUser();

			var allowanceList =
				from d in period.DayCollection()
				select new { Date = d, Time = TimeSpan.Zero, Heads = TimeSpan.Zero, Availability = false };

			var budgetGroupPeriods = _extractBudgetGroupPeriods.BudgetGroupsForPeriod(person, period);
			var defaultScenario = _scenarioRepository.LoadDefaultScenario();

			if (person.WorkflowControlSet != null && person.WorkflowControlSet.AbsenceRequestOpenPeriods != null)
			{

				var openPeriods = person.WorkflowControlSet.AbsenceRequestOpenPeriods;


				if (openPeriods.Count != 0)
				{
					allowanceList =
						from d in period.DayCollection()
						select new
							{
								Date = d,
								Time = TimeSpan.Zero,
								Heads = TimeSpan.Zero,
								Availability = false
							};

					foreach (var openPeriod in openPeriods)
					{
						
						var allowanceFromBudgetDays =
							from budgetGroupPeriod in budgetGroupPeriods
							from budgetDay in _budgetDayRepository.Find(defaultScenario, budgetGroupPeriod.Item2, budgetGroupPeriod.Item1)
							where openPeriod.GetPeriod(_now.LocalDateOnly()).Contains(budgetDay.Day)
							where openPeriod.OpenForRequestsPeriod.Contains(_now.LocalDateOnly())
							where openPeriod.AbsenceRequestProcess.GetType() != typeof (DenyAbsenceRequest)
							select
								new
									{
										Date = budgetDay.Day,
										Time = TimeSpan.FromHours(Math.Max(budgetDay.Allowance*budgetDay.FulltimeEquivalentHours, 0)),
										Heads = TimeSpan.FromHours(Math.Max(budgetDay.FulltimeEquivalentHours, 0)),
										Availability = false //(invalidOpenPeriods.Count <= 0 || !invalidOpenPeriods.Any(x => x.GetPeriod(_now.LocalDateOnly()).Contains(budgetDay.Day)))
									};
						allowanceList = allowanceList.Concat(allowanceFromBudgetDays);
					}
				}
			
			}

			
			//return 
			//	from p in allowanceList
			//	group p by p.Date into g
			//	orderby g.Key
			//	select new Tuple<DateOnly, TimeSpan, TimeSpan, bool>
			//		(g.Key, TimeSpan.FromTicks(g.Sum(p => p.Time.Ticks)), TimeSpan.FromTicks(g.Sum(p => p.Heads.Ticks)), g.First(o => o.Date == g.Key).Availability);
			//}
			return null;
		}

		public void FillAllowanceForDay(DateOnly dateOnly, IList<Tuple<DateOnly, int, int, bool>> allowanceList,
										IAbsenceRequestOpenPeriod openPeriod, IBudgetGroup budgetGroup, IScenario scenario)
		{
			if (openPeriod.StaffingThresholdValidator.GetType() == typeof (BudgetGroupAllowanceValidator))
				FillAllowanceBudgetGroupAllowance(dateOnly, allowanceList, openPeriod,budgetGroup,scenario);

			if (openPeriod.StaffingThresholdValidator.GetType() == typeof(BudgetGroupHeadCountValidator))
				FillAllowanceBudgetGroupAllowance(dateOnly, allowanceList, openPeriod,budgetGroup,scenario);
			
		}

		public void FillAllowanceBudgetGroupAllowance(DateOnly dateOnly, IList<Tuple<DateOnly, int, int, bool>> allowanceList,
										IAbsenceRequestOpenPeriod openPeriod, IBudgetGroup budgetGroup, IScenario scenario)
		{
			
		}

		public void FillAllowanceBudgetGroupHeadCount(DateOnly dateOnly, IList<Tuple<DateOnly, int, int, bool>> allowanceList,
										IAbsenceRequestOpenPeriod openPeriod, IBudgetGroup budgetGroup, IScenario scenario)
		{

		}
		//public List<Tuple<DateOnly, string, string>> GetAbsenceRequestProbabilityForPeriod(DateOnlyPeriod period)
		//{
		//	var absenceTimeCollection = _absenceTimeProvider.GetAbsenceTimeForPeriod(period);
		//	var allowanceCollection = _allowanceProvider.GetAllowanceForPeriod(period);

		//	var ret = new List<Tuple<DateOnly, string, string>>();

		//	foreach (var dateOnly in period.DayCollection())
		//	{

		//		var absenceTimeForDay = absenceTimeCollection == null
		//									? 0
		//									: absenceTimeCollection.First(a => a.Date == dateOnly).AbsenceTime;

		//		var fulltimeEquivalentForDay = allowanceCollection == null
		//										   ? 0
		//										   : allowanceCollection.First(a => a.Item1 == dateOnly).Item3.TotalMinutes;

		//		var allowanceForDay = allowanceCollection == null
		//								  ? 0
		//								  : allowanceCollection.First(a => a.Item1 == dateOnly).Item2.TotalMinutes;


		//		var percent = 0d;
		//		if (!Equals(allowanceForDay, .0))
		//			percent = 100*((allowanceForDay - absenceTimeForDay)/allowanceForDay);


		//		var index = 0;
		//		if (percent > 0 && (allowanceForDay - absenceTimeForDay) >= fulltimeEquivalentForDay)
		//			index = percent > 30 && (allowanceForDay - absenceTimeForDay) >= 2*fulltimeEquivalentForDay ? 2 : 1;

		//		ret.Add(new Tuple<DateOnly, string, string>(dateOnly, cssClass[index], texts[index]));
		//	}

		//	return ret;
		//}
	}
}