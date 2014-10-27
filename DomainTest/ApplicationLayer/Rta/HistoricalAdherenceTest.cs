using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta
{
	[TestFixture]
	public class HistoricalAdherenceTest
	{
		[Test]
		public void ShouldBeFiftyPercent_WhenInAdherenceAndOutOfAdherenceAreSame()
		{
			var target = new HistoricalAdherence();
			var data = new AdherencePercentageReadModel()
			{
				MinutesInAdherence = 74,
				MinutesOutOfAdherence = 74
			};
			var result = target.ForDay(data);

			result.Value.Should().Be.EqualTo(0.5);
		}

		[Test]
		public void ShouldBeHundredPercent_WhenOnlyInAdherence()
		{
			var target = new HistoricalAdherence();
			var data = new AdherencePercentageReadModel()
			{
				MinutesInAdherence = 37,
				MinutesOutOfAdherence = 0
			};
			var result = target.ForDay(data);

			result.Value.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldBeZeroPercent_WhenOnlyOutOfAdherence()
		{
			var target = new HistoricalAdherence();
			var data = new AdherencePercentageReadModel()
			{
				MinutesInAdherence = 0,
				MinutesOutOfAdherence = 12
			};
			var result = target.ForDay(data);

			result.Value.Should().Be.EqualTo(0);
		}
	}
}
