namespace Teleopti.Interfaces.Domain
{
	public interface IOvertimeShiftLayer : IShiftLayer
	{
		IMultiplicatorDefinitionSet DefinitionSet { get; }
	}
}