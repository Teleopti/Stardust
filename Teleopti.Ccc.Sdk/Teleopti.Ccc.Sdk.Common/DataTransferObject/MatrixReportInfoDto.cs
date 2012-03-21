using System;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    /// <summary>
    /// Represents a MatrixReportInfoDto object.
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/")]
    public class MatrixReportInfoDto : Dto
    {
        /// <summary>
        /// Gets or sets the report id.
        /// </summary>
        /// <value>The report id.</value>
        [DataMember]
        [Obsolete("This will be removed, the Id is now a Guid and on the Id property")]
        public int ReportId{ get; set; }

        /// <summary>
        /// Gets or sets the name of the report.
        /// </summary>
        /// <value>The name of the report.</value>
        [DataMember]
        public string ReportName{ get; set; }

        /// <summary>
        /// Gets or sets the report URL.
        /// </summary>
        /// <value>The report URL.</value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings"), DataMember]
        public string ReportUrl{ get; set; }

        /// <summary>
        /// Gets or sets the target frame.
        /// </summary>
        /// <value>The target frame.</value>
        [DataMember]
        public string TargetFrame { get; set; }

    }
}