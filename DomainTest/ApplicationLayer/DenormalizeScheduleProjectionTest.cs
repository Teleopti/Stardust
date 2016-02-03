using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
    [TestFixture]
	public class DenormalizeScheduleProjectionTest
    {
    	private ScheduleChangedEvent target;
    	private DateTimePeriod period;

    	[SetUp]
		public void Setup()
    	{
    		period = new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow);
    		target = new ScheduleChangedEvent
    		         	{
    		         		LogOnBusinessUnitId = Guid.NewGuid(),
    		         		LogOnDatasource = "test",
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
        	target.PersonId.Should().Be.EqualTo(target.PersonId);
        	target.StartDateTime.Should().Be.EqualTo(period.StartDateTime);
        	target.EndDateTime.Should().Be.EqualTo(period.EndDateTime);
        	target.Timestamp.Should().Be.EqualTo(period.StartDateTime);
        	target.ScenarioId.Should().Be.EqualTo(target.ScenarioId);
        	target.LogOnBusinessUnitId.Should().Be.EqualTo(target.LogOnBusinessUnitId);
        	target.LogOnDatasource.Should().Be.EqualTo(target.LogOnDatasource);
        }
    }
}