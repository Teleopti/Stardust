using System.Collections.Generic;

namespace Teleopti.Ccc.OnlineReporting.Testing
{
    public class Report
    {
        private readonly string _rdlcFile;
        private readonly string _name;

        public Report(string rdlcFile, string name)
        {
            _rdlcFile = rdlcFile;
            _name = name;
        }
        public string RdlcFile
        {
            get { return _rdlcFile; }
        }

        public string Name
        {
            get { return _name; }
        }

        public static IList<Report> GetAllReports()
        {
            return new List<Report>{new Report("report_scheduled_time_per_activity.rdlc", "Schemalagd tid per aktivitet")};
        }
    }
}
