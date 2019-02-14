using Teleopti.Ccc.Domain.MonitorSystem;

namespace Teleopti.Ccc.DomainTest.MonitorSystemTest
{
	public class FakeCallLegacySystemStatus : ICallLegacySystemStatus
	{
		private bool _returnValue = true;
		
		public void WillFail()
		{
			_returnValue = false;
		}
		
		public bool Execute()
		{
			return _returnValue;
		}
	}
}