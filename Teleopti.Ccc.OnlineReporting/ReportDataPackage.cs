using System.Collections.Generic;

namespace Teleopti.Ccc.OnlineReporting
{
    public class ReportDataPackage<T>
    {
        public ReportDataPackage(IDictionary<string, IList<T>> data, IList<IReportDataParameter> parameters)
        {
            Data = data;
            Parameters = parameters;
        }

        public IDictionary<string, IList<T>> Data { get; private set; }
        public IList<IReportDataParameter> Parameters { get; private set; }
    }
}