using System.Collections.Generic;

namespace Teleopti.Ccc.Win.Reporting
{
    public class ReportDataPackage<T>
    {
        public ReportDataPackage(IDictionary<string, IList<T>> data, IList<IReportDataParameter> parameters, bool limitReached)
        {
            Data = data;
            Parameters = parameters;
	        LimitReached = limitReached;
        }

	    public bool LimitReached { get; private set; }
	    public IDictionary<string, IList<T>> Data { get; private set; }
        public IList<IReportDataParameter> Parameters { get; private set; }
    }
}