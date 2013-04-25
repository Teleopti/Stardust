using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public class AbsenceTimeProvider : IAbsenceTimeProvider
	{
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IScheduleProjectionReadOnlyRepository _scheduleProjectionReadOnlyRepository;

		public AbsenceTimeProvider(ILoggedOnUser loggedOnUser, IScenarioRepository scenarioRepository, IScheduleProjectionReadOnlyRepository scheduleProjectionReadOnlyRepository)
		{
			_loggedOnUser = loggedOnUser;
			_scenarioRepository = scenarioRepository;
			_scheduleProjectionReadOnlyRepository = scheduleProjectionReadOnlyRepository;
		}

		private void AddTime(IEnumerable<AbsenceAgents> target, DateOnlyPeriod period, IBudgetGroup budgetGroup, IScenario scenario)
		{
			if (budgetGroup != null)
			{
				var absenceTime = _scheduleProjectionReadOnlyRepository.AbsenceTimePerBudgetGroup(period, budgetGroup, scenario);

				if (absenceTime != null)
				{
					foreach (var payloadWorkTime in absenceTime)
					{
						target.First(a => a.Date == payloadWorkTime.BelongsToDate).AbsenceTime = TimeSpan.FromTicks(payloadWorkTime.TotalContractTime).TotalMinutes;
					}
				}
			}
		}
	
		public IEnumerable<IAbsenceAgents> GetAbsenceTimeForPeriod(DateOnlyPeriod period)
		{
			var currentDate = period.StartDate;

			var person = _loggedOnUser.CurrentUser();
			var defaultScenario = _scenarioRepository.LoadDefaultScenario();
			List<AbsenceAgents> absenceDays = period.DayCollection().Select(d => new AbsenceAgents() { Date = d, AbsenceTime = 0 }).ToList();

			var affectedPersonPeriods = person.PersonPeriods(period);
			foreach (var affectedPersonPeriod in affectedPersonPeriods)
			{
				var endDate = affectedPersonPeriod.EndDate() < period.EndDate ? affectedPersonPeriod.EndDate() : period.EndDate;
				if (currentDate <= endDate)
				{
					var thePeriod = new DateOnlyPeriod(currentDate, endDate);
					var budgetGroup = affectedPersonPeriod.BudgetGroup;
					AddTime(absenceDays,thePeriod,budgetGroup,defaultScenario);
					currentDate = thePeriod.EndDate.AddDays(1);
				}

			}
			return absenceDays;
		}

		
	}
}