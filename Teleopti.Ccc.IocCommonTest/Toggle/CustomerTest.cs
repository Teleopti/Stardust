namespace Teleopti.Ccc.IocCommonTest.Toggle
{
	public class CustomerTest : ToggleBaseTest
	{
		protected override bool UndefinedFeatureShouldBe => false;

		protected override bool EnabledFeatureShouldBe => true;

		protected override bool DisabledFeatureShouldBe => false;

		protected override bool RcFeatureShouldBe => false;

		protected override string ToggleMode => "someCustomer";
	}
}