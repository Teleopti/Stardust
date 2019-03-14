using System;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Wfm.Adherence.Historical.Adjustment;
using Teleopti.Wfm.Adherence.Historical.Events;

namespace Teleopti.Wfm.Adherence.Test.Historical.Unit.AdjustAdherence
{
	[DomainTest]
	[TestFixture]
	public class CancelAdjustmentToNeutralCommandHandlerTest
	{
		public CancelAdjustmentToNeutralCommandHandler Target;
		public FakeEventPublisher Publisher;
		public FakeUserTimeZone TimeZone;

		[Test]
		public void ShouldCancelAdjustmentToNeutral()
		{
			Target.Handle(new CancelAdjustmentToNeutralCommand
			{
				StartDateTime = "2019-03-14 08:00",
				EndDateTime = "2019-03-14 10:00"
			});

			var published = Publisher.PublishedEvents.OfType<PeriodAdjustmentToNeutralCanceledEvent>().Single();
			published.StartTime.Should().Be("2019-03-14 08:00:00".Utc());
			published.EndTime.Should().Be("2019-03-14 10:00:00".Utc());
		}

		[Test]
		public void ShouldCancelAdjustmentFromUsersTimeZone()
		{
			TimeZone.IsSweden();

			Target.Handle(new CancelAdjustmentToNeutralCommand
			{
				StartDateTime = "2019-03-14 14:00",
				EndDateTime = "2019-03-14 15:00"
			});

			var published = Publisher.PublishedEvents.OfType<PeriodAdjustmentToNeutralCanceledEvent>().Single();
			published.StartTime.Should().Be("2019-03-14 13:00:00".Utc());
			published.EndTime.Should().Be("2019-03-14 14:00:00".Utc());
		}
		
		[Test]
		public void ShouldNotAllowEndTimeBeforeStartTime()
		{
			var command = new CancelAdjustmentToNeutralCommand
			{
				StartDateTime = "2019-03-14 15:00",
				EndDateTime = "2019-03-14 14:00"
			};			
			Publisher.Clear();
			
			Assert.Throws<ArgumentOutOfRangeException>(() => Target.Handle(command));
			Publisher.PublishedEvents.Should().Be.Empty();				
		}	
	}
}