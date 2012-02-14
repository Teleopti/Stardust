using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Security.AuthorizationEntities
{
    /// <summary>
    /// Matrix report data imitation.
    /// </summary>
    public class MatrixReportInfo
    {
        private int _reportId;
        private string _reportName;
        private string _reportUrl;
        private string _targetFrame;

        /// <summary>
        /// Parameterles constructor
        /// </summary>
        public MatrixReportInfo()
        {
            //to satisfy  Hibernate
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MatrixReportInfo"/> class.
        /// </summary>
        /// <param name="reportId">The report id.</param>
        /// <param name="reportName">Name of the report.</param>
        public MatrixReportInfo(int reportId, string reportName)
        {
            _reportId = reportId;
            _reportName = reportName;
        }

        /// <summary>
        /// Finds the application function in the list by matrix report id.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public static MatrixReportInfo FindByReportId(IEnumerable<MatrixReportInfo> list, int id)
        {
            foreach (MatrixReportInfo info in list)
            {
                if (info.ReportId == id)
                    return info;
            }
            return null;
        }

        /// <summary>
        /// Gets or sets the report id.
        /// </summary>
        /// <value>The report id.</value>
        public int ReportId
        {
            get { return _reportId; }
            set { _reportId = value; }
        }

        /// <summary>
        /// Gets or sets the name of the report.
        /// </summary>
        /// <value>The name of the report.</value>
        public string ReportName
        {
            get { return _reportName; }
            set { _reportName = value; }
        }

        /// <summary>
        /// Gets or sets the report URL.
        /// </summary>
        /// <value>The report URL.</value>
        public string ReportUrl
        {
            get { return _reportUrl; }
            set { _reportUrl = value; }
        }

        /// <summary>
        /// Gets or sets the target frame.
        /// </summary>
        /// <value>The target frame.</value>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2010-03-05
        /// </remarks>
        public string TargetFrame
        {
            get { return _targetFrame; }
            set { _targetFrame = value; }
        }
    }
}
