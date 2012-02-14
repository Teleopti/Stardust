using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    /// <summary>
    /// Load options when retrieving data
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/")]
    public class LoadOptionDto : IExtensibleDataObject
    {
        /// <summary>
        /// Gets or sets a value indicating whether deleted items should be loaded.
        /// </summary>
        /// <value><c>true</c> if deleted items shuold be loaded; otherwise, <c>false</c>.</value>
        [DataMember]
        public bool LoadDeleted { get; set; }

        public ExtensionDataObject ExtensionData
        {
            get; set;
        }
    }
}