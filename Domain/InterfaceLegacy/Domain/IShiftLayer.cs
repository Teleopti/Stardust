namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IShiftLayer : IAggregateEntity, ILayer<IActivity>
	{
		IShiftLayer EntityClone();
	}
}