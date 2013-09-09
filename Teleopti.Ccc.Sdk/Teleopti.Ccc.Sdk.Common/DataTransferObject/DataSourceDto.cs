using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    /// <summary>
    /// Contains the name of the data source and if it was loaded using Windows or Application log on
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/")]
    public class DataSourceDto : IExtensibleDataObject
    {
        /// <summary>
        /// Gets or sets the name of the data source.
        /// </summary>
        /// <value>The name.</value>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the authentication type option (Application or Windows).
        /// </summary>
        /// <value>The authentication type option.</value>
        [DataMember]
        public AuthenticationTypeOptionDto AuthenticationTypeOptionDto { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public DataSourceDto(AuthenticationTypeOptionDto authenticationTypeOptionDto)
        {
            AuthenticationTypeOptionDto = authenticationTypeOptionDto;
        }

        public DataSourceDto()
        {
        }

		/// <summary>
		/// Internal data for version compatibility.
		/// </summary>
        public ExtensionDataObject ExtensionData
        {
            get; set;
        }

        /// <summary>
        /// Set this and IpAddress to save logon attempts.
        /// </summary>
        [DataMember(IsRequired = false, Order = 1)]
        public string Client
        {
            get; set;
        }

        /// <summary>
        /// Set this and Client to save logon attempts.
        /// </summary>
        [DataMember(IsRequired = false, Order = 2)]
        public string IpAddress { get; set; }
    }
}