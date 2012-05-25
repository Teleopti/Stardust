using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork
{
	[TestFixture]
	public class GroupPageDenormalizerTest
	{
		private IDenormalizer target;
		private MockRepository mocks;
        private ISaveToDenormalizationQueue saveToDenormalizationQueue;
		private ISendDenormalizeNotification sendDenormalizeNotification;

		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
            saveToDenormalizationQueue = mocks.DynamicMock<ISaveToDenormalizationQueue>();

			sendDenormalizeNotification = mocks.DynamicMock<ISendDenormalizeNotification>();
			target = new GroupPageDenormalizer(sendDenormalizeNotification, saveToDenormalizationQueue);
		}

		[Test]
		public void ShouldRebuildReadModelForPerson()
		{
			var session = mocks.DynamicMock<IRunSql>();
			var sqlQuery = mocks.DynamicMock<ISqlQuery>();
			var person = mocks.DynamicMock<IPerson>();

			using (mocks.Record())
			{
				Expect.Call(session.Create("")).IgnoreArguments().Return(sqlQuery);
				Expect.Call(sqlQuery.Execute);
			}
			using (mocks.Playback())
			{
				target.Execute(session, new IRootChangeInfo[] { new RootChangeInfo(person, DomainUpdateType.Insert) });
			}
		}

		[Test]
		public void ShouldRebuildReadModelForGroupPage()
		{
			var session = mocks.DynamicMock<IRunSql>();
			var sqlQuery = mocks.DynamicMock<ISqlQuery>();
			var groupPage = mocks.DynamicMock<IGroupPage>();

			using (mocks.Record())
			{
				Expect.Call(session.Create("")).IgnoreArguments().Return(sqlQuery);
				Expect.Call(sqlQuery.Execute);
			}
			using (mocks.Playback())
			{
				target.Execute(session, new IRootChangeInfo[] { new RootChangeInfo(groupPage, DomainUpdateType.Insert) });
			}
		}

		[Test]
		public void ShouldNotRebuildReadModelForScenario()
		{
			var session = mocks.DynamicMock<IRunSql>();
			var scenario = mocks.DynamicMock<IScenario>();

			using (mocks.Record())
			{
			}
			using (mocks.Playback())
			{
				target.Execute(session, new IRootChangeInfo[] { new RootChangeInfo(scenario, DomainUpdateType.Insert) });
			}
		}
	}
}