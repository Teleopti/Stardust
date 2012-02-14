using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.AgentPortalCode.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.AgentPortal.Reports
{
    public class ScheduleAdherence
    {
        private readonly VisualProjection _visualProjection;
        private readonly IList<AdherenceLayer> _adherenceCollection;

        public ScheduleAdherence(VisualProjection visualProjection, IList<AdherenceLayer> adherenceCollection)
        {
            _visualProjection = visualProjection;
            _adherenceCollection = adherenceCollection;
        }

        public VisualProjection VisualProjection
        {
            get { return _visualProjection; }
        }

        public ReadOnlyCollection<AdherenceLayer> AdherenceLayerCollection
        {
            get { return new ReadOnlyCollection<AdherenceLayer>(_adherenceCollection); }
        }

        public TimePeriod? Period()
        {
            TimeSpan min = TimeSpan.MaxValue;
            TimeSpan max = TimeSpan.MinValue;
            TimePeriod? thisPeriod = null;
            if (_adherenceCollection.Count > 0)
            {
                foreach (AdherenceLayer layer in _adherenceCollection)
                {
                    if (layer.Period.StartTime < min)
                        min = layer.Period.StartTime;

                    if (layer.Period.EndTime > max)
                        max = layer.Period.EndTime;
                }
                thisPeriod = new TimePeriod(min, max);
            }

            if (_visualProjection.Period().HasValue)
            {
                if (thisPeriod.HasValue)
                {
                    min =
                        new TimeSpan(Math.Min(_visualProjection.Period().Value.StartTime.Ticks,
                                              thisPeriod.Value.StartTime.Ticks));
                    max =
                        new TimeSpan(Math.Max(_visualProjection.Period().Value.EndTime.Ticks,
                                              thisPeriod.Value.EndTime.Ticks));
                    return new TimePeriod(min, max);
                }

                return _visualProjection.Period();

            }
            return thisPeriod;
        }
    }
}
