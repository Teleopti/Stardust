﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.Common;
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
        private TimeSpan? _workShiftProjectionContractTime;
        private DateTimePeriod? _workShiftProjectionPeriod;
	    private IVisualLayerCollection _mainshiftProjection;
        private TimeZoneInfo _localTimeZoneInfo;
        private DayOfWeek _dayOfWeek;
    	private readonly IPersonalShiftMeetingTimeChecker _personalShiftMeetingTimeChecker;
	    private IDateOnlyAsDateTimePeriod _dateOnlyAsPeriod;

	    protected ShiftProjectionCache()
        { }

     
        public ShiftProjectionCache(IWorkShift workShift, IPersonalShiftMeetingTimeChecker personalShiftMeetingTimeChecker)
        { 
            _workShift = workShift;
        	_personalShiftMeetingTimeChecker = personalShiftMeetingTimeChecker;
        }

        public void SetDate(DateOnly schedulingDate, TimeZoneInfo localTimeZoneInfo)
        {
            if (_dateOnlyAsPeriod == null || _dateOnlyAsPeriod.DateOnly != schedulingDate || _localTimeZoneInfo.Id != localTimeZoneInfo.Id)
            {
                _localTimeZoneInfo = localTimeZoneInfo;
                _dayOfWeek = schedulingDate.DayOfWeek;
	            _mainShift = null;
                _mainshiftProjection = null;
	            _dateOnlyAsPeriod = new DateOnlyAsDateTimePeriod(schedulingDate, localTimeZoneInfo);
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
	        get
	        {
		        if (_mainShift == null)
					_mainShift = _workShift.ToEditorShift(_dateOnlyAsPeriod, _localTimeZoneInfo);
				return _mainShift;
	        }
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
	        get
	        { return _workShift; }
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
                    _mainshiftProjection = TheMainShift.ProjectionService().CreateProjection();
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

			if (meetings.Count > 0 && !_personalShiftMeetingTimeChecker.CheckTimeMeeting(TheMainShift, meetings))
				return false;

			if (personAssignment != null && !_personalShiftMeetingTimeChecker.CheckTimePersonAssignment(TheMainShift, personAssignment))
				return false;

            return true;
        }

        public int ShiftCategoryDayOfWeekJusticeValue
        {
			get { return TheMainShift.ShiftCategory.DayOfWeekJusticeValues[_dayOfWeek]; }
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

	    public DateOnly SchedulingDate { get { return _dateOnlyAsPeriod == null ? DateOnly.MinValue : _dateOnlyAsPeriod.DateOnly; } }
    }
}
