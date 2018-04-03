namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IOvertimeRequestProcessor
	{
		int StaffingDataAvailableDays { get; set; }
		void Process(IPersonRequest personRequest);
	}
}