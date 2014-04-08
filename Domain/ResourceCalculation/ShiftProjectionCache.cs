using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Secrets.WorkShiftCalculator;
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
        private IEditableShift _mainShift;
        private readonly IWorkShift _workShift;
        private  DateTime _schedulingDate;
        private TimeSpan? _workShiftProjectionContractTime;
        private DateTimePeriod? _workShiftProjectionPeriod;
        private  IVisualLayerCollection _mainshiftProjection;
        private TimeZoneInfo _localTimeZoneInfo;
        private DayOfWeek _dayOfWeek;
    	private IPersonalShiftMeetingTimeChecker _personalShiftMeetingTimeChecker;

        protected ShiftProjectionCache()
        { }

     
        public ShiftProjectionCache(IWorkShift workShift, IPersonalShiftMeetingTimeChecker personalShiftMeetingTimeChecker)
        { 
            _workShift = workShift;
        	_personalShiftMeetingTimeChecker = personalShiftMeetingTimeChecker;
        }

        public void SetDate(DateOnly schedulingDate, TimeZoneInfo localTimeZoneInfo)
        {

            if (_schedulingDate != schedulingDate || !_localTimeZoneInfo.Equals(localTimeZoneInfo))
            {
                _localTimeZoneInfo = localTimeZoneInfo;
                _schedulingDate = schedulingDate;
                _dayOfWeek = _schedulingDate.DayOfWeek;
                _mainShift = _workShift.ToEditorShift(schedulingDate.Date, localTimeZoneInfo);
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
        public IEditableShift TheMainShift
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

		IEnumerable<IWorkShiftCalculatableLayer> IWorkShiftCalculatableProjection.WorkShiftCalculatableLayers
		{
			get { return new WorkShiftCalculatableVisualLayerCollection(MainShiftProjection); }
		}

        public DayOfWeek DayOfWeek { get { return _dayOfWeek; } }

        public bool PersonalShiftsAndMeetingsAreInWorkTime(ReadOnlyCollection<IPersonMeeting> meetings, IPersonAssignment personAssignment)
        {
            if (meetings.Count == 0 && personAssignment == null)
            {
                return true;
            }

            var mainShiftProjection = MainShiftProjection;

			if (meetings.Count > 0 && !_personalShiftMeetingTimeChecker.CheckTimeMeeting(_mainShift, mainShiftProjection, meetings))
				return false;

			if (personAssignment != null && !_personalShiftMeetingTimeChecker.CheckTimePersonAssignment(_mainShift, mainShiftProjection, personAssignment))
				return false;

            return true;
        }

        public int ShiftCategoryDayOfWeekJusticeValue
        {
            get { return _mainShift.ShiftCategory.DayOfWeekJusticeValues[_dayOfWeek]; }
        }

    	public TimeSpan WorkShiftStartTime
    	{
			get { return WorkShiftProjectionPeriod.StartDateTime.TimeOfDay; }
    	}

		public TimeSpan WorkShiftEndTime
    	{
			get
			{
				var basedate = WorkShift.BaseDate;
				double day = WorkShiftProjectionPeriod.EndDateTime.Date.Subtract(basedate).TotalDays;
				return WorkShiftProjectionPeriod.EndDateTime.TimeOfDay.Add(TimeSpan.FromDays(day));
			}
    	}

    }
}
