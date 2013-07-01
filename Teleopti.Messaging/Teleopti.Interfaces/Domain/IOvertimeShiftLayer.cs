namespace Teleopti.Interfaces.Domain
{
	public interface IOvertimeShiftLayer : IAggregateEntity, ILayer<IActivity>
	{
		IMultiplicatorDefinitionSet DefinitionSet { get; }
	}
}