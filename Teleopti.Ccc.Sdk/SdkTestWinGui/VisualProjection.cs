using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace GridTest
{
    public class VisualProjection
    {
        private readonly string _agentName;
        private readonly IList<VisualLayer> _layerCollection;

        public VisualProjection(string agentName, IList<VisualLayer>  layerCollection)
        {
            _agentName = agentName;
            _layerCollection = layerCollection;
        }

        public string AgentName
        {
            get { return _agentName; }
        }

        public ReadOnlyCollection<VisualLayer> LayerCollection
        {
            get { return new ReadOnlyCollection<VisualLayer>(_layerCollection); }
        }

        public TimePeriod Period()
        {
            TimeSpan min = TimeSpan.MaxValue;
            TimeSpan max = TimeSpan.MinValue;
            foreach (VisualLayer layer in LayerCollection)
            {
                if (layer.Period.StartTime < min)
                    min = layer.Period.StartTime;

                if (layer.Period.EndTime > max)
                    max = layer.Period.EndTime;
            }

            return new TimePeriod(min, max);
        }
    }
}
