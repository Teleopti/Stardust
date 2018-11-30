using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public interface ISchedulerStateHolder
	{
		bool ConsiderShortBreaks { get; set; }

		ISchedulingResultStateHolder SchedulingResultState { get; }

		IDateOnlyPeriodAsDateTimePeriod RequestedPeriod { get; set; }

		IScenario RequestedScenario { get; }

		IList<IPerson> ChoosenAgents { get; }

		void ResetFilteredPersons();

		void ResetFilteredPersonsOvertimeAvailability();

		void ResetFilteredPersonsHourlyAvailability();

		void LoadSchedules(IFindSchedulesForPersons findSchedulesForPersons, IEnumerable<IPerson> personsInOrganization, ScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions, DateTimePeriod period);

		IScheduleDictionary Schedules { get; }

		IDictionary<Guid, IPerson> FilteredCombinedAgentsDictionary { get; }


		IDictionary<Guid, IPerson> FilteredAgentsDictionary { get; }

		TimeZoneInfo TimeZoneInfo { get; set; }

		ICommonStateHolder CommonStateHolder { get; }

		void LoadCommonState(IUnitOfWork unitOfWork, IRepositoryFactory repositoryFactory);

		CommonNameDescriptionSetting CommonNameDescription { get; }

		void SetRequestedScenario(IScenario scenario);

		void FilterPersonsOvertimeAvailability(IEnumerable<IPerson> selectedPersons);

		void FilterPersonsHourlyAvailability(IList<IPerson> selectedPersons);

		void FilterPersons(HashSet<Guid> selectedGuids);

		void ClearDaysToRecalculate();

		void LoadSettings(IUnitOfWork unitOfWork, IRepositoryFactory repositoryFactory);

		void MarkDateToBeRecalculated(DateOnly dateToRecalculate);

		IEnumerable<DateOnly> DaysToRecalculate { get; }
		ShiftTradeRequestStatusCheckerWithSchedule ShiftTradeRequestStatusChecker { get; set; }
	}
}
