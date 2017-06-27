using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public interface ISchedulerStateHolder
	{
		bool ConsiderShortBreaks { get; set; }

		ISchedulingResultStateHolder SchedulingResultState { get; }

		IDateOnlyPeriodAsDateTimePeriod RequestedPeriod { get; set; }

		IScenario RequestedScenario { get; }

		IList<IPerson> AllPermittedPersons { get; }

		void ResetFilteredPersons();

		void ResetFilteredPersonsOvertimeAvailability();

		void ResetFilteredPersonsHourlyAvailability();

		void LoadSchedules(IFindSchedulesForPersons findSchedulesForPersons, IPersonProvider personsProvider, ScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions, IScheduleDateTimePeriod period);

		IScheduleDictionary Schedules { get; }

		IDictionary<Guid, IPerson> FilteredCombinedAgentsDictionary { get; }


		IDictionary<Guid, IPerson> FilteredAgentsDictionary { get; }

		TimeZoneInfo TimeZoneInfo { get; set; }

		IList<IPersonRequest> PersonRequests { get; }

		ICommonStateHolder CommonStateHolder { get; }

		void LoadCommonState(IUnitOfWork unitOfWork, IRepositoryFactory repositoryFactory);

		[Obsolete("Remove this one")]
		IUndoRedoContainer UndoRedoContainer { get; set; }

		CommonNameDescriptionSetting CommonNameDescription { get; }

		bool ChangedRequests();

		int DefaultSegmentLength { get; }

		void SetRequestedScenario(IScenario scenario);

		void FilterPersons(IList<IPerson> selectedPersons);

		void FilterPersonsOvertimeAvailability(IEnumerable<IPerson> selectedPersons);

		void FilterPersonsHourlyAvailability(IList<IPerson> selectedPersons);

		void FilterPersons(HashSet<Guid> selectedGuids);

		void ClearDaysToRecalculate();

		void LoadSettings(IUnitOfWork unitOfWork, IRepositoryFactory repositoryFactory);

		void MarkDateToBeRecalculated(DateOnly dateToRecalculate);

		string CommonAgentName(IPerson person);

		string CommonAgentNameScheduleExport(IPerson person);

		IEnumerable<DateOnly> DaysToRecalculate { get; }

		void LoadPersonRequests(IUnitOfWork unitOfWork, IRepositoryFactory repositoryFactory,
										 IPersonRequestCheckAuthorization authorization, int numberOfDaysToShowNonPendingRequests);

		IPersonRequest RequestUpdateFromBroker(IPersonRequestRepository personRequestRepository, Guid personRequestId, IScheduleStorage scheduleStorage);

		IPersonRequest RequestDeleteFromBroker(Guid personRequestId);

		bool AgentFilter();
		void LoadCommonStateForResourceCalculationOnly(IUnitOfWork unitOfWork, IRepositoryFactory repositoryFactory);
	}
}
