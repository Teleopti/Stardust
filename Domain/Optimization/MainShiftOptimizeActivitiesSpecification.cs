using System;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Domain.Specification;

namespace Teleopti.Ccc.Domain.Optimization
{
    public class MainShiftOptimizeActivitiesSpecification : Specification<IMainShift>
    {
        private readonly IOptimizerActivitiesPreferences _optimizerActivitiesPreferences;
        private  IMainShift _originalMainShift;
        private  DateOnly _viewerDate;
        private  ICccTimeZoneInfo _viewerTimeZone;
        private readonly IVisualLayerCollection _visualLayerColl;

        public MainShiftOptimizeActivitiesSpecification(IOptimizerActivitiesPreferences optimizerActivitiesPreferences)
        {
            _optimizerActivitiesPreferences = optimizerActivitiesPreferences;
            
            _visualLayerColl = _originalMainShift.ProjectionService().CreateProjection();
        }

        public override bool IsSatisfiedBy(IMainShift obj)
        {
            IVisualLayerCollection other = obj.ProjectionService().CreateProjection();

            return (CorrectShiftCategory(obj)
                && CorrectContractTime(other)
                && CorrectStart(other)
                && CorrectEnd(other)
                && CorrectAlteredBetween(other)
                && LockedActivityNotMoved(other));
        }

        public  bool IsSatisfiedBy(IMainShift obj, IMainShift originalMainShift, DateOnly viewerDate,ICccTimeZoneInfo viewerTimeZone)
        {
            _originalMainShift = originalMainShift;
            _viewerDate = viewerDate;
            _viewerTimeZone = viewerTimeZone;

            IVisualLayerCollection other = obj.ProjectionService().CreateProjection();

            return (CorrectShiftCategory(obj)
                && CorrectContractTime(other)
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

        public bool CorrectContractTime(IVisualLayerCollection other)
        {
            return _visualLayerColl.ContractTime().Equals(other.ContractTime());
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