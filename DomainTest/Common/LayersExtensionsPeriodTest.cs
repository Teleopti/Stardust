using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.DomainTest.Common
{
	[TestFixture]
	public class LayersExtensionsPeriodTest
	{
		[Test]
		public void ShouldReturnNullIfEmpty()
		{
			new ILayer<IActivity>[0]
				.Period().HasValue.Should().Be.False();
		}

		[Test]
		public void ShouldReturnNullIfAllIsNull()
		{
			new ILayer<IActivity>[]{null, null}
				.Period().HasValue.Should().Be.False();
		}

		[Test]
		public void ShouldReturnValueIfOnlyOne()
		{
			var period = new DateTimePeriod(2000, 1, 1, 2000, 1, 3);
			new [] {new MainShiftActivityLayerNew(new Activity("sdf"), period)}
				.Period().Value.Should().Be.EqualTo(period);
		}

		[Test]
		public void ShouldReturnMaximumPeriodAndIgnoreNull()
		{
			new[]
				{
					new MainShiftActivityLayerNew(new Activity("sdf"), new DateTimePeriod(2000, 1, 4, 2000, 11, 11)),
					null,
					new MainShiftActivityLayerNew(new Activity("sdf"), new DateTimePeriod(2000, 1, 4, 2001, 1, 1)),
					new MainShiftActivityLayerNew(new Activity("sdf"), new DateTimePeriod(2000, 1, 1, 2000, 1, 2))
				}
				.Period().Value.Should().Be.EqualTo(new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
		}
	}
}