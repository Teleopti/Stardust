using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class ShiftProjectionCache : IWorkShiftCalculatableProjection
	{
		private Lazy<IEditableShift> _mainShift;
		private Lazy<IVisualLayerCollection> _mainshiftProjection;
		private IDateOnlyAsDateTimePeriod _dateOnlyAsPeriod;
		private WorkShiftCalculatableVisualLayerCollection _workShiftCalculatableLayers;

		public ShiftProjectionCache(IWorkShift workShift)
		{
			TheWorkShift = workShift;
		}

		public ShiftProjectionCache(IWorkShift workShift,
			IDateOnlyAsDateTimePeriod dateOnlyAsDateTimePeriod)
		{
			TheWorkShift = workShift;
			SetDate(dateOnlyAsDateTimePeriod);
		}

		public void SetDate(IDateOnlyAsDateTimePeriod dateOnlyAsDateTimePeriod)
		{
			if (_dateOnlyAsPeriod != null && _dateOnlyAsPeriod.Equals(dateOnlyAsDateTimePeriod)) return;

			_dateOnlyAsPeriod = dateOnlyAsDateTimePeriod;
			_workShiftCalculatableLayers = null;
			_mainShift = new Lazy<IEditableShift>(() => TheWorkShift.ToEditorShift(_dateOnlyAsPeriod, _dateOnlyAsPeriod.TimeZone()));
			_mainshiftProjection = new Lazy<IVisualLayerCollection>(() => TheMainShift.ProjectionService().CreateProjection());
		}
		
		public IEditableShift TheMainShift => _mainShift.Value;

		public IWorkShift TheWorkShift { get; }

		public TimeSpan WorkShiftProjectionContractTime => TheWorkShift.Projection.ContractTime();

		public IVisualLayerCollection MainShiftProjection() =>
			_mainshiftProjection?.Value ?? TheMainShift.ProjectionService().CreateProjection();

		public IEnumerable<IWorkShiftCalculatableLayer> WorkShiftCalculatableLayers => _workShiftCalculatableLayers ??
																					   (_workShiftCalculatableLayers = new WorkShiftCalculatableVisualLayerCollection(MainShiftProjection()));

		public DateOnly SchedulingDate => _dateOnlyAsPeriod?.DateOnly ?? DateOnly.MinValue;

		public void ClearMainShiftProjectionCache()
		{
			_mainshiftProjection = null;
		}
	}
}
