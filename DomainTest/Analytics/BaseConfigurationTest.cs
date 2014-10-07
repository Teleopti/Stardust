using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Analytics;

namespace Teleopti.Ccc.DomainTest.Analytics
{
	[TestFixture]
	public class BaseConfigurationTest
	{
		private BaseConfiguration _target;

		[Test]
		public void ShouldBeAbleToHavePropertiesSetToNull()
		{
			
			_target = new BaseConfiguration(null, null, null, null);

			_target.CultureId.HasValue.Should().Be.False();
			_target.IntervalLength.HasValue.Should().Be.False();
			_target.TimeZoneCode.Should().Be.Null();
			_target.EtlToggleManager.Should().Be.Null();
		}

		[Test]
		public void ShouldBeAbleToHavePropertiesSetToValues()
		{
			_target = new BaseConfiguration(1053, 15, "UTC", new EtlToggleManager());

			_target.CultureId.HasValue.Should().Be.True();
			_target.IntervalLength.HasValue.Should().Be.True();
			_target.TimeZoneCode.Should().Not.Be.Null();

			_target.CultureId.Should().Be.EqualTo(1053);
			_target.IntervalLength.Should().Be.EqualTo(15);
			_target.TimeZoneCode.Should().Be.EqualTo("UTC");
			_target.EtlToggleManager.IsEnabled("dummyToggle").Should().Be.False();
		}

		[Test]
		public void ShouldBeAbleToHaveTrueToggle()
		{
			var toggleManager = new EtlToggleManager();
			toggleManager.AddToggle("trueThing", true);
			_target = new BaseConfiguration(1053, 15, "UTC", toggleManager);
			_target.EtlToggleManager.IsEnabled("trueThing").Should().Be.True();
	}
}
}
