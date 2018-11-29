using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public interface ILoadSchedulesForRequestWithoutResourceCalculation
	{
		void Execute(IScenario scenario, DateTimePeriod period, IEnumerable<IPerson> requestedPersons, ISchedulingResultStateHolder schedulingResultStateHolder);
	}

	public class LoadSchedulesForRequestWithoutResourceCalculation : ILoadSchedulesForRequestWithoutResourceCalculation
	{
		private readonly IPersonAbsenceAccountRepository _personAbsenceAccountRepository;
		private readonly IScheduleStorage _scheduleStorage;

		public LoadSchedulesForRequestWithoutResourceCalculation(
			IPersonAbsenceAccountRepository personAbsenceAccountRepository, IScheduleStorage scheduleStorage)
		{
			_personAbsenceAccountRepository = personAbsenceAccountRepository;
			_scheduleStorage = scheduleStorage;
		}

		public void Execute(IScenario scenario, DateTimePeriod period, IEnumerable<IPerson> requestedPersons,
			ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			schedulingResultStateHolder.LoadedAgents = requestedPersons.ToList();

			var personsProvider = schedulingResultStateHolder.LoadedAgents;

			var scheduleDictionaryLoadOptions = new ScheduleDictionaryLoadOptions(false, false);

			schedulingResultStateHolder.Schedules =
				_scheduleStorage.FindSchedulesForPersons(
					scenario,
					personsProvider,
					scheduleDictionaryLoadOptions,
					period,
					requestedPersons, true);

			schedulingResultStateHolder.AllPersonAccounts = _personAbsenceAccountRepository.FindByUsers(requestedPersons);
			schedulingResultStateHolder.SkillDays = new Dictionary<ISkill, IEnumerable<ISkillDay>>();
		}
	}
}