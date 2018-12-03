using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class RemoveDayOffCommand : ITrackableCommand, IErrorAttachedCommand
	{
		public IPerson Person { get; set; }
		public DateOnly Date { get; set; }
		public TrackedCommandInfo TrackedCommandInfo { get; set; }
		public IList<string> ErrorMessages { get; set; }
	}
}
