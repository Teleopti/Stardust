using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork.TransactionHooks
{
	public class SettingsForPersonPeriodChangedEventPublisherTest
	{
		[Test]
		public void ShouldNotSendMessagesWhenOnlyPersonAndIrrelevantInterfaceAmongChanges()
		{
			var publisher = MockRepository.GenerateMock<IEventPopulatingPublisher>();
			var y = new SettingsForPersonPeriodChangedEventPublisher(publisher);
			var root = PersonFactory.CreatePerson().WithId();
			y.AfterCompletion(new IRootChangeInfo[]
			{
				new RootChangeInfo(root, DomainUpdateType.Update),
				new RootChangeInfo(new PersonWriteProtectionInfo(root).WithId(), DomainUpdateType.Update)
			});

			publisher.AssertWasNotCalled(x => x.Publish(new SettingsForPersonPeriodChangedEvent()), o => o.IgnoreArguments());
		}

		[Test]
		public void ShouldSendMessagesWhenOnlyPersonAndRelevantInterfaceAmongChanges()
		{
			var publisher = MockRepository.GenerateMock<IEventPopulatingPublisher>();
			var y = new SettingsForPersonPeriodChangedEventPublisher(publisher);
			var root = PersonFactory.CreatePerson().WithId();
			y.AfterCompletion(new IRootChangeInfo[]
			{
				new RootChangeInfo(root, DomainUpdateType.Update),
				new RootChangeInfo(TeamFactory.CreateSimpleTeam().WithId(), DomainUpdateType.Update)
			});

			publisher.AssertWasCalled(x => x.Publish(new SettingsForPersonPeriodChangedEvent()), o => o.IgnoreArguments());
		}
	}
}