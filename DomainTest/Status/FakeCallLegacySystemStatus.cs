using Teleopti.Ccc.Domain.Status;

namespace Teleopti.Ccc.DomainTest.Status
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