using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class ChangeShiftCategoryCommand : ITrackableCommand, IErrorAttachedCommand
	{
		public TrackedCommandInfo TrackedCommandInfo { get; set; }
		public IList<string> ErrorMessages { get; set; }
		public Guid PersonId { get; set; }
		public DateOnly Date { get; set; }
		public Guid ShiftCategoryId { get; set; }
	}
}
