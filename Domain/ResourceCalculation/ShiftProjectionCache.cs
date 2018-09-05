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
		private readonly IWorkShift _workShift;
		private Lazy<IVisualLayerCollection> _mainshiftProjection;
		private IDateOnlyAsDateTimePeriod _dateOnlyAsPeriod;
		private WorkShiftCalculatableVisualLayerCollection _workShiftCalculatableLayers;

		public ShiftProjectionCache(IWorkShift workShift)
		{
			_workShift = workShift;
		}

		public ShiftProjectionCache(IWorkShift workShift,
			IDateOnlyAsDateTimePeriod dateOnlyAsDateTimePeriod)
		{
			_workShift = workShift;
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
		
		public IEditableShift TheMainShift => _mainShift.Value;

		public IWorkShift TheWorkShift => _workShift;

		public TimeSpan WorkShiftProjectionContractTime => _workShift.Projection.ContractTime();

		public DateTimePeriod WorkShiftProjectionPeriod => _workShift.Projection.Period().Value;

		public IVisualLayerCollection MainShiftProjection() =>
			_mainshiftProjection?.Value ?? TheMainShift.ProjectionService().CreateProjection();

		public IEnumerable<IWorkShiftCalculatableLayer> WorkShiftCalculatableLayers => _workShiftCalculatableLayers ??
																					   (_workShiftCalculatableLayers = new WorkShiftCalculatableVisualLayerCollection(MainShiftProjection()));

		public TimeSpan WorkShiftStartTime => WorkShiftProjectionPeriod.StartDateTime.TimeOfDay;

		public TimeSpan WorkShiftEndTime => WorkShiftProjectionPeriod.EndDateTime.Subtract(WorkShiftProjectionPeriod.StartDateTime.Date);

		public DateOnly SchedulingDate => _dateOnlyAsPeriod?.DateOnly ?? DateOnly.MinValue;

		public void ClearMainShiftProjectionCache()
		{
			_mainshiftProjection = null;
		}
	}
	
	public static class ShiftProjectionCacheExtensions
	{
		public static void ClearMainShiftProjectionCaches(this IEnumerable<ShiftProjectionCache> shiftProjectionCaches)
		{
			foreach (var shiftProjectionCache in shiftProjectionCaches)
			{
				shiftProjectionCache.ClearMainShiftProjectionCache();
			}
		}
	}
}
