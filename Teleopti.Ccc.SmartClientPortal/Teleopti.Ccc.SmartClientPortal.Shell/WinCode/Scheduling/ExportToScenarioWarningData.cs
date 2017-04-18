namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling
{
    public struct ExportToScenarioWarningData
    {
        private readonly string _personName;
        private readonly string _warningInfo;

        public ExportToScenarioWarningData(string personName, string warningInfo)
        {
            _personName = personName;
            _warningInfo = warningInfo;
        }

        public string WarningInfo
        {
            get { return _warningInfo; }
        }

        public string PersonName
        {
            get { return _personName; }
        }

        public override bool Equals(object obj)
        {
            if(!(obj is ExportToScenarioWarningData))
                return false;

            ExportToScenarioWarningData casted = (ExportToScenarioWarningData)obj;
            return casted.PersonName.Equals(PersonName) && casted.WarningInfo.Equals(WarningInfo);
        }

        public override int GetHashCode()
        {
            return PersonName.GetHashCode() ^ WarningInfo.GetHashCode();
        }

        public static bool operator ==(ExportToScenarioWarningData obj1, ExportToScenarioWarningData obj2)
        {
            return obj1.Equals(obj2);
        }

        public static bool operator !=(ExportToScenarioWarningData obj1, ExportToScenarioWarningData obj2)
        {
            return !obj1.Equals(obj2);
        }
    }
}