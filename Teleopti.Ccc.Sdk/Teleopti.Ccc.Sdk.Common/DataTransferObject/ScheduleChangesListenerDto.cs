using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
	/// <summary>
	/// Configuration for one listener for schedule changes.
	/// </summary>
	[DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2016/03/")]
	public class ScheduleChangesListenerDto
	{
		/// <summary>
		/// The callback url that listens to the schedule changes.
		/// </summary>
		[DataMember]
		public string Url { get; set; }

		/// <summary>
		/// The days offset from the current date to listen for changes.
		/// </summary>
		/// <example>-2 will give you changes from the current date minues 2 days.</example>
		/// <remarks>This value must be smaller than or equal to <see cref="DaysEndFromCurrentDate"/></remarks>
		[DataMember]
		public int DaysStartFromCurrentDate { get; set; }

		/// <summary>
		/// The days offset from the current date to listen for changes.
		/// </summary>
		/// <example>2 will give you changes from the current date plus 2 days.</example>
		/// <remarks>This value must be greater than or equal to <see cref="DaysStartFromCurrentDate"/></remarks>
		[DataMember]
		public int DaysEndFromCurrentDate { get; set; }

		/// <summary>
		/// The mandatory name of this listener 
		/// </summary>
		[DataMember]
		public string Name { get; set; }
	}
}