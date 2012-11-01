using System;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    /// <summary>
    /// Represents a DateOnlyDto object.
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/")]
    [Serializable]
    public class DateOnlyDto
	{
		private DateTime _dateTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="DateOnlyDto"/> class.
        /// </summary>
        public DateOnlyDto()
        {}

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