namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public interface ITrackedCommand
	{
		TrackedCommandInfo TrackedCommandInfo { get; set; }
	}
}