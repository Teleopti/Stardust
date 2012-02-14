using System.Drawing;

namespace Teleopti.Ccc.AgentPortalCode.Common
{
    public class DayOff
    {
        private readonly Color _color;
        private readonly string _name;
        private readonly string _shortName;
        private readonly string _id;

        private DayOff(){}

        public DayOff(string name, string shortName, string id, Color color)
            : this()
        {
            _name = name;
            _shortName = shortName;
            _color = color;
            _id = id;
        }

        public Color DisplayColor
        {
            get { return _color; }
        }

        public string Name
        {
            get { return _name; }
        }

        public string ShortName
        {
            get { return _shortName; }
        }

        public string Id
        {
            get { return _id; }
        }
    }
}