using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider
{
	public class ShiftTradePersonScheduleProvider : IShiftTradePersonScheduleProvider
	{
		private readonly ICurrentScenario _currentScenario;
		private readonly IScheduleRepository _scheduleRepository;

		public ShiftTradePersonScheduleProvider(ICurrentScenario currentScenario, IScheduleRepository scheduleRepository)
		{
			_currentScenario = currentScenario;
			_scheduleRepository = scheduleRepository;
		}

		public IEnumerable<IScheduleDay> GetScheduleForPersons(DateOnly date, IEnumerable<IPerson> persons)
		{
			var defaultScenario = _currentScenario.Current();

			var dictionary = _scheduleRepository.FindSchedulesForPersonsOnlyInGivenPeriod(
				persons,
				new ScheduleDictionaryLoadOptions(false, false),
				new DateOnlyPeriod(date, date),
				defaultScenario);

			return dictionary.SchedulesForDay(date);
		}
	}
}