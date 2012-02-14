using System;
using System.Runtime.Serialization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    /// <summary>
    /// Represents BusinessUnitDto object.
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/")]
    [Serializable]
    public class BusinessUnitDto : Dto
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BusinessUnitDto"/> class.
        /// </summary>
        /// <param name="businessUnit">The business unit.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public BusinessUnitDto(IBusinessUnit businessUnit)
        {
            Name = businessUnit.Name;
            Id = businessUnit.Id;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BusinessUnitDto"/> class.
        /// </summary>
        public BusinessUnitDto()
        {
        }
    }
}