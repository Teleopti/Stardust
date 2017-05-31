using System;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	//[TestFixture]
	//public class RunWaitlistCommandHandlerTest
	//{
	//	private IEventPublisher publisher;
	//	private RunWaitlistCommandHandler target;
	//	private ICurrentBusinessUnit currentCurrentBusinessUnit;
	//	private ICurrentDataSource currentDataSource;

	//	[SetUp]
	//	public void Setup()
	//	{
	//		currentCurrentBusinessUnit = new FakeCurrentBusinessUnit();
	//		((FakeCurrentBusinessUnit) currentCurrentBusinessUnit)
	//			.FakeBusinessUnit(new Domain.Common.BusinessUnit("TestBu"));
	//		currentDataSource = new FakeCurrentDatasource("TestDataSource");
	//		publisher = new LegacyFakeEventPublisher();
	//		target = new RunWaitlistCommandHandler();
	//	}

	//	[Test]
	//	public void ShouldPublishRunWaitlistEvent()
	//	{
	//		var period = new DateTimePeriod(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow);
	//		var command = new RunWaitlistCommand
	//		{
	//			Period = period,
	//			TrackedCommandInfo = new TrackedCommandInfo
	//			{
	//				TrackId = Guid.NewGuid()
	//			}
	//		};

	//		//target.Handle(command);
	//		var @event = ((LegacyFakeEventPublisher) publisher).PublishedEvents.Single() as RunRequestWaitlistEvent;
	//		Assert.NotNull(@event);
	//		Assert.AreEqual(@event.StartTime, period.StartDateTime);
	//		Assert.AreEqual(@event.EndTime, period.EndDateTime);
	//	}
	//}
}