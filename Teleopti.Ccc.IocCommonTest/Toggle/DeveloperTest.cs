namespace Teleopti.Ccc.IocCommonTest.Toggle
{
	public class DeveloperTest : ToggleBaseTest
	{
		protected override bool UndefinedFeatureShouldBe => true;

		protected override bool EnabledFeatureShouldBe => true;

		protected override bool DisabledFeatureShouldBe => false;

		protected override bool RcFeatureShouldBe => true;

		protected override string ToggleMode => " AlL	";
	}
}