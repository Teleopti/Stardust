using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands
{
	/// <summary>
	/// Command for adding a schedule changes listener 
	/// </summary>
	[DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2016/03/")]
	public class AddScheduleChangesListenerCommandDto : CommandDto
	{
		/// <summary>
		/// Gets or sets the listener to add to the schedule changes subscriptions.
		/// </summary>
		[DataMember]
		public ScheduleChangesListenerDto Listener { get; set; }
	}
}