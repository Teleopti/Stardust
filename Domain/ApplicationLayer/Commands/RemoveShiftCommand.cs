using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class RemoveShiftCommand : ITrackableCommand, IErrorAttachedCommand
	{
		public RemoveShiftCommand()
		{
			ErrorMessages = new List<string>();
		}

		public IPerson Person { get; set; }
		public DateOnly Date { get; set; }
		public TrackedCommandInfo TrackedCommandInfo { get; set; }
		public IList<string> ErrorMessages { get; set; }
	}
}