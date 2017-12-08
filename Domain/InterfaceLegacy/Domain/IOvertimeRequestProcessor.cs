namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IOvertimeRequestProcessor
	{
		int StaffingDataAvailableDays { get; set; }
		void Process(IPersonRequest personRequest, bool isAutoGrant);
		void Process(IPersonRequest personRequest);
		void CheckAndProcessDeny(IPersonRequest personRequest);
	}
}