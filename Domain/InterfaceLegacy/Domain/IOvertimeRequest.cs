namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IOvertimeRequest : IRequest
	{
		IMultiplicatorDefinitionSet MultiplicatorDefinitionSet { get; }
	}
}