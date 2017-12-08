using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests
{
	public class OvertimeRequestValidationContext
	{
		public OvertimeRequestValidationContext(IPersonRequest personRequest)
		{
			PersonRequest = personRequest;
		}

		public IPersonRequest PersonRequest { get; }

		public int StaffingDataAvailableDays { get; set; }
	}
}
