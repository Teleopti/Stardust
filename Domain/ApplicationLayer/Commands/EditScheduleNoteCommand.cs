using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class EditScheduleNoteCommand: ITrackableCommand
	{
		public Guid PersonId { get; set; }
		public DateOnly Date { get; set; }
		public string InternalNote { get; set; }
		public string PublicNote { get; set; }
		public TrackedCommandInfo TrackedCommandInfo { get; set; }
		public IList<string> ErrorMessages { get; set; }
	}
}