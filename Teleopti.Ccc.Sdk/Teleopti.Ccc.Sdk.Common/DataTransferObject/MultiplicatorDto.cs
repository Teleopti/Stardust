using System.Runtime.Serialization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    /// <summary>
    /// Represents a MultiplicatorDto object.
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/")]
    public class MultiplicatorDto : Dto
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MultiplicatorDto"/> class.
        /// </summary>
        /// <param name="multiplicator">The multiplicator.</param>
        public MultiplicatorDto(IMultiplicator multiplicator)
        {
            if (multiplicator != null)
            {
                Id = multiplicator.Id;
                PayrollCode = multiplicator.ExportCode;
                MultiplicatorType = (MultiplicatorTypeDto) multiplicator.MultiplicatorType;
                Name = multiplicator.Description.Name;
                Multiplicator = multiplicator.MultiplicatorValue;
                Color = new ColorDto(multiplicator.DisplayColor);
            }
        }

        /// <summary>
        /// Gets or sets the color.
        /// </summary>
        /// <value>The color.</value>
        [DataMember]
        public ColorDto Color { get; protected set; }

        /// <summary>
        /// Gets or sets the multiplicator.
        /// </summary>
        /// <value>The multiplicator.</value>
        [DataMember]
        public double Multiplicator { get; protected set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        [DataMember]
        public string Name { get; protected set; }

        /// <summary>
        /// Gets or sets the type of the multiplicator.
        /// </summary>
        /// <value>The type of the multiplicator.</value>
        [DataMember]
        public MultiplicatorTypeDto MultiplicatorType { get; protected set; }

        /// <summary>
        /// Gets or sets the payroll code.
        /// </summary>
        /// <value>The payroll code.</value>
        [DataMember]
        public string PayrollCode { get; set; }
    }
}