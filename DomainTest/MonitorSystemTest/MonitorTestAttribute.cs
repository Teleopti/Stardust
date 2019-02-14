using Teleopti.Ccc.Domain.MonitorSystem;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.MonitorSystemTest
{
	public class MonitorTestAttribute : DomainTestAttribute
	{
		protected override void Isolate(IIsolate isolate)
		{
			base.Isolate(isolate);
			
			isolate.UseTestDouble<FakeCallLegacySystemStatus>().For<ICallLegacySystemStatus>();
		}
	}
}