using System;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands
{
	/// <summary>
	/// A command that sets the worktime on a schedule period. 
	/// </summary>
	[DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2013/03/")]
	public class SetSchedulePeriodWorktimeOverrideCommandDto : CommandDto
	{
		/// <summary>
		/// Set the Work Time on the Person with this Id.
		/// </summary>
		[DataMember]
		public Guid PersonId { get; set; }

		/// <summary>
		/// Set the Work Time on the Schedule Period belonging to this date.
		/// </summary>
		[DataMember]
		public DateOnlyDto Date { get; set; }

		/// <summary>
		/// The Work Time to set on the Period.
		/// </summary>
		[DataMember]
		public double PeriodTimeInMinutes { get; set; }
	}
}