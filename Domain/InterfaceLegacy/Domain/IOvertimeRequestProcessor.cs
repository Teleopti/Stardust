namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IOvertimeRequestProcessor
	{
		void Process(IPersonRequest personRequest, bool isAutoGrant);
		void CheckAndProcessDeny(IPersonRequest personRequest);
	}
}