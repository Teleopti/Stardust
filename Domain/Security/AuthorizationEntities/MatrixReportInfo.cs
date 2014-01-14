using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Security.AuthorizationEntities
{
    /// <summary>
    /// Matrix report data imitation.
    /// </summary>
    public class MatrixReportInfo
    {
        private Guid _reportId;
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

        public MatrixReportInfo(Guid reportId, string reportName)
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public static MatrixReportInfo FindByReportId(IEnumerable<MatrixReportInfo> list, Guid id)
        {
            foreach (MatrixReportInfo info in list)
            {
                if (info.ReportId.Equals(id))
                    return info;
            }
            return null;
        }

        /// <summary>
        /// Gets or sets the report id.
        /// </summary>
        /// <value>The report id.</value>
        public Guid ReportId
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

	    public string Version { get; set; }
    }
}
