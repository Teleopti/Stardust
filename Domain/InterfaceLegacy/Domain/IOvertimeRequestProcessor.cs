namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IOvertimeRequestProcessor
	{
		void Process(IPersonRequest personRequest, bool isAutoGrant);
		bool CheckAndProcessDeny(IPersonRequest personRequest);
	}
}