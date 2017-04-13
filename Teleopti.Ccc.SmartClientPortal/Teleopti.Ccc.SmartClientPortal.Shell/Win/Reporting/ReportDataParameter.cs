
namespace Teleopti.Ccc.Win.Reporting
{
    public class ReportDataParameter :IReportDataParameter
    {
        private string _name;
        private string _value;
        public ReportDataParameter(string name, string value)
        {
            _name = name;
            _value = value;
        }
        public string Name
        {
            get { return _name; }
        }

        public string Value
        {
            get { return _value; }
        }
    }
}
