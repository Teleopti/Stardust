using System.Net;
using Teleopti.Ccc.Domain.MonitorSystem;

namespace Teleopti.Ccc.DomainTest.MonitorSystemTest
{
	public class FakeCallLegacySystemStatus : ICallLegacySystemStatus
	{
		private HttpStatusCode _code = HttpStatusCode.OK;
		
		public void SetReturnValue(HttpStatusCode code)
		{
			_code = code;
		}
		
		public HttpStatusCode Execute()
		{
			return _code;
		}
	}
}