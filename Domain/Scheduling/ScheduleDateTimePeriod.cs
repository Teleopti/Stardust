using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;

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
        /// <param name="visiblePeriod">The period.</param>
        /// <param name="personCollection">The person collection.
        /// These people will extend the "LoadedPeriod" to include time outside period based
        /// on each persons scheduleperiod.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-05-14
        /// </remarks>
        public ScheduleDateTimePeriod(DateTimePeriod visiblePeriod, IEnumerable<IPerson> personCollection)
            : this(visiblePeriod, personCollection, new SchedulerRangeToLoadCalculator(visiblePeriod))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScheduleDateTimePeriod"/> class.
        /// This instance won't take personschedule into consideration.
        /// Use : (int)StateHolderReader.Instance.StateReader.SessionScopeData.SystemSetting[SettingKeys.JusticePointWindow] to load
        /// </summary>
        /// <param name="visiblePeriod">The period.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-05-15
        /// </remarks>
        public ScheduleDateTimePeriod(DateTimePeriod visiblePeriod)
            : this(visiblePeriod, new List<IPerson>(), new SchedulerRangeToLoadCalculator(visiblePeriod))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScheduleDateTimePeriod"/> class.
        /// </summary>
        /// <param name="visiblePeriod">The period.</param>
        /// <param name="personCollection">The person collection.</param>
        /// <param name="rangeToLoadCalculator">The range to load calculator.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-05-20
        /// </remarks>
        public ScheduleDateTimePeriod(DateTimePeriod visiblePeriod, IEnumerable<IPerson> personCollection, ISchedulerRangeToLoadCalculator rangeToLoadCalculator) 
        {
            _visiblePeriod = visiblePeriod;
            _personCollection = personCollection;
            _rangeToLoadCalculator = rangeToLoadCalculator;
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
