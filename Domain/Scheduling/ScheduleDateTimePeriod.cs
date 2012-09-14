﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
    /// <summary>
    /// Gives support to the scheduler on what needed to load, what to show etc
    /// based on a period choosen by the user
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-05-14
    /// </remarks>
    public class ScheduleDateTimePeriod : IScheduleDateTimePeriod
    {
        private readonly DateTimePeriod _visiblePeriod;
        private readonly IEnumerable<IPerson> _personCollection;
        private DateTimePeriod _loadedPeriod;
        private readonly ISchedulerRangeToLoadCalculator _rangeToLoadCalculator;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScheduleDateTimePeriod"/> class.
        /// Use : (int)StateHolderReader.Instance.StateReader.SessionScopeData.SystemSetting[SettingKeys.JusticePointWindow] to load
        /// </summary>
        /// <param name="period">The period.</param>
        /// <param name="personCollection">The person collection.
        /// These people will extend the "LoadedPeriod" to include time outside period based
        /// on each persons scheduleperiod.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-05-14
        /// </remarks>
        public ScheduleDateTimePeriod(DateTimePeriod period, IEnumerable<IPerson> personCollection)
            : this(period, personCollection, new SchedulerRangeToLoadCalculator(period))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScheduleDateTimePeriod"/> class.
        /// This instance won't take personschedule into consideration.
        /// Use : (int)StateHolderReader.Instance.StateReader.SessionScopeData.SystemSetting[SettingKeys.JusticePointWindow] to load
        /// </summary>
        /// <param name="period">The period.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-05-15
        /// </remarks>
        public ScheduleDateTimePeriod(DateTimePeriod period)
            : this(period, new List<IPerson>(), new SchedulerRangeToLoadCalculator(period))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScheduleDateTimePeriod"/> class.
        /// </summary>
        /// <param name="period">The period.</param>
        /// <param name="personCollection">The person collection.</param>
        /// <param name="rangeToLoadCalculator">The range to load calculator.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-05-20
        /// </remarks>
        public ScheduleDateTimePeriod(DateTimePeriod period, IEnumerable<IPerson> personCollection, ISchedulerRangeToLoadCalculator rangeToLoadCalculator) 
        {
            _visiblePeriod = period;
            _personCollection = personCollection;
            _rangeToLoadCalculator = rangeToLoadCalculator;
        }

        /// <summary>
        /// Gets the person collection.
        /// </summary>
        /// <value>The person collection.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-05-14
        /// </remarks>
        public ReadOnlyCollection<IPerson> PersonCollection
        {
            get
            {
                return new List<IPerson>(_personCollection).AsReadOnly();
            }
        }

        /// <summary>
        /// Gets the original period.
        /// </summary>
        /// <value>The period.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-05-14
        /// </remarks>
        public DateTimePeriod VisiblePeriod
        {
            get { return _visiblePeriod; }
        }

		public DateTimePeriod VisiblePeriodMinusFourWeeksPeriod()
		{
			var offset = TimeSpan.FromDays(-28);
			return VisiblePeriod.ChangeStartTime(offset);
		}

        /// <summary>
        /// Gets or sets the range to load calculator.
        /// </summary>
        /// <value>The range to load calculator.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-05-20
        /// </remarks>
        public ISchedulerRangeToLoadCalculator RangeToLoadCalculator
        {
            get 
            {
                return _rangeToLoadCalculator;
            }
        }

        /// <summary>
        /// Gets the max period, based on earliest and latest date
        /// from the people's scheduleperiod
        /// </summary>
        /// <value>The max period.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-05-14
        /// </remarks>
        public DateTimePeriod LoadedPeriod()
        {
            if (_loadedPeriod == new DateTimePeriod())
            {
                DateTimePeriod retVal=VisiblePeriod;
                foreach (IPerson person in _personCollection)
                {
                    DateTimePeriod loadPeriod = _rangeToLoadCalculator.SchedulerRangeToLoad(person);
                    retVal = retVal.MaximumPeriod(loadPeriod);
                }
                _loadedPeriod = retVal;
            }
            return _loadedPeriod;
        }
    }
}
