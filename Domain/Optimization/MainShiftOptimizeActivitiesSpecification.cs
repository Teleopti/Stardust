using System;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Domain.Specification;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class MainShiftOptimizeActivitiesSpecification : Specification<IEditableShift>
	{
        private readonly OptimizerActivitiesPreferences _optimizerActivitiesPreferences;
		private readonly IEditableShift _originalMainShift;
		private readonly DateOnly _viewerDate;
        private readonly TimeZoneInfo _viewerTimeZone;
        private readonly Lazy<IVisualLayerCollection> _visualLayerColl;

		public MainShiftOptimizeActivitiesSpecification(OptimizerActivitiesPreferences optimizerActivitiesPreferences, IEditableShift originalMainShift, DateOnly viewerDate, TimeZoneInfo viewerTimeZone)
        {
            _optimizerActivitiesPreferences = optimizerActivitiesPreferences;
			_originalMainShift = originalMainShift;
			_viewerDate = viewerDate;
			_viewerTimeZone = viewerTimeZone;

			_visualLayerColl = new Lazy<IVisualLayerCollection>(()=>_originalMainShift.ProjectionService().CreateProjection());
        }

		public override bool IsSatisfiedBy(IEditableShift shift)
        {
            IVisualLayerCollection other = shift.ProjectionService().CreateProjection();

	        return CorrectShiftCategory(shift)
				   && CorrectStart(other)
				   && CorrectEnd(other)
				   && CorrectAlteredBetween(other)
				   && LockedActivityNotMoved(other)
				   && LengthOfActivityEqual(other);
        }

        public bool LockedActivityNotMoved(IVisualLayerCollection other)
        {
            if(_optimizerActivitiesPreferences.DoNotMoveActivities.Count == 0)
                return true;

            return compareLockedActivities(_visualLayerColl.Value, other) && compareLockedActivities(other, _visualLayerColl.Value);
        }

		public bool LengthOfActivityEqual(IVisualLayerCollection other)
		{
			if (_optimizerActivitiesPreferences.DoNotAlterLengthOfActivity == null)
				return true;

			return compareLengthOfStaticLengthActivity(_visualLayerColl.Value, other);
		}

		public bool CorrectShiftCategory(IEditableShift otherShift)
        {
            if(!_optimizerActivitiesPreferences.KeepShiftCategory)
                return true;

			return otherShift.ShiftCategory.Equals(_originalMainShift.ShiftCategory);
        }

        public bool CorrectStart(IVisualLayerCollection other)
        {
            if(!_optimizerActivitiesPreferences.KeepStartTime)
                return true;

            return other.Period().Value.StartDateTime.Equals(_visualLayerColl.Value.Period().Value.StartDateTime);
        }

        public bool CorrectEnd(IVisualLayerCollection other)
        {
            if (!_optimizerActivitiesPreferences.KeepEndTime)
                return true;

            return other.Period().Value.EndDateTime.Equals(_visualLayerColl.Value.Period().Value.EndDateTime);
        }

		public bool CorrectAlteredBetween(IVisualLayerCollection other)
		{
			if (!_optimizerActivitiesPreferences.AllowAlterBetween.HasValue)
				return true;

			var allowPeriod = _optimizerActivitiesPreferences.UtcPeriodFromDateAndTimePeriod(_viewerDate, _viewerTimeZone).Value;
			var baseDate = TimeZoneHelper.ConvertToUtc(_viewerDate.Date, _viewerTimeZone);
			var start = baseDate.AddDays(-1).Add(allowPeriod.EndDateTime.Subtract(baseDate));
			var periodBefore = new DateTimePeriod(start, allowPeriod.StartDateTime);
			var shiftLayersOutsideBefore = other.FilterLayers(periodBefore);
			var originalLayersOutsideBefore = _visualLayerColl.Value.FilterLayers(periodBefore);

			if (!shiftLayersOutsideBefore.IsSatisfiedBy(VisualLayerCollectionSpecification.IdenticalLayers(originalLayersOutsideBefore)))
				return false;

			var end = baseDate.AddDays(1).Add(allowPeriod.StartDateTime.Subtract(baseDate));
			var periodAfter = new DateTimePeriod(allowPeriod.EndDateTime, end);
			var shiftLayersOutsideAfter = other.FilterLayers(periodAfter);
			var originalLayersOutsideAfter = _visualLayerColl.Value.FilterLayers(periodAfter);
			if (!shiftLayersOutsideAfter.IsSatisfiedBy(VisualLayerCollectionSpecification.IdenticalLayers(originalLayersOutsideAfter)))
				return false;


			return true;
		}

		private bool compareLengthOfStaticLengthActivity(IVisualLayerCollection compareFrom, IVisualLayerCollection compareTo)
		{
			IActivity staticActivity = _optimizerActivitiesPreferences.DoNotAlterLengthOfActivity;
			TimeSpan fromLength = TimeSpan.Zero;
			foreach (var fromLayer in compareFrom)
			{
				if (fromLayer.Payload.Equals(staticActivity))
					fromLength = fromLength.Add(fromLayer.Period.ElapsedTime());
			}

			TimeSpan toLength = TimeSpan.Zero;
			foreach (var toLayer in compareTo)
			{
				if (toLayer.Payload.Equals(staticActivity))
					toLength = toLength.Add(toLayer.Period.ElapsedTime());
			}

			return fromLength == toLength;
		}

		private bool compareLockedActivities(IVisualLayerCollection compareFrom, IVisualLayerCollection compareTo)
        {
            foreach (var fromLayer in compareFrom)
            {
                if (_optimizerActivitiesPreferences.DoNotMoveActivities.Contains((IActivity)fromLayer.Payload))
                {
                    bool found = false;
                    foreach (var toLayer in compareTo)
                    {
                        if (fromLayer.Period.Equals(toLayer.Period) && fromLayer.Payload.Equals(toLayer.Payload))
                            found = true;
                    }
                    if (!found)
                        return false;
                }
            }
            return true;
        }
    }
}