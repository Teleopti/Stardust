using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.DomainTest.Helper
{
    [TestFixture]
	public class DenormalizeScheduleProjectionTest
    {
    	private DenormalizeScheduleProjection target;
    	private DateTimePeriod period;

    	[SetUp]
		public void Setup()
    	{
    		period = new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow);
    		target = new DenormalizeScheduleProjection
    		         	{
    		         		BusinessUnitId = Guid.NewGuid(),
    		         		Datasource = "test",
    		         		EndDateTime = period.EndDateTime,
    		         		StartDateTime = period.StartDateTime,
    		         		PersonId = Guid.NewGuid(),
    		         		ScenarioId = Guid.NewGuid(),
    		         		Timestamp = period.StartDateTime
    		         	};
    	}

        [Test]
        public void PropertiesShouldWork()
        {
        	target.Identity.Should().Not.Be.EqualTo(Guid.Empty);
        	target.PersonId.Should().Be.EqualTo(target.PersonId);
        	target.StartDateTime.Should().Be.EqualTo(period.StartDateTime);
        	target.EndDateTime.Should().Be.EqualTo(period.EndDateTime);
        	target.Timestamp.Should().Be.EqualTo(period.StartDateTime);
        	target.ScenarioId.Should().Be.EqualTo(target.ScenarioId);
        	target.BusinessUnitId.Should().Be.EqualTo(target.BusinessUnitId);
        	target.Datasource.Should().Be.EqualTo(target.Datasource);
        }
    }
}