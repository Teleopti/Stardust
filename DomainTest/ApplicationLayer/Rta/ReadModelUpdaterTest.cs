using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta
{
	public class ReadModelUpdaterTest : DomainTestAttribute
	{
		protected override FakeToggleManager Toggles()
		{
			var toggles = base.Toggles();
			toggles.Enable(Domain.FeatureFlags.Toggles.RTA_AdherenceDetails_34267);
			return toggles;
		}
	}

}