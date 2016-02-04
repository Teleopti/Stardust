using System;

namespace Teleopti.Interfaces.Messages.Requests
{
	public class NewAbsenceReportCreated : MessageWithLogOnContext
	{
		/// <summary>
		/// Message to inform consumers that a new absence report has been created
		/// </summary>
		/// <remarks>
		/// Created by: chundanx
		/// Created date: 2014-11-11
		/// </remarks>

		/// <summary>
		/// Gets or sets the absence id.
		/// </summary>
		/// <value>The reported absence id.</value>
		/// <remarks>
		/// Created by: chundanx
		/// Created date: 2014-11-11
		/// </remarks>
		public Guid AbsenceId { get; set; }
		public DateTime RequestedDate { get; set; }

		/// <summary>
		/// Identity for this message
		/// </summary>
		public override Guid Identity
		{
			get { return AbsenceId; }
		}

		public Guid PersonId { get; set; }
	}
}