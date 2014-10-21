using System;
using System.Drawing;
using NUnit.Framework;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Rta.ImplementationDetailsTests
{
	[TestFixture]
	public class ScheduleLayerTest
	{
		private ScheduleLayer _target;

		[SetUp]
		public void Setup()
		{
			_target = new ScheduleLayer
			          	{
			          		DisplayColor = 255,
			          		StartDateTime = new DateTime(2013, 01, 25, 00, 00, 00, DateTimeKind.Utc),
							EndDateTime = new DateTime(2013, 01, 26, 00, 00, 00, DateTimeKind.Utc)
			          	};
		}

		[Test]
		public void ShouldReturnColor()
		{
			var color = Color.FromArgb(255);
			Assert.That(_target.TheColor(), Is.EqualTo(color));
		}

		[Test]
		public void ShouldReturnPeriod()
		{
			var period = new DateTimePeriod(new DateTime(2013, 01, 25, 00, 00, 00, DateTimeKind.Utc), new DateTime(2013, 01, 26, 00, 00, 00, DateTimeKind.Utc));
			Assert.That(_target.Period(), Is.EqualTo(period));
		}
	}
}
