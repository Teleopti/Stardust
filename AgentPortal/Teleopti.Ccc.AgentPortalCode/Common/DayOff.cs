using System;
using System.Drawing;

namespace Teleopti.Ccc.AgentPortalCode.Common
{
    public class DayOff
    {
        private DayOff(){}

        public DayOff(string name, string shortName, Guid? id, Color color)
            : this()
        {
            Name = name;
            ShortName = shortName;
            DisplayColor = color;
            Id = id;
        }

        public Color DisplayColor { get; private set; }

        public string Name { get; private set; }

        public string ShortName { get; private set; }

        public Guid? Id { get; private set; }
    }
}