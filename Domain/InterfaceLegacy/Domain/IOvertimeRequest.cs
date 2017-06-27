namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IOvertimeRequest : IRequest
	{
		IActivity Activity { get; }

		OvertimeType OvertimeType { get; }
	}
}