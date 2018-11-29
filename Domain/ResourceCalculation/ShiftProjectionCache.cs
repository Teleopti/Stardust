using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class ShiftProjectionCache : IWorkShiftCalculatableProjection
	{
		private Lazy<IEditableShift> _mainShift;
		private IDateOnlyAsDateTimePeriod _dateOnlyAsPeriod;
		private WorkShiftCalculatableVisualLayerCollection _workShiftCalculatableLayers;

		public ShiftProjectionCache(IWorkShift workShift, IDateOnlyAsDateTimePeriod dateOnlyAsDateTimePeriod)
		{
			TheWorkShift = workShift;
			if (dateOnlyAsDateTimePeriod != null)
			{
				SetDate(dateOnlyAsDateTimePeriod);	
			}
		}
		
		public IEditableShift TheMainShift => _mainShift.Value;
		public IWorkShift TheWorkShift { get; }
		public IVisualLayerCollection MainShiftProjection() => TheMainShift.ProjectionService().CreateProjection();
		public IEnumerable<IWorkShiftCalculatableLayer> WorkShiftCalculatableLayers => _workShiftCalculatableLayers ??
																					   (_workShiftCalculatableLayers = new WorkShiftCalculatableVisualLayerCollection(MainShiftProjection()));
		public DateOnly SchedulingDate => _dateOnlyAsPeriod?.DateOnly ?? DateOnly.MinValue;
		
		public void SetDate(IDateOnlyAsDateTimePeriod dateOnlyAsDateTimePeriod)
		{
			if (_dateOnlyAsPeriod != null && _dateOnlyAsPeriod.Equals(dateOnlyAsDateTimePeriod)) return;

			_dateOnlyAsPeriod = dateOnlyAsDateTimePeriod;
			_workShiftCalculatableLayers = null;
			_mainShift = new Lazy<IEditableShift>(() => TheWorkShift.ToEditorShift(_dateOnlyAsPeriod, _dateOnlyAsPeriod.TimeZone()));
		}
	}
}
