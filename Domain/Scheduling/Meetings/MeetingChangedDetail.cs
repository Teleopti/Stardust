using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Meetings
{
	/// <summary>
	/// Detailed information about changes for a meeting.
	/// </summary>
	public class MeetingChangedDetail : IRootChangeInfo
	{
		/// <summary>
		/// Creates an instance of the <see cref="MeetingChangedDetail"/>
		/// </summary>
		/// <param name="root">The extra details of the root.</param>
		/// <param name="status">The status of the update (can differ from the status for the root object).</param>
		public MeetingChangedDetail(ICustomChangedEntity root, DomainUpdateType status)
		{
			Root = root;
			Status = status;
		}

		/// <summary>
		/// Gets the extra details.
		/// </summary>
		public object Root { get; private set; }

		/// <summary>
		/// Gets the updates status.
		/// </summary>
		public DomainUpdateType Status { get; private set; }
	}
}