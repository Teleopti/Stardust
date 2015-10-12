using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.Common;
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
        private Lazy<IEditableShift> _mainShift;
        private readonly IWorkShift _workShift;
	    private Lazy<IVisualLayerCollection> _mainshiftProjection;
        private TimeZoneInfo _localTimeZoneInfo;
    	private readonly IPersonalShiftMeetingTimeChecker _personalShiftMeetingTimeChecker;
	    private IDateOnlyAsDateTimePeriod _dateOnlyAsPeriod;
	    private readonly Lazy<IVisualLayerCollection> _workShiftProjection;

	    protected ShiftProjectionCache()
        { }

        public ShiftProjectionCache(IWorkShift workShift, IPersonalShiftMeetingTimeChecker personalShiftMeetingTimeChecker)
        { 
            _workShift = workShift;
	        _workShiftProjection = new Lazy<IVisualLayerCollection>(()=>_workShift.ProjectionService().CreateProjection());
        	_personalShiftMeetingTimeChecker = personalShiftMeetingTimeChecker;
        }

        public void SetDate(DateOnly schedulingDate, TimeZoneInfo localTimeZoneInfo)
        {
            if (_dateOnlyAsPeriod == null || _dateOnlyAsPeriod.DateOnly != schedulingDate || _localTimeZoneInfo.Id != localTimeZoneInfo.Id)
            {
                _localTimeZoneInfo = localTimeZoneInfo;
				_mainShift = new Lazy<IEditableShift>(() => _workShift.ToEditorShift(_dateOnlyAsPeriod, _localTimeZoneInfo));
				_mainshiftProjection = new Lazy<IVisualLayerCollection>(() => TheMainShift.ProjectionService().CreateProjection());
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
				return _mainShift.Value;
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
                return _workShiftProjection.Value.ContractTime();
            }
        }

        public DateTimePeriod WorkShiftProjectionPeriod
        {
            get 
            {
                return _workShiftProjection.Value.Period().Value;
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
            get {
	            return _mainshiftProjection.Value;
            }
        }

		IEnumerable<IWorkShiftCalculatableLayer> IWorkShiftCalculatableProjection.WorkShiftCalculatableLayers
		{
			get { return new WorkShiftCalculatableVisualLayerCollection(MainShiftProjection); }
		}

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

    	public TimeSpan WorkShiftStartTime
    	{
			get { return WorkShiftProjectionPeriod.StartDateTime.TimeOfDay; }
    	}

		public TimeSpan WorkShiftEndTime
    	{
			get
			{
				return WorkShiftProjectionPeriod.EndDateTime.Subtract(WorkShiftProjectionPeriod.StartDateTime.Date);
			}
    	}

	    public DateOnly SchedulingDate { get { return _dateOnlyAsPeriod == null ? DateOnly.MinValue : _dateOnlyAsPeriod.DateOnly; } }
    }
}
