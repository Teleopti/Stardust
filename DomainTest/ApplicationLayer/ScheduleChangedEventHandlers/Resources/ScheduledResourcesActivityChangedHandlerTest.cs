using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Resources;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ScheduleChangedEventHandlers.Resources
{
	[TestFixture]
	public class ScheduledResourcesActivityChangedHandlerTest
	{
		private ScheduledResourcesActivityChangedHandler _target;
		private IScheduledResourcesReadModelPersister _storage;
		private readonly Guid _activityId = Guid.NewGuid();

		[SetUp]
		public void Setup()
		{
			_storage = MockRepository.GenerateMock<IScheduledResourcesReadModelPersister>();
			_target = new ScheduledResourcesActivityChangedHandler(_storage);
		}

		[Test]
		public void ShouldHandleActivityNoLongerRequiresSeat()
		{
			_target.Handle(new ActivityChangedEvent{ActivityId = _activityId,Property = "RequiresSeat", OldValue = "true", NewValue = "false"});

			_storage.AssertWasCalled(x => x.ActivityUpdated(_activityId,false));
		}

		[Test]
		public void ShouldHandleActivityNowRequiresSeat()
		{
			_target.Handle(new ActivityChangedEvent { ActivityId = _activityId, Property = "RequiresSeat", OldValue = "false", NewValue = "true" });

			_storage.AssertWasCalled(x => x.ActivityUpdated(_activityId, true));
		}

		[Test]
		public void ShouldIgnoreOtherChangesToActivity()
		{
			_target.Handle(new ActivityChangedEvent { ActivityId = _activityId, Property = "Name", OldValue = "hej", NewValue = "hå" });

			_storage.AssertWasNotCalled(x => x.ActivityUpdated(_activityId, true),s => s.IgnoreArguments());
		}
	}
}
