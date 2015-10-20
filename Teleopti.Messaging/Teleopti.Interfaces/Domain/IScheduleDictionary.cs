using System;
using System.Collections.Generic;
using System.Globalization;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Holds multiple schedule range objects.
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-06-02
    /// </remarks>
    public interface IScheduleDictionary : IDictionary<IPerson, IScheduleRange>, ICloneable
    {
        /// <summary>
        /// Gets a value indicating whether [permissions enabled].
        /// </summary>
        /// <value><c>true</c> if [permissions enabled]; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2009-12-07
        /// </remarks>
        bool PermissionsEnabled { get; }

        /// <summary>
        /// Gets the period.
        /// </summary>
        /// <value>The period.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-02-12
        /// </remarks>
        IScheduleDateTimePeriod Period { get; }

        /// <summary>
        /// Gets the scenario.
        /// </summary>
        /// <value>The scenario.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-02-12
        /// </remarks>
        IScenario Scenario { get; }

        /// <summary>
        /// Changes made since last snapshot.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-05-29
        /// </remarks>
		IDifferenceCollection<IPersistableScheduleData> DifferenceSinceSnapshot();

        /// <summary>
        /// Extracts all schedule data.
        /// </summary>
        /// <param name="extractor">The extractor.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-05-19
        /// </remarks>
        [Obsolete("Depricated. Always specify a period!")]
        void ExtractAllScheduleData(IScheduleExtractor extractor);

        /// <summary>
        /// Extracts all schedule data.
        /// </summary>
        /// <param name="extractor">The extractor.</param>
        /// <param name="period">The period.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-05-19
        /// </remarks>
        void ExtractAllScheduleData(IScheduleExtractor extractor, DateTimePeriod period);

		IEnumerable<IBusinessRuleResponse> Modify(IScheduleDay scheduleDay);

		/// <summary>
		/// Modifies the specified modifier.
		/// </summary>
		/// <param name="modifier">The modifier.</param>
		/// <param name="scheduleParts">The schedule parts.</param>
		/// <param name="newBusinessRuleCollection">The new business rule collection.</param>
		/// <param name="scheduleDayChangeCallback">The schedule day change callback.</param>
		/// <param name="scheduleTagSetter">The schedule tag setter.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2010-04-22
		/// </remarks>
		IEnumerable<IBusinessRuleResponse> Modify(ScheduleModifier modifier,
                                                  IEnumerable<IScheduleDay> scheduleParts,
                                                  INewBusinessRuleCollection newBusinessRuleCollection,
                                                  IScheduleDayChangeCallback scheduleDayChangeCallback,
                                                  IScheduleTagSetter scheduleTagSetter
            );

        /// <summary>
        /// Modifies the specified modifier.
        /// </summary>
        /// <param name="modifier">The modifier.</param>
        /// <param name="schedulePart">The schedule part.</param>
        /// <param name="newBusinessRuleCollection">The new business rule collection.</param>
        /// <param name="scheduleDayChangeCallback">The schedule day change callback.</param>
        /// <param name="scheduleTagSetter">The schedule tag setter.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2010-04-22
        /// </remarks>
        IEnumerable<IBusinessRuleResponse> Modify(ScheduleModifier modifier,
                                                  IScheduleDay schedulePart,
                                                  INewBusinessRuleCollection newBusinessRuleCollection,
                                                  IScheduleDayChangeCallback scheduleDayChangeCallback,
                                                  IScheduleTagSetter scheduleTagSetter
            );


		/// <summary>
		/// Gets all the schedules for the specified period.
		/// </summary>
		/// <param name="dateOnly">The date.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: micke
		/// Created date: 2008-02-26
		/// </remarks>
		IEnumerable<IScheduleDay> SchedulesForDay(DateOnly dateOnly);

        /// <summary>
        /// Takes the snapshot for later use when checking what has been changed.
        /// Is not supposed to be called explicitly in normal cases.
        /// </summary>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-05-28
        /// </remarks>
        void TakeSnapshot();

        /// <summary>
        /// Gets the difference service.
        /// </summary>
        /// <value>The difference service.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-05-29
        /// </remarks>
        IDifferenceCollectionService<IPersistableScheduleData> DifferenceCollectionService { get; }

        /// <summary>
        /// Holds a list of modifyed personal accounts
        /// </summary>
        ICollection<IPersonAbsenceAccount> ModifiedPersonAccounts { get;}
        
        /// <summary>
        /// Updates this instance from data source/message broker.
        /// </summary>
        /// <param name="personAssignmentRepository">The person assignment repository.</param>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-06-12
        /// </remarks>
        //todo: Change this one to accept an IMessage and IScheduleRepository instead!
	IPersistableScheduleData UpdateFromBroker<T>(ILoadAggregateFromBroker<T> personAssignmentRepository, Guid id) where T : IPersistableScheduleData;

        /// <summary>
        /// Updates this instance from data source (Meeting)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="meetingRepository"></param>
        /// <param name="id"></param>
	void MeetingUpdateFromBroker<T>(ILoadAggregateFromBroker<T> meetingRepository, Guid id) where T : IMeeting;

        /// <summary>
        /// Deletes from data source.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-06-17
        /// </remarks>
	IPersistableScheduleData DeleteFromBroker(Guid id);

        /// <summary>
        /// Validates the business rules on persons.
        /// </summary>
        /// <param name="people">The persons.</param>
        /// <param name="cultureInfo">The CultureInfo.</param>
        /// <param name="newBusinessRuleCollection">The new business rule collection.</param>
        /// ///
        /// <remarks>
        /// Created by: Ola
        /// Created date: 2008-08-27
        /// /// </remarks>
        void ValidateBusinessRulesOnPersons(IEnumerable<IPerson> people, CultureInfo cultureInfo, INewBusinessRuleCollection newBusinessRuleCollection);


        /// <summary>
        /// Occurs when [part modified].
        /// </summary>
        event EventHandler<ModifyEventArgs> PartModified;

        /// <summary>
        /// Sets the undo redo container.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-11-11
        /// </remarks>
        void SetUndoRedoContainer(IUndoRedoContainer container);

        /// <summary>
        /// Deletes the meeting from broker.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-10-26
        /// </remarks>
        void DeleteMeetingFromBroker(Guid id);

    }
}