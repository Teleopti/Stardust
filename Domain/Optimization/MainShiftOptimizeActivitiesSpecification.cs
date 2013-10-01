using System;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Domain.Specification;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class MainShiftOptimizeActivitiesSpecification : Specification<IMainShift>
	{
        private readonly IOptimizerActivitiesPreferences _optimizerActivitiesPreferences;
		private readonly IMainShift _originalMainShift;
		private readonly DateOnly _viewerDate;
        private readonly TimeZoneInfo _viewerTimeZone;
        private readonly IVisualLayerCollection _visualLayerColl;

		public MainShiftOptimizeActivitiesSpecification(IOptimizerActivitiesPreferences optimizerActivitiesPreferences, IMainShift originalMainShift, DateOnly viewerDate, TimeZoneInfo viewerTimeZone)
        {
            _optimizerActivitiesPreferences = optimizerActivitiesPreferences;
			_originalMainShift = originalMainShift;
			_viewerDate = viewerDate;
			_viewerTimeZone = viewerTimeZone;

			_visualLayerColl = _originalMainShift.ProjectionService().CreateProjection();
        }

        public override bool IsSatisfiedBy(IMainShift obj)
        {
            IVisualLayerCollection other = obj.ProjectionService().CreateProjection();

	        return (CorrectShiftCategory(obj)
	                && CorrectStart(other)
	                && CorrectEnd(other)
	                && CorrectAlteredBetween(other)
	                && LockedActivityNotMoved(other));
        }

        public bool LockedActivityNotMoved(IVisualLayerCollection other)
        {
            if(_optimizerActivitiesPreferences.DoNotMoveActivities.Count == 0)
                return true;

            return (compareLockedActivities(_visualLayerColl, other) && compareLockedActivities(other, _visualLayerColl));

        }

        public bool CorrectShiftCategory(IMainShift shift)
        {
            if(!_optimizerActivitiesPreferences.KeepShiftCategory)
                return true;

            return shift.ShiftCategory.Equals(_originalMainShift.ShiftCategory);
        }

        public bool CorrectStart(IVisualLayerCollection other)
        {
            if(!_optimizerActivitiesPreferences.KeepStartTime)
                return true;

            return other.Period().Value.StartDateTime.Equals(_visualLayerColl.Period().Value.StartDateTime);
        }

        public bool CorrectEnd(IVisualLayerCollection other)
        {
            if (!_optimizerActivitiesPreferences.KeepEndTime)
                return true;

            return other.Period().Value.EndDateTime.Equals(_visualLayerColl.Period().Value.EndDateTime);
        }

        public bool CorrectAlteredBetween(IVisualLayerCollection other)
        {
            if(!_optimizerActivitiesPreferences.AllowAlterBetween.HasValue)
                return true;

            if (!isIdenticalPeriodBefore(other))
                return false;

            if (!isIdenticalPeriodAfter(other))
                return false;


            return true;
        }

        private bool isIdenticalPeriodBefore(IVisualLayerCollection other)
        {
            DateTimePeriod periodBefore = new DateTimePeriod(new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                                                             allowAlterBetweenDateTimePeriod().
                                                                 StartDateTime);
                        
            IVisualLayerCollection shiftLayersOutsideBefore =
                other.FilterLayers(periodBefore);

            IVisualLayerCollection originalLayersOutsideBefore =
                _visualLayerColl.FilterLayers(periodBefore);

            return
                shiftLayersOutsideBefore.IsSatisfiedBy(
                    VisualLayerCollectionSpecification.IdenticalLayers(originalLayersOutsideBefore));

        }

        private bool isIdenticalPeriodAfter(IVisualLayerCollection other)
        {
            DateTimePeriod periodAfter =
                new DateTimePeriod(allowAlterBetweenDateTimePeriod().EndDateTime,
                                   new DateTime(2100, 1, 1, 0, 0, 0, DateTimeKind.Utc));

            IVisualLayerCollection shiftLayersOutsideAfter =
                other.FilterLayers(periodAfter);

            IVisualLayerCollection originalLayersOutsideAfter =
                _visualLayerColl.FilterLayers(periodAfter);

            return
                shiftLayersOutsideAfter.IsSatisfiedBy(
                    VisualLayerCollectionSpecification.IdenticalLayers(originalLayersOutsideAfter));
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

       private DateTimePeriod allowAlterBetweenDateTimePeriod()
       {
           return _optimizerActivitiesPreferences.UtcPeriodFromDateAndTimePeriod(_viewerDate, _viewerTimeZone).Value;
       }
    }
}