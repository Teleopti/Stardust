using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class AddDayOffCommand : ITrackableCommand, IErrorAttachedCommand
	{
		public TrackedCommandInfo TrackedCommandInfo { get; set; }
		public IList<string> ErrorMessages { get; set; }
		public DateOnly StartDate { get; set; }
		public IPerson Person { get; set; }
		public DateOnly EndDate { get; set; }
		public IDayOffTemplate Template { get; set; }
	}
}