using System;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    /// <summary>
    /// Represents a person day off, the actual day off in the schedule.
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/")]
    public class PersonDayOffDto : LayerDto
    {
        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        /// <remarks>For optimistic lock check.</remarks>
        [DataMember]
        public int Version { get; set; }

        /// <summary>
        /// Gets or sets the person.
        /// </summary>
        /// <value>The person.</value>
        [DataMember]
        public PersonDto Person { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the short name.
        /// </summary>
        /// <value>The short name.</value>
        [DataMember]
        public string ShortName { get; set; }

        /// <summary>
        /// Gets or sets the anchor time, the time where the day off is anchored.
        /// </summary>
        /// <value>The anchor time.</value>
        [DataMember]
        public TimeSpan AnchorTime { get; set; }

        /// <summary>
        /// Gets or sets the anchor point as date and time.
        /// </summary>
        /// <value>The anchor.</value>
        [DataMember]
        public DateTime Anchor { get; set; }

        /// <summary>
        /// Gets or sets the flexibility, how much the day off can be moved.
        /// </summary>
        /// <value>The flexibility.</value>
        [DataMember]
        public TimeSpan Flexibility { get; set; }

        /// <summary>
        /// Gets or sets the length.
        /// </summary>
        /// <value>The length.</value>
        [DataMember]
        public TimeSpan Length { get; set; }

        /// <summary>
        /// Gets or sets the color.
        /// </summary>
        /// <value>The color.</value>
        [DataMember]
        public ColorDto Color { get; set; }

		/// <summary>
		/// Gets or sets the payroll code.
		/// </summary>
		/// <value>The payroll code.</value>
		[DataMember(IsRequired = false, Order = 1)]
		public string PayrollCode { get; set; }
    }
}