namespace Teleopti.Interfaces.Domain
{
	public interface IShiftLayer : IAggregateEntity, ILayer<IActivity>
	{
		IShiftLayer EntityClone();
	}
}