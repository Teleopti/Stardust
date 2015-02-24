using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    /// <summary>
    /// Gives the client connecting feedback from the authentication process
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/08/")]
    public class AuthenticationResultDto : IExtensibleDataObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationResultDto"/> class.
        /// </summary>
        public AuthenticationResultDto()
        {
            BusinessUnitCollection = new List<BusinessUnitDto>();
        }

		/// <summary>
		/// Internal data for version compatibility.
		/// </summary>
        public ExtensionDataObject ExtensionData
        {
            get;
            set;
        }

        /// <summary>
        /// Indicates that this instance contains a message.
        /// </summary>
        [DataMember]
        public bool HasMessage { get; set; }

        /// <summary>
        /// Gets or sets the result of the authentication.
        /// </summary>
        [DataMember]
        public bool Successful { get; set; }

        /// <summary>
        /// The message.
        /// </summary>
        /// <remarks>The message is translated to the language of the SDK user. One example of a message is warning for upcoming expiration of passwords if password policy is used.</remarks>
        [DataMember]
        public string Message { get; set; }

        /// <summary>
        /// A list of the available business units.
        /// </summary>
        [DataMember]
        public ICollection<BusinessUnitDto> BusinessUnitCollection { get; private set; }

		  /// <summary>
		  /// The tenant.
		  /// </summary>
		  /// <remarks>The tenant should be provided in the header in further calls to the sdk</remarks>
		  [DataMember]
		  public string Tenant { get; set; }
    }
}
