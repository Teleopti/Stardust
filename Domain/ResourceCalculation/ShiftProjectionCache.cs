using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Secrets.WorkShiftCalculator;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class ShiftProjectionCache : IWorkShiftCalculatableProjection
	{
		private Lazy<IEditableShift> _mainShift;
        private readonly IWorkShift _workShift;
	    private Lazy<IVisualLayerCollection> _mainshiftProjection;
    	private readonly IPersonalShiftMeetingTimeChecker _personalShiftMeetingTimeChecker;
	    private IDateOnlyAsDateTimePeriod _dateOnlyAsPeriod;
		private WorkShiftCalculatableVisualLayerCollection _workShiftCalculatableLayers;

		public ShiftProjectionCache(IWorkShift workShift, IPersonalShiftMeetingTimeChecker personalShiftMeetingTimeChecker) : this()
        {
	        _workShift = workShift;
        	_personalShiftMeetingTimeChecker = personalShiftMeetingTimeChecker;
        }

		protected ShiftProjectionCache() { }

        public void SetDate(IDateOnlyAsDateTimePeriod dateOnlyAsDateTimePeriod)
        {
	        if (_dateOnlyAsPeriod!=null && _dateOnlyAsPeriod.Equals(dateOnlyAsDateTimePeriod)) return;

	        _dateOnlyAsPeriod = dateOnlyAsDateTimePeriod;
	        _workShiftCalculatableLayers = null;
	        _mainShift = new Lazy<IEditableShift>(() => _workShift.ToEditorShift(_dateOnlyAsPeriod, _dateOnlyAsPeriod.TimeZone()));
	        _mainshiftProjection = new Lazy<IVisualLayerCollection>(() => TheMainShift.ProjectionService().CreateProjection());
        }

        public IEditableShift TheMainShift => _mainShift.Value;

	    public IWorkShift TheWorkShift => _workShift;
		
		public TimeSpan WorkShiftProjectionContractTime => _workShift.Projection.ContractTime();

	    public TimeSpan WorkShiftProjectionWorkTime => _workShift.Projection.WorkTime();

	    public DateTimePeriod WorkShiftProjectionPeriod => _workShift.Projection.Period().Value;

	    public IVisualLayerCollection MainShiftProjection => _mainshiftProjection.Value;

	    public IEnumerable<IWorkShiftCalculatableLayer> WorkShiftCalculatableLayers => _workShiftCalculatableLayers ??
																					   (_workShiftCalculatableLayers = new WorkShiftCalculatableVisualLayerCollection(MainShiftProjection));

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

    	public TimeSpan WorkShiftStartTime => WorkShiftProjectionPeriod.StartDateTime.TimeOfDay;

	    public TimeSpan WorkShiftEndTime => WorkShiftProjectionPeriod.EndDateTime.Subtract(WorkShiftProjectionPeriod.StartDateTime.Date);

	    public DateOnly SchedulingDate => _dateOnlyAsPeriod?.DateOnly ?? DateOnly.MinValue;
    }
}
