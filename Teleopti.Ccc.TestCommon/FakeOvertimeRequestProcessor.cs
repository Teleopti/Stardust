using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeOvertimeRequestProcessor : IOvertimeRequestProcessor
	{
		public void Process(IPersonRequest personRequest, bool isAutoGrant)
		{
			personRequest.Pending();
		}
	}
}