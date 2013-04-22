using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
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

		public IEnumerable<IAbsenceAgents> GetAbsenceTimeForPeriod(DateOnlyPeriod period)
		{
			var person = _loggedOnUser.CurrentUser();
			
			var personPeriod =
				person.PersonPeriodCollection.OrderBy(p => p.StartDate).LastOrDefault(p => p.StartDate < period.StartDate);
			if (personPeriod == null || personPeriod.BudgetGroup == null)
			{
				return period.DayCollection().Select(d => new AbsenceAgents() {Date = d, AbsenceTime = 0});
			}
			
			var defaultScenario = _scenarioRepository.LoadDefaultScenario();
			var absenceTime = _scheduleProjectionReadOnlyRepository.AbsenceTimePerBudgetGroup(period, personPeriod.BudgetGroup, defaultScenario);
			return absenceTime.Select(day => new AbsenceAgents() { Date = day.BelongsToDate, AbsenceTime = day.TotalContractTime});
		}
	}
}