using System;

namespace Teleopti.Ccc.WinCode.Shifts
{
    public class GridClassType
    {
        private string _name;
        private Type _classType;

        public GridClassType(string name, Type classType)
        {
            Name = name;
            ClassType = classType;
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public Type ClassType
        {
            get { return _classType; }
            set { _classType = value; }
        }
    }
}
