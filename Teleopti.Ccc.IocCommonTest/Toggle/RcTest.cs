namespace Teleopti.Ccc.IocCommonTest.Toggle
{
	public class RcTest : ToggleBaseTest
	{
		protected override bool UndefinedFeatureShouldBe => false;

		protected override bool EnabledFeatureShouldBe => true;

		protected override bool DisabledFeatureShouldBe => false;

		protected override bool RcFeatureShouldBe => true;

		protected override string ToggleMode => " Rc ";
	}
}