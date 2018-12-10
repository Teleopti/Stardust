using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Messages.Denormalize;


namespace Teleopti.Ccc.DomainTest.Helper
{
	[TestFixture]
	public class InitialLoadScheduleProjectionTest
	{
		private InitialLoadScheduleProjection target;
		private DateTimePeriod period;

		[SetUp]
		public void Setup()
		{
			period = new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow);
			target = new InitialLoadScheduleProjection
			         	{
			         		LogOnBusinessUnitId = Guid.NewGuid(),
			         		LogOnDatasource = "test",
			         		Timestamp = period.StartDateTime
			         	};
		}

		[Test]
		public void PropertiesShouldWork()
		{
			target.Identity.Should().Not.Be.EqualTo(Guid.Empty);
			target.Timestamp.Should().Be.EqualTo(period.StartDateTime);
			target.LogOnBusinessUnitId.Should().Be.EqualTo(target.LogOnBusinessUnitId);
			target.LogOnDatasource.Should().Be.EqualTo(target.LogOnDatasource);
		}
	}
}