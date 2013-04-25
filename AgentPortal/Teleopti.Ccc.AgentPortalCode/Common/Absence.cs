using System;
using System.Drawing;

namespace Teleopti.Ccc.AgentPortalCode.Common
{
    public class Absence
    {
        private readonly Color _color;
        private readonly string _name;
        private readonly string _shortName;
        private readonly Guid? _id;

        public Absence(string name, string shortName, Guid? id, Color color)
        {
            _color = color;
            _name = name;
            _shortName = shortName;
            _id = id;
        }

        public string Name
        {
            get { return _name; }
        }

        public string ShortName
        {
            get { return _shortName; }
        }

        public Guid? Id
        {
            get { return _id; }
        }

        public Color Color
        {
            get { return _color; }
        }
    }
}
