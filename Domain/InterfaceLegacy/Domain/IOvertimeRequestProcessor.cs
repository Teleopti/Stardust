namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IOvertimeRequestProcessor
	{
		void Process(IPersonRequest personRequest, bool isAutoGrant);
		void Process(IPersonRequest personRequest);
		void CheckAndProcessDeny(IPersonRequest personRequest);
	}
}