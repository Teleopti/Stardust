using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.FeatureFlags;
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

		public ShiftProjectionCache(IWorkShift workShift, IPersonalShiftMeetingTimeChecker personalShiftMeetingTimeChecker)
		{
			_workShift = workShift;
			_personalShiftMeetingTimeChecker = personalShiftMeetingTimeChecker;
		}

		public ShiftProjectionCache(IWorkShift workShift,
			IPersonalShiftMeetingTimeChecker personalShiftMeetingTimeChecker,
			IDateOnlyAsDateTimePeriod dateOnlyAsDateTimePeriod)
		{
			_workShift = workShift;
			_personalShiftMeetingTimeChecker = personalShiftMeetingTimeChecker;
			SetDate(dateOnlyAsDateTimePeriod);
		}

		public void SetDate(IDateOnlyAsDateTimePeriod dateOnlyAsDateTimePeriod)
		{
			if (_dateOnlyAsPeriod != null && _dateOnlyAsPeriod.Equals(dateOnlyAsDateTimePeriod)) return;

			_dateOnlyAsPeriod = dateOnlyAsDateTimePeriod;
			_workShiftCalculatableLayers = null;
			_mainShift = new Lazy<IEditableShift>(() => _workShift.ToEditorShift(_dateOnlyAsPeriod, _dateOnlyAsPeriod.TimeZone()));
			_mainshiftProjection = new Lazy<IVisualLayerCollection>(() => TheMainShift.ProjectionService().CreateProjection());
		}

		[RemoveMeWithToggle("replace SetDate",Toggles.ResourcePlanner_LessResourcesXXL_74915)]
		public void SetDateLessResources(IDateOnlyAsDateTimePeriod dateOnlyAsDateTimePeriod)
		{
			if (_dateOnlyAsPeriod != null && _dateOnlyAsPeriod.Equals(dateOnlyAsDateTimePeriod)) return;

			_dateOnlyAsPeriod = dateOnlyAsDateTimePeriod;
			_workShiftCalculatableLayers = null;
			_mainShift = new Lazy<IEditableShift>(() => _workShift.ToEditorShift(_dateOnlyAsPeriod, _dateOnlyAsPeriod.TimeZone()));
			_mainshiftProjection = null;
		}

		public IEditableShift TheMainShift => _mainShift.Value;

		public IWorkShift TheWorkShift => _workShift;

		public TimeSpan WorkShiftProjectionContractTime => _workShift.Projection.ContractTime();

		public DateTimePeriod WorkShiftProjectionPeriod => _workShift.Projection.Period().Value;

		[RemoveMeWithToggle("always create",Toggles.ResourcePlanner_LessResourcesXXL_74915)]
		public IVisualLayerCollection MainShiftProjection => _mainshiftProjection?.Value ?? TheMainShift.ProjectionService().CreateProjection();

		public IEnumerable<IWorkShiftCalculatableLayer> WorkShiftCalculatableLayers => _workShiftCalculatableLayers ??
																					   (_workShiftCalculatableLayers = new WorkShiftCalculatableVisualLayerCollection(MainShiftProjection));

		public bool PersonalShiftsAndMeetingsAreInWorkTime(IPersonMeeting[] meetings, IPersonAssignment personAssignment)
		{
			if (meetings.Length == 0 && personAssignment == null)
			{
				return true;
			}

			if (meetings.Length > 0 && !_personalShiftMeetingTimeChecker.CheckTimeMeeting(TheMainShift, meetings))
				return false;

			if (personAssignment != null && !_personalShiftMeetingTimeChecker.CheckTimePersonAssignment(TheMainShift, personAssignment))
				return false;

			return true;
		}

		public TimeSpan WorkShiftStartTime => WorkShiftProjectionPeriod.StartDateTime.TimeOfDay;

		public TimeSpan WorkShiftEndTime => WorkShiftProjectionPeriod.EndDateTime.Subtract(WorkShiftProjectionPeriod.StartDateTime.Date);

		public DateOnly SchedulingDate => _dateOnlyAsPeriod?.DateOnly ?? DateOnly.MinValue;

		public ShiftProjectionCache GetOrCreateNew(IDateOnlyAsDateTimePeriod dateOnlyAsDateTimePeriod)
		{
			return dateOnlyAsDateTimePeriod.Equals(_dateOnlyAsPeriod) ?
				this :
				new ShiftProjectionCache(_workShift, _personalShiftMeetingTimeChecker, dateOnlyAsDateTimePeriod);
		}
	}
}
