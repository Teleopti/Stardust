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
        private readonly bool _isDayOff;
        private readonly string _dayOffName = string.Empty;
        private readonly string _publicNote;
	    private readonly string _scheduleTag;

	    public VisualProjection(string agentName, IList<VisualLayer> layerCollection, bool isDayOff, string dayOffName, string publicNote, string scheduleTag)
        {
            _publicNote = publicNote;
	        _scheduleTag = scheduleTag;
	        _agentName = agentName;
            _layerCollection = layerCollection;
            _isDayOff = isDayOff;
            _dayOffName = dayOffName;
        }

        public string AgentName
        {
            get { return _agentName; }
        }

        public ReadOnlyCollection<VisualLayer> LayerCollection
        {
            get { return new ReadOnlyCollection<VisualLayer>(_layerCollection); }
        }

        public bool IsDayOff
        {
            get { return _isDayOff; }
        }

        public string DayOffName
        {
            get { return _dayOffName; }
        }

        public TimePeriod? Period()
        {
            if(LayerCollection.Count == 0)
                return null;

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

        public string PublicNote
        {
            get { return _publicNote; }
        }

		public string ScheduleTag
		{
			get { return _scheduleTag; }
		}
    }
}
