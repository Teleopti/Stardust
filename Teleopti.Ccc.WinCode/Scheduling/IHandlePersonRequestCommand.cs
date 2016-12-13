using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public interface IHandlePersonRequestCommand : IExecutableCommand
	{
		PersonRequestViewModel Model { get; set; }
	}
}