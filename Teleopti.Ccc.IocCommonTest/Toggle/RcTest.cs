using Teleopti.Ccc.IocCommon.Toggle;

namespace Teleopti.Ccc.IocCommonTest.Toggle
{
	public class RcTest : ToggleBaseTest
	{
		protected override bool UndefinedFeatureShouldBe
		{
			get { return false; }
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
			get { return ToggleNetModule.RcLicenseName; }
		}
	}
}