using System;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    /// <summary>
    /// Represents a SessionDataDto object.
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/")]
    [Serializable]
    public class SessionDataDto : Dto
    {
        /// <summary>
        /// Gets or sets the type of the authentication.
        /// </summary>
        /// <value>The type of the authentication.</value>
        [DataMember]
        public AuthenticationTypeOptionDto AuthenticationType { get; set; }

        /// <summary>
        /// Gets or sets the logged on person.
        /// </summary>
        /// <value>The logged on person.</value>
        [DataMember]
        public PersonDto LoggedOnPerson { get; set; }

        /// <summary>
        /// Gets or sets the data source.
        /// </summary>
        /// <value>The data source.</value>
        [DataMember]
        public DataSourceDto DataSource { get; set; }

        /// <summary>
        /// Gets or sets the business unit.
        /// </summary>
        /// <value>The business unit.</value>
        [DataMember]
        public BusinessUnitDto BusinessUnit { get; set; }

        /// <summary>
        /// Gets or sets the logged on password.
        /// </summary>
        /// <value>The logged on password.</value>
        [DataMember]
        public string LoggedOnPassword { get; set; }
    }
}