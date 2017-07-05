using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests
{
	public class OvertimeRequestProcessor:IOvertimeRequestProcessor
	{
		public void Process(IPersonRequest personRequest)
		{
				personRequest.Pending();
		}
	}
}