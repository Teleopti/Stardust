using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Wfm.Adherence.Historical.Events;
using Teleopti.Wfm.Adherence.Historical.Infrastructure;
using Teleopti.Wfm.Adherence.Test.InfrastructureTesting;

namespace Teleopti.Wfm.Adherence.Test.Historical.Infrastructure
{
	[TestFixture]
	[DatabaseTest]
	public class RtaEventStoreApprovedReadTest
	{
		public IEventPublisher Publisher;
		public IRtaEventStoreReader Target;
		public WithUnitOfWork WithUnitOfWork;

		[Test]
		public void ShouldLoadForDate()
		{
			var personId = Guid.NewGuid();
			Publisher.Publish(new PeriodApprovedAsInAdherenceEvent
			{
				PersonId = personId,
				BelongsToDate = "2018-10-31".Date()
			});
			Publisher.Publish(new PeriodApprovedAsInAdherenceEvent
			{
				PersonId = personId,
				BelongsToDate = "2018-11-01".Date()
			});

			var actual = WithUnitOfWork.Get(() => Target.Load(personId, "2018-10-31".Date()));

			actual.Cast<PeriodApprovedAsInAdherenceEvent>().Single().BelongsToDate.Should().Be("2018-10-31".Date());
		}
	}
}