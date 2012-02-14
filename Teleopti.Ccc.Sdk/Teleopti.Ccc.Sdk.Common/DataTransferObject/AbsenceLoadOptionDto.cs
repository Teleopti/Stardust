using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    /// <summary>
    /// Load options for absence
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/")]
    public class AbsenceLoadOptionDto : LoadOptionDto
    {
        /// <summary>
        /// Gets or sets a value indicating whether absences set as requestable should be loaded.
        /// </summary>
        /// <value><c>true</c> if absences set as requestable should be loaded; otherwise, <c>false</c>.</value>
        [DataMember]
        public bool LoadRequestable { get; set; }
    }
}
