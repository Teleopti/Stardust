using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public class AbsenceTimeProvider : IAbsenceTimeProvider
	{
		private readonly ILoggedOnUser _loggedOnUser;
		public readonly IBudgetDayRepository _budgetDayRepository;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IUserTimeZone _userTimeZone;

		public AbsenceTimeProvider(IBudgetDayRepository budgetDayRepository, IUserTimeZone userTimeZone, ILoggedOnUser loggedOnUser, IScenarioRepository scenarioRepository)
		{
			_budgetDayRepository = budgetDayRepository;
			_userTimeZone = userTimeZone;
			_loggedOnUser = loggedOnUser;
			_scenarioRepository = scenarioRepository;
		}

		public IEnumerable<IAbsenceTimeDay> GetAbsenceTimeForPeriod(DateOnlyPeriod period)
		{
			throw new NotImplementedException();
		}
	}
}