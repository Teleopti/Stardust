namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IOvertimeShiftLayer : IShiftLayer
	{
		IMultiplicatorDefinitionSet DefinitionSet { get; }
	}
}