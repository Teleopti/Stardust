using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling
{
	public interface IHandlePersonRequestCommand : IExecutableCommand
	{
		PersonRequestViewModel Model { get; set; }
	}
}