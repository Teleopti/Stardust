using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Analytics.Etl.Transformer;
using Teleopti.Ccc.Domain.Analytics;

namespace Teleopti.Analytics.Etl.TransformerTest
{
	[TestFixture]
	public class BaseConfigurationTest
	{
		private BaseConfiguration _target;

		[Test]
		public void ShouldBeAbleToHavePropertiesSetToNull()
		{
			_target = new BaseConfiguration(null, null, null);

			_target.CultureId.HasValue.Should().Be.False();
			_target.IntervalLength.HasValue.Should().Be.False();
			_target.TimeZoneCode.Should().Be.Null();
		}

		[Test]
		public void ShouldBeAbleToHavePropertiesSetToValues()
		{
			_target = new BaseConfiguration(1053, 15, "UTC");

			_target.CultureId.HasValue.Should().Be.True();
			_target.IntervalLength.HasValue.Should().Be.True();
			_target.TimeZoneCode.Should().Not.Be.Null();

			_target.CultureId.Should().Be.EqualTo(1053);
			_target.IntervalLength.Should().Be.EqualTo(15);
			_target.TimeZoneCode.Should().Be.EqualTo("UTC");
		}
	}
}
