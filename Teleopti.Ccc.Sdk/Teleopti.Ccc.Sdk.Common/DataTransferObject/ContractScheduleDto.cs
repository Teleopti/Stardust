using System;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    /// <summary>
    /// The contract schedule.
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/05/")]
    public class ContractScheduleDto : Dto
    {
        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the deleted flag.
        /// </summary>
        /// <remarks>When loading contract schedules from the SDK this flag indicates that the contract schedule should not be used anymore.</remarks>
        [DataMember]
        public bool IsDeleted { get; set; }

		/// <summary>
		/// The detailed information on the contract schedule working days.
		/// </summary>
		[DataMember(IsRequired = false,Order = 1)]
		public ContractScheduleWeekDto[] Weeks { get; set; }
    }

	/// <summary>
	/// Detailed information on the contract schedule week
	/// </summary>
	[DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2016/01/")]
	public class ContractScheduleWeekDto : Dto
	{
		/// <summary>
		/// The number of the week compared to other weeks in this contract schedule.
		/// </summary>
		[DataMember]
		public int WeekNumber { get; set; }

		/// <summary>
		/// The days marked as working days for this week in the contract schedule.
		/// </summary>
		[DataMember]
		public DayOfWeek[] WorkingDays { get; set; }
	}
}