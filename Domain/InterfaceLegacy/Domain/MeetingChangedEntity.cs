using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	///<summary>
	/// Extra details about a changed part of a meeting.
	///</summary>
	public interface IMeetingChangedEntity : ICustomChangedEntity
	{
	}

	/// <summary>
	/// Extra details about a changed part of a meeting.
	/// </summary>
	public class MeetingChangedEntity : IMeetingChangedEntity
	{
		/// <summary>
		/// Gets or sets the period this part contains details for.
		/// </summary>
		public DateTimePeriod Period { get; set; }

		/// <summary>
		/// Gets or sets the main root reference (preferrably a <see cref="IPerson"/>).
		/// </summary>
		public IAggregateRoot MainRoot { get; set; }

		/// <summary>
		/// Gets or sets the id associated with this change. (The id of the <see cref="IMeeting"/>.)
		/// </summary>
		public Guid? Id { get; set; }
	}
}