﻿using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Common
{
    public interface ISchedulerStateHolder
    {
        /// <summary>
        /// Gets or sets the state of the scheduling result.
        /// </summary>
        /// <value>The state of the scheduling result.</value>
        ISchedulingResultStateHolder SchedulingResultState { get; }

        /// <summary>
        /// Gets the load period.
        /// </summary>
        /// <value>The load period.</value>
		IDateOnlyPeriodAsDateTimePeriod RequestedPeriod { get; set; }

        /// <summary>
        /// Gets the load scenario.
        /// </summary>
        /// <value>The load scenario.</value>
        IScenario RequestedScenario { get; }

        /// <summary>
        /// Gets the people who are permitted in the state.
        /// </summary>
        IList<IPerson> AllPermittedPersons { get; }

        /// <summary>
        /// Resets the filtered people in the state.
        /// </summary>
        void ResetFilteredPersons();

        /// <summary>
        /// Loads the schedules.
        /// </summary>
        /// <param name="scheduleRepository">The schedule repository.</param>
        /// <param name="personsProvider">The persons in organisation provider.</param>
        /// <param name="scheduleDictionaryLoadOptions">The persons in organisation provider.</param>
        /// <param name="period">The period.</param>
        void LoadSchedules(IScheduleRepository scheduleRepository, IPersonProvider personsProvider, IScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions, IScheduleDateTimePeriod period);

        /// <summary>
        /// Gets the schedules.
        /// </summary>
        /// <value>The schedules.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2009-03-21
        /// </remarks>
        IScheduleDictionary Schedules { get; }

        /// <summary>
        /// Gets the filtered person dictionary.
        /// </summary>
        /// <value>The filtered person dictionary.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2009-03-21
        /// </remarks>
        IDictionary<Guid, IPerson> FilteredPersonDictionary { get; }

        /// <summary>
        /// Gets the time zone info.
        /// </summary>
        /// <value>The time zone info.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2009-03-21
        /// </remarks>
        ICccTimeZoneInfo TimeZoneInfo { get; set; }

        /// <summary>
        /// Gets the person requests.
        /// </summary>
        /// <value>The person requests.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-09-08
        /// </remarks>
        IList<IPersonRequest> PersonRequests { get; }

        /// <summary>
        /// Gets the common state holder.
        /// </summary>
        /// <value>The common state holder.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-09-29
        /// </remarks>
        CommonStateHolder CommonStateHolder { get; }

        void LoadCommonState(IUnitOfWork unitOfWork, IRepositoryFactory repositoryFactory);

        /// <summary>
        /// Gets or sets the undo redo container.
        /// </summary>
        /// <value>The undo redo container.</value>
        IUndoRedoContainer UndoRedoContainer { get; set; }

        CommonNameDescriptionSetting CommonNameDescription { get; }

        CommonNameDescriptionSettingScheduleExport CommonNameDescriptionScheduleExport { get; }

        /// <summary>
        /// Check if there are any changed requests
        /// </summary>
        /// <returns></returns>
        bool ChangedRequests();

        void ClearReferredShiftTradeRequests();

        /// <summary>
        /// Gets the default segment length
        /// </summary>
        int DefaultSegmentLength { get; }

        void SetRequestedScenario(IScenario scenario);

        void FilterPersons(IList<IPerson> selectedPersons);

		void FilterPersons(HashSet<Guid> selectedGuids);

        void ClearDaysToRecalculate();

        void LoadSettings(IUnitOfWork unitOfWork, IRepositoryFactory repositoryFactory);

        void MarkDateToBeRecalculated(DateOnly dateToRecalculate);

        string CommonAgentName(IPerson person);

        string CommonAgentNameScheduleExport(IPerson person);

        IEnumerable<DateOnly> DaysToRecalculate { get; }

        DateTimePeriod? LoadedPeriod { get; }

        void LoadPersonRequests(IUnitOfWork unitOfWork, IRepositoryFactory repositoryFactory,
                                IPersonRequestCheckAuthorization authorization);

        IPersonRequest RequestUpdateFromBroker(IPersonRequestRepository personRequestRepository, Guid personRequestId);

        IPersonRequest RequestDeleteFromBroker(Guid personRequestId);
    }
}
