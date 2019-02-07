using System;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using SharpTestsEx;
using Teleopti.Wfm.Adherence.Historical.AdjustAdherence;
using Teleopti.Wfm.Adherence.Historical.Events;

namespace Teleopti.Wfm.Adherence.Test.Historical.Unit.AdjustAdherence
{
	[DomainTest]
	[TestFixture]
	public class AdjustAdherenceToNeutralCommandHandlerTest
	{
		public AdjustAdherenceToNeutralCommandHandler Target;
		public FakeEventPublisher Publisher;
		public FakeUserTimeZone TimeZone;
		
		[Test]
		public void ShouldAdjustToNeutral()
		{
			Target.Handle(new AdjustAdherenceToNeutralCommand
			{
				StartDateTime = "2019-01-30 08:00",
				EndDateTime = "2019-01-30 10:00"
			});

			var published = Publisher.PublishedEvents.OfType<PeriodAdjustedToNeutralEvent>().Single();
			published.StartTime.Should().Be("2019-01-30 08:00:00".Utc());
			published.EndTime.Should().Be("2019-01-30 10:00".Utc());
		}
		
		[Test]
		public void ShouldAdjustToNeutralFromUsersTimeZone()
		{
			TimeZone.IsSweden();

			Target.Handle(new AdjustAdherenceToNeutralCommand
			{
				StartDateTime = "2019-01-30 16:00",
				EndDateTime = "2019-01-30 18:00"
			});
	
			var published = Publisher.PublishedEvents.OfType<PeriodAdjustedToNeutralEvent>().Single();
			published.StartTime.Should().Be("2019-01-30 15:00:00".Utc());
			published.EndTime.Should().Be("2019-01-30 17:00:00".Utc());
		}
		
		[Test]
		public void ShouldNotAllowEndTimeBeforeStartTime()
		{
			var command = new AdjustAdherenceToNeutralCommand
			{
				StartDateTime = "2019-01-30 18:00",
				EndDateTime = "2019-01-30 16:00"
			};			
			Publisher.Clear();
			
			Assert.Throws<ArgumentOutOfRangeException>(() => Target.Handle(command));
			Publisher.PublishedEvents.Should().Be.Empty();				
		}					
	}
}