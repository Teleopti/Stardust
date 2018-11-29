using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TimeLayer
{
    /// <summary>
    /// Class for creating multiplicatorprojections
    /// </summary>
    /// <remarks>
    /// Decided 2009-03-25
    /// Rules for Overtime/OBTime:
    /// When checking for overime, only layers with that definitionset should count as overtime
    /// When checking for OBTime,all layers should be projected against that DefinitionSet (changed to not project overtime layers against OB)
    /// DefintionSets of one type can never createMultiplicatorLayers that overlap
    /// </remarks>
    public class MultiplicatorProjectionService : IMultiplicatorProjectionService
    {
        private readonly IScheduleDay _schedulePart;
        private readonly DateOnly _dateOnly;

        public MultiplicatorProjectionService(IScheduleDay schedulePart,DateOnly dateOnly)
        {
            _schedulePart = schedulePart;
            _dateOnly = dateOnly;
        }

        public IList<IMultiplicatorLayer> CreateProjection()
        {
            var retList = new List<IMultiplicatorLayer>();
            //Get all definitionsets from the projection:
            var definitions = AllUniqueDefintionSets();
            
            //Create all MultiplicatorLayers:
            IEnumerable<IMultiplicatorLayer> multiplicatorLayers = ProjectionFromDefinitionSets(definitions);

            //Create projection 
            var projection = _schedulePart.ProjectionService().CreateProjection();
            
            //If OB is available from contract multiplicator definition sets we need to make a projection of the day
            var shiftAllowanceLayers = CreateLayersWithShiftAllowance(definitions, multiplicatorLayers, projection);
            retList.AddRange(shiftAllowanceLayers);

            var overtimeLayers = CreateLayersWithOvertime(definitions, multiplicatorLayers, projection);
            retList.AddRange(overtimeLayers);

            return retList;
        }

        private IEnumerable<IMultiplicatorLayer> CreateLayersWithOvertime(IEnumerable<IMultiplicatorDefinitionSet> definitions, IEnumerable<IMultiplicatorLayer> multiplicatorLayers, IVisualLayerCollection projection)
        {
            foreach (var definitionSet in definitions.Where(d => d.MultiplicatorType == MultiplicatorType.Overtime))
            {
                var definitionSetLayers =
                    multiplicatorLayers.Where(m => m.MultiplicatorDefinitionSet.Equals(definitionSet));
                var layersWithOvertimeForDefinitionSet = projection.Where(l => definitionSet.Equals(l.DefinitionSet));
                foreach (var overtimeLayer in layersWithOvertimeForDefinitionSet)
                {
                    if (!LayerIsValidForOvertime(projection, overtimeLayer)) continue;
                    foreach (var intersectingLayer in IntersectingLayers(overtimeLayer,definitionSetLayers))
                    {
                        yield return intersectingLayer;
                    }
                }
            }
        }

        private IEnumerable<IMultiplicatorLayer> CreateLayersWithShiftAllowance(IEnumerable<IMultiplicatorDefinitionSet> definitions, IEnumerable<IMultiplicatorLayer> multiplicatorLayers, IVisualLayerCollection projection)
        {
            if (NotDeletedShiftAllowance(definitions))
            {
                var obMultiplicatorLayers =
                    multiplicatorLayers.Where(m => m.Payload.MultiplicatorType == MultiplicatorType.OBTime);
                foreach (IVisualLayer layer in projection)
                {
                    
                    if (!LayerIsValidForOB(layer)) continue;
                    foreach (var projectedLayer in IntersectingLayersForOB(layer, obMultiplicatorLayers))
                    {
                        yield return projectedLayer;
                    }
                }
            }
        }

        private static bool NotDeletedShiftAllowance(IEnumerable<IMultiplicatorDefinitionSet> definitions)
        {
            return definitions.Any(d => d.MultiplicatorType == MultiplicatorType.OBTime && !d.IsDeleted);
        }

        private static bool LayerIsValidForOB(IVisualLayer visualLayer)
        {
            if (visualLayer.DefinitionSet != null && visualLayer.DefinitionSet.MultiplicatorType == MultiplicatorType.Overtime)
                return false;
            return !(visualLayer.Payload is IAbsence) && visualLayer.Payload.InContractTime;
        }

        private static bool LayerIsValidForOvertime(IVisualLayerCollection projection, IVisualLayer visualLayer)
        {
            return !(visualLayer.Payload is IAbsence) && projection.FilterLayers(visualLayer.Period).PaidTime() > TimeSpan.Zero;
        }

        //Returns all the definitionsets in the visuallayercollection
        private IEnumerable<IMultiplicatorDefinitionSet> AllUniqueDefintionSets()
        {
            var personPeriod = _schedulePart.Person.Period(_dateOnly);
            if (personPeriod == null ||
                personPeriod.PersonContract==null)
                return new List<IMultiplicatorDefinitionSet>();

            return personPeriod.PersonContract.Contract.MultiplicatorDefinitionSetCollection.Where(c => c.IsDeleted == false);
        }

        /// <summary>
        /// Returns a projection of the visuallayer and the supplied layersToCheck collection
        /// </summary>
        /// <remarks>
        /// Henrik 2009-03-25
        /// Assuming a definitionset with one type of Multiplicatror (Overtime, OB etc) can not overlap
        /// Two defintionsets can NEVER create a intersecting MultiplicatorLayer with the same multiplicator.
        /// If needed, add a check here and throw an error if thats the case.
        /// </remarks>
        private IEnumerable<IMultiplicatorLayer> ProjectionFromDefinitionSets(IEnumerable<IMultiplicatorDefinitionSet> sets)
        {
            var period = new DateOnlyPeriod(_dateOnly.AddDays(-1), _dateOnly.AddDays(1));
			
			return sets.SelectMany(s => s.CreateProjectionForPeriod(period, _schedulePart.TimeZone)).ToArray();
        }

        private static IEnumerable<MultiplicatorLayer> IntersectingLayersForOB(IVisualLayer layer, IEnumerable<IMultiplicatorLayer> layersToCheck)
        {
            IList<MultiplicatorLayer> ret = new List<MultiplicatorLayer>();
            foreach (var multiplicatorLayer in layersToCheck)
            {
                if(layer.Period.Intersect(multiplicatorLayer.Period))
                    ret.Add(new MultiplicatorLayer(multiplicatorLayer.MultiplicatorDefinitionSet, multiplicatorLayer.Payload, layer.Period.Intersection(multiplicatorLayer.Period).Value));
            }

            return ret;
        }

        /// <summary>
        /// Intersectings the layers.
        /// </summary>
        /// <param name="layer">The layer.</param>
        /// <param name="layersToCheck">The layers to check.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-12-11
        /// </remarks>
        private IEnumerable<MultiplicatorLayer> IntersectingLayers(IVisualLayer layer, IEnumerable<IMultiplicatorLayer> layersToCheck)
        {
            var periods = SplitPeriodOverMidnight(layer.Period);
            return (from p in periods
                    from l in layersToCheck
                    where l.Period.Intersect(p)
                    select new MultiplicatorLayer(l.MultiplicatorDefinitionSet, l.Payload,
                                               l.Period.Intersection(p).Value));
        }

        private IEnumerable<DateTimePeriod> SplitPeriodOverMidnight(DateTimePeriod period)
        {
            var dayPeriod = _schedulePart.DateOnlyAsPeriod.Period();
            if (period.EndDateTime>dayPeriod.EndDateTime && period.StartDateTime<dayPeriod.EndDateTime)
            {
                yield return new DateTimePeriod(period.StartDateTime,dayPeriod.EndDateTime);
                yield return new DateTimePeriod(dayPeriod.EndDateTime,period.EndDateTime);
            }
            else
            {
                yield return period;   
            }
        }
    }
}
