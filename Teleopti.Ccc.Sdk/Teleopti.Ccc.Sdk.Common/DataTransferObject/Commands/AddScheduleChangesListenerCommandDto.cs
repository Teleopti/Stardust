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
		public ScheduleChangesListenerDto Listener { get; set; }
	}

	/// <summary>
	/// Command for revoking a schedule changes listener 
	/// </summary>
	[DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2016/03/")]
	public class RevokeScheduleChangesListenerCommandDto : CommandDto
	{
		/// <summary>
		/// Gets or sets the name of the listener to revoke the schedule changes subscription for.
		/// </summary>
		public string ListenerName { get; set; }
	}
}