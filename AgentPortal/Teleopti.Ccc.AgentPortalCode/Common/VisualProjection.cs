using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Sdk.Client.SdkServiceReference;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.AgentPortalCode.Common
{
    public class VisualProjection
    {
        private readonly IList<ActivityVisualLayer> _layerCollection;
        private readonly string _dayOffName;
        private readonly bool _isDayOff;
        private readonly PersonDto _person;

        public VisualProjection(PersonDto person, IList<ActivityVisualLayer> layerCollection, string dayOffName, bool isDayOff)
        {
            _person = person;
            _layerCollection = layerCollection;
            _dayOffName = dayOffName;
            _isDayOff = isDayOff;
        }

        public string AgentName
        {
            get { return Person.Name; }
        }

        public ReadOnlyCollection<ActivityVisualLayer> LayerCollection
        {
            get { return new ReadOnlyCollection<ActivityVisualLayer>(_layerCollection); }
        }

        public string DayOffName
        {
            get { return _dayOffName; }
        }

        public bool IsDayOff
        {
            get { return _isDayOff; }
        }

        public PersonDto Person
        {
            get { return _person; }
        }

        public TimeSpan ScheduleStartTime
        {
            get
            {
                var timePeriod = Period();
                return !timePeriod.HasValue ? TimeSpan.MaxValue : timePeriod.Value.StartTime;
            }
        }

        public TimePeriod? Period()
        {
            TimeSpan min = TimeSpan.MaxValue;
            TimeSpan max = TimeSpan.MinValue;
            foreach (ActivityVisualLayer layer in LayerCollection)
            {
                if (layer.Period.StartTime < min)
                    min = layer.Period.StartTime;

                if (layer.Period.EndTime > max)
                    max = layer.Period.EndTime;
            }
            if (LayerCollection.Count == 0)
                return null;
            return new TimePeriod(min, max);
        }
    }
}