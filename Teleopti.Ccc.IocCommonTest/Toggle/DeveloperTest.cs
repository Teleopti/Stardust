using Teleopti.Ccc.IocCommon.Toggle;

namespace Teleopti.Ccc.IocCommonTest.Toggle
{
	public class DeveloperTest : ToggleBaseTest
	{
		protected override bool UndefinedFeatureShouldBe
		{
			get { return true; }
		}

		protected override bool EnabledFeatureShouldBe
		{
			get { return true; }
		}

		protected override bool DisabledFeatureShouldBe
		{
			get { return false; }
		}

		protected override bool RcFeatureShouldBe
		{
			get { return true; }
		}

		protected override string LicenseCustomerName
		{
			get { return ToggleNetModule.DeveloperLicenseName; }
		}
	}
}