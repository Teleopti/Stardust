namespace Teleopti.Interfaces.Domain
{
	public interface IMainShiftActivityLayerNew : IAggregateEntity, ICloneableEntity<IMainShiftActivityLayerNew>
	{
		IActivity Payload { get; }
		DateTimePeriod Period { get; }
		int OrderIndex { get; }
	}
}