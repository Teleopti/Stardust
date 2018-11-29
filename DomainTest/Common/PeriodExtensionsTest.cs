using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;

using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.DomainTest.Common
{
	[TestFixture]
	public class PeriodExtensionsTest
	{
		[Test]
		public void ShouldReturnEmptyIfEmpty()
		{
			new ILayer<IActivity>[0]
				.PeriodBlocks().Should().Be.Empty();
		}

		[Test]
		public void ShouldReturnValueIfOnlyOne()
		{
			var period = new DateTimePeriod(2000, 1, 1, 2000, 1, 3);
			new[] { new MainShiftLayer(new Activity("sdf"), period) }.PeriodBlocks().Single()
				.Should().Be.EqualTo(period);
		}

		[Test]
		public void ShouldReturnPeriodBlocks()
		{
			new[]
				{
					//block 4th-12th
					new MainShiftLayer(new Activity("sdf"), new DateTimePeriod(2000, 1, 4, 2000, 1, 11)),
					new MainShiftLayer(new Activity("sdf"), new DateTimePeriod(2000, 1, 6, 2000, 1, 12)),
					//block 1th-3th
					new MainShiftLayer(new Activity("sdf"), new DateTimePeriod(2000, 1, 1, 2000, 1, 3)),
					new MainShiftLayer(new Activity("sdf"), new DateTimePeriod(2000, 1, 1, 2000, 1, 2))
				}.PeriodBlocks()
				.Should().Have.SameValuesAs(new DateTimePeriod(2000, 1, 4, 2000, 1, 12), new DateTimePeriod(2000, 1, 1, 2000, 1, 3));
		}

		[Test]
		public void ShouldBeOneBlockOnly()
		{
			new[]
				{
					new MainShiftLayer(new Activity("sdf"), new DateTimePeriod(2000, 1, 8, 2000, 1, 10)),
					new MainShiftLayer(new Activity("sdf"), new DateTimePeriod(2000, 1, 1, 2000, 1, 2)),
					new MainShiftLayer(new Activity("sdf"), new DateTimePeriod(2000, 1, 2, 2000, 1, 6)),
					new MainShiftLayer(new Activity("sdf"), new DateTimePeriod(2000, 1, 4, 2000, 1, 8))
				}.PeriodBlocks()
					.Should().Have.SameValuesAs(new DateTimePeriod(2000, 1, 1, 2000, 1, 10));
		}

		[Test]
		public void ShouldCombineIntersectingLayers()
		{
			new[]
				{
					new MainShiftLayer(new Activity("sdf"), new DateTimePeriod(2000, 1, 1, 2000, 1, 2)),
					new MainShiftLayer(new Activity("sdf"), new DateTimePeriod(2000, 1, 2, 2000, 1, 3))
				}.PeriodBlocks()
				 .Should().Have.SameValuesAs(new DateTimePeriod(2000, 1, 1, 2000, 1, 3));
		}

		[Test]
		public void VerifyPeriod()
		{
			new[]
				{
					new MainShiftLayer(new Activity("d"), new DateTimePeriod(2000, 1, 1, 2000, 1, 3)),
					new MainShiftLayer(new Activity("cd"), new DateTimePeriod(2000, 1, 2, 2000, 1, 7)),
					new MainShiftLayer(new Activity("cd"), new DateTimePeriod(2000, 1, 5, 2000, 1, 6))
				}.OuterPeriod().Value
				 .Should().Be.EqualTo(new DateTimePeriod(2000, 1, 1, 2000, 1, 7));
		}

		[Test]
		public void VerifyPeriodWhenListIsEmpty()
		{
			new List<IPeriodized>().OuterPeriod().HasValue
				.Should().Be.False();
		}
	}
}