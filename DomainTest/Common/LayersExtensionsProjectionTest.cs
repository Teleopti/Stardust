using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.TestData;


namespace Teleopti.Ccc.DomainTest.Common
{
	[TestFixture]
	public class LayersExtensionsProjectionTest
	{
		[Test]
		public void ShouldReturnProjectionForOneLayer()
		{
			var proj = new MainShiftLayer(new Activity("sdf"), new DateTimePeriod(2000, 1, 1, 2000, 1, 2)).CreateProjection();
			proj.Single().Period.Should().Be.EqualTo(new DateTimePeriod(2000, 1, 1, 2000, 1, 2));
		}

		[Test]
		public void ShouldReturnProjectionForMultipleLayers()
		{
			var start = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			var proj = new[]
				{
					new MainShiftLayer(new Activity("1"), new DateTimePeriod(start, start.AddHours(8))),
					new MainShiftLayer(new Activity("2"), new DateTimePeriod(start.AddHours(4), start.AddHours(5)))
				}.CreateProjection();
			proj.Count().Should().Be.EqualTo(3);
		}

		[Test]
		public void ShouldReturnEmptyProjectionForZeroLayers()
		{
			new MainShiftLayer[0].CreateProjection().Should().Be.Empty();
		}
	}
}