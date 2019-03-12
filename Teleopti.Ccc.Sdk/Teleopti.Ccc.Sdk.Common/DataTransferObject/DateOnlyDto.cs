using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    /// <summary>
    /// Represents a DateOnlyDto object.
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/")]
    [Serializable]
	[DebuggerDisplay("{DateTime.ToShortDateString()}")]
    public class DateOnlyDto
	{
		private DateTime _dateTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="DateOnlyDto"/> class.
        /// </summary>
        public DateOnlyDto()
        {}
		
        /// <summary>
        /// Initializes a new instance of the <see cref="DateOnlyDto"/> class.
        /// </summary>
        /// <param name="dateOnly">The date only.</param>
		[Obsolete("This constructor should not be used. Use empty constructor and set date using individual parts or properties available if you need a new instance.")]
        public DateOnlyDto(DateOnly dateOnly)
        {
            DateTime = dateOnly.Date;
        }

        public DateOnlyDto(int year, int month, int day)
        {
            DateTime = new DateTime(year, month, day);
        }

    	/// <summary>
        /// Gets or sets the date time.
        /// </summary>
        /// <value>The date time.</value>
        [DataMember]
        public DateTime DateTime
    	{
    		get { return _dateTime; }
    		set { _dateTime = DateTime.SpecifyKind(value,DateTimeKind.Unspecified); }
    	}
    }
}