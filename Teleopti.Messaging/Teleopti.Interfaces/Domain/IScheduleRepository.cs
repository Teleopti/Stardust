﻿using System;
using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Interface for schedule repository
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-02-19
    /// </remarks>
    public interface IScheduleRepository : IRepository<IPersistableScheduleData> {


        ///<summary>
        /// Gets the entity of the specified type using the specified id
        ///</summary>
        ///<param name="id"></param>
        ///<typeparam name="T"></typeparam>
        ///<returns></returns>
        T Get<T>(Guid id) where T : IPersistableScheduleData;

        ///<summary>
        /// Gets the entity of the specified type using the specified id
        ///</summary>
        ///<param name="concreteType"></param>
        ///<param name="id"></param>
        ///<returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Get")]
        IPersistableScheduleData Get(Type concreteType, Guid id);
        
        /// <summary>
        /// Finds schedule for the specified people only withing the given period.
        /// </summary>
        /// <param name="personsProvider">A people provider.</param>
        /// <param name="scheduleDictionaryLoadOptions">A people provider.</param>
        /// <param name="period">The period.</param>
        /// <param name="scenario">The scenario.</param>
        /// <returns>A schedule dictionary that can be used to view schedule, but not to modify anything.</returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-03-31
        /// </remarks>
        IScheduleDictionary FindSchedulesOnlyInGivenPeriod(IPersonProvider personsProvider, IScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions, DateTimePeriod period, IScenario scenario);

        /// <summary>
        /// Finds schedules for a person where there is a certain absence
        /// </summary>
        /// <param name="period">The period.</param>
        /// <param name="scenario">The scenario.</param>
        /// <param name="person">The agent.</param>
        /// <param name="absence">The search.</param>
        /// <returns>All underlying asignments</returns>
        /// <remarks>
        /// calculates the periods if there is any absence on that period, even if the highest in projection
        /// </remarks>
        IScheduleRange ScheduleRangeBasedOnAbsence(DateTimePeriod period, IScenario scenario, IPerson person, IAbsence absence);

    	/// <summary>
    	/// Finds the schedules for a specific period and scenario.
    	/// </summary>
    	/// <param name="period">The period.</param>
    	/// <param name="scenario">The scenario.</param>
    	/// <param name="personsProvider">The persons in organization provider.</param>
        /// <param name="scheduleDictionaryLoadOptions">The persons in organization provider.</param>
    	/// <param name="visiblePersons"></param>
    	/// <returns></returns>
    	/// <remarks>
    	/// Created by: rogerkr
    	/// Created date: 2008-02-12
    	/// </remarks>
    	IScheduleDictionary FindSchedulesForPersons(
            IScheduleDateTimePeriod period, 
            IScenario scenario, 
            IPersonProvider personsProvider,
            IScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions,
			IEnumerable<IPerson> visiblePersons);

        /// <summary>
        /// Loads the schedule data aggregate.
        /// </summary>
        /// <param name="scheduleDataType">Type of the schedule data.</param>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2009-11-18
        /// </remarks>
        IPersistableScheduleData LoadScheduleDataAggregate(Type scheduleDataType, Guid id);
    }
}