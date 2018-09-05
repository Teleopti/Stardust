using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	[RemoveMeWithToggle(Toggles.ResourcePlanner_XXL_76496)]
	public class ShiftProjectionCacheOld_KeepProjectionState : ShiftProjectionCache
	{
		private Lazy<IVisualLayerCollection> _mainshiftProjection;
		
		public ShiftProjectionCacheOld_KeepProjectionState(IWorkShift workShift, IDateOnlyAsDateTimePeriod dateOnlyAsDateTimePeriod) : base(workShift, dateOnlyAsDateTimePeriod)
		{
		}

		public override IVisualLayerCollection MainShiftProjection() =>
			_mainshiftProjection?.Value ?? TheMainShift.ProjectionService().CreateProjection();

		public override void SetDateExtra()
		{
			_mainshiftProjection = new Lazy<IVisualLayerCollection>(() => TheMainShift.ProjectionService().CreateProjection());
		}
	}
	
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
		[RemoveMeWithToggle("make sealed", Toggles.ResourcePlanner_XXL_76496)]
		public virtual IVisualLayerCollection MainShiftProjection() => TheMainShift.ProjectionService().CreateProjection();
		public IEnumerable<IWorkShiftCalculatableLayer> WorkShiftCalculatableLayers => _workShiftCalculatableLayers ??
																					   (_workShiftCalculatableLayers = new WorkShiftCalculatableVisualLayerCollection(MainShiftProjection()));
		public DateOnly SchedulingDate => _dateOnlyAsPeriod?.DateOnly ?? DateOnly.MinValue;
		
		public void SetDate(IDateOnlyAsDateTimePeriod dateOnlyAsDateTimePeriod)
		{
			if (_dateOnlyAsPeriod != null && _dateOnlyAsPeriod.Equals(dateOnlyAsDateTimePeriod)) return;

			_dateOnlyAsPeriod = dateOnlyAsDateTimePeriod;
			_workShiftCalculatableLayers = null;
			_mainShift = new Lazy<IEditableShift>(() => TheWorkShift.ToEditorShift(_dateOnlyAsPeriod, _dateOnlyAsPeriod.TimeZone()));
			SetDateExtra();
		}

		[RemoveMeWithToggle(Toggles.ResourcePlanner_XXL_76496)]
		public virtual void SetDateExtra(){}
	}
}
