using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public interface ITrackableCommand
	{
		TrackedCommandInfo TrackedCommandInfo { get; set; }
	}
}