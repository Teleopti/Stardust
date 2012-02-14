using System.Drawing;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.AgentPortalCode.Common
{
    public class ActivityVisualLayer
    {
        public ActivityVisualLayer(TimePeriod period, Color color, string description)
        {
            Period = period;
            Color = color;
            Description = description;
        }

        public Color Color { get; private set; }

        public string Description { get; private set; }

        public TimePeriod Period { get; private set; }
    }
}