using System;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    /// <summary>
    /// Used in Scheduling to cache the projection so it only has to be done once
    /// </summary>
    /// /// 
    /// <remarks>
    ///  Created by: Ola
    ///  Created date: 2008-09-16    
    /// /// </remarks>
    public class ShiftProjectionCache : IShiftProjectionCache
    {
        private IMainShift _mainShift;
        private readonly IWorkShift _workShift;
        private  DateTime _schedulingDate;
        private TimeSpan? _workShiftProjectionContractTime;
        private DateTimePeriod? _workShiftProjectionPeriod;
        private  IVisualLayerCollection _mainshiftProjection;
        private ICccTimeZoneInfo _localTimeZoneInfo;
        private DayOfWeek _dayOfWeek;

        protected ShiftProjectionCache()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShiftProjectionCache"/> class.
        /// </summary>
        /// <param name="workShift">The work shift.</param>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-09-16    
        /// /// </remarks>
        public ShiftProjectionCache(IWorkShift workShift)
        { 
            _workShift = workShift;
        }

        public void SetDate(DateOnly schedulingDate, ICccTimeZoneInfo localTimeZoneInfo)
        {

            if (_schedulingDate != schedulingDate || !_localTimeZoneInfo.Equals(localTimeZoneInfo))
            {
                _localTimeZoneInfo = localTimeZoneInfo;
                _schedulingDate = schedulingDate;
                _dayOfWeek = _schedulingDate.DayOfWeek;
                _mainShift = _workShift.ToMainShift(schedulingDate.Date, localTimeZoneInfo);
                _mainshiftProjection = null;
            }
            
            
        }
        /// <summary>
        /// Gets the main shift.
        /// </summary>
        /// <value>The main shift.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-09-16    
        /// /// </remarks>
        public IMainShift TheMainShift
        {
            get { return _mainShift; }
        }
        /// <summary>
        /// Gets the work shift.
        /// </summary>
        /// <value>The work shift.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-09-16    
        /// /// </remarks>
        public IWorkShift TheWorkShift
        {
            get { return _workShift; }
        }

        public TimeSpan WorkShiftProjectionContractTime
        {
            get
            {
                if (!_workShiftProjectionContractTime.HasValue)
                    _workShiftProjectionContractTime = _workShift.ProjectionService().CreateProjection().ContractTime();
                return _workShiftProjectionContractTime.Value;
            }
        }

        public DateTimePeriod WorkShiftProjectionPeriod
        {
            get 
            {
                if (!_workShiftProjectionPeriod.HasValue)
                    _workShiftProjectionPeriod = _workShift.ProjectionService().CreateProjection().Period();
                return _workShiftProjectionPeriod.Value;
            }
        }

        /// <summary>
        /// Gets the main shift projection.
        /// </summary>
        /// <value>The main shift projection.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-09-16    
        /// /// </remarks>
        public IVisualLayerCollection MainShiftProjection
        {
            get
            {
                if (_mainshiftProjection == null)
                    _mainshiftProjection = _mainShift.ProjectionService().CreateProjection();
                return _mainshiftProjection;
            }
        }

        public DayOfWeek DayOfWeek { get { return _dayOfWeek; } }

        public bool PersonalShiftsAndMeetingsAreInWorkTime(ReadOnlyCollection<IPersonMeeting> meetings, ReadOnlyCollection<IPersonAssignment> personAssignments)
        {
            if (meetings.Count == 0 && personAssignments.Count == 0)
            {
                return true;
            }
            IVisualLayerCollection mainShiftProjection = MainShiftProjection;

            foreach (IPersonMeeting personMeeting in meetings)
            {
                if (!PeriodIsWorkTimeInProjection(mainShiftProjection, personMeeting.Period))
                    return false;
            }
            foreach (IPersonAssignment personAssignment in personAssignments)
            {
                foreach (IPersonalShift personalShift in personAssignment.PersonalShiftCollection)
                {
                    if (personalShift.LayerCollection.Period().HasValue)
                        if(!PeriodIsWorkTimeInProjection(mainShiftProjection, personalShift.LayerCollection.Period().Value))
                            return false;
                }
            }
            return true;
        }

        public int ShiftCategoryDayOfWeekJusticeValue
        {
            get { return _mainShift.ShiftCategory.DayOfWeekJusticeValues[_dayOfWeek]; }
        }

        private static bool PeriodIsWorkTimeInProjection(IVisualLayerCollection mainShiftProjection, DateTimePeriod period)
        {
            foreach (VisualLayer visualLayer in mainShiftProjection)
            {
                if (visualLayer.Period.Intersect(period))
                {
                    if (visualLayer.HighestPriorityActivity != null && !visualLayer.HighestPriorityActivity.InWorkTime)
                        return false;
                }
            }
            return true;
        }
    }
}
