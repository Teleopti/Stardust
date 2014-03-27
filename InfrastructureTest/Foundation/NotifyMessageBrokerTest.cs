using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Events;
using List=Rhino.Mocks.Constraints.List;


namespace Teleopti.Ccc.InfrastructureTest.Foundation
{
    [TestFixture]
    [Category("LongRunning")]
    public class NotifyMessageBrokerTest : DatabaseTest
    {
        private IMessageBroker messBroker;
        private MockRepository mocks;
        private IUnitOfWork uow;

        protected override void SetupForRepositoryTest()
        {
            mocks = new MockRepository();
            messBroker = mocks.StrictMock<IMessageBroker>();
            SkipRollback();
            uow = createUnitOfWorkWithMessageMock();
        }

        protected override void TeardownForRepositoryTest()
        {
            uow.Dispose();
        }

        [Test]
        public void VerifyRootWithDeleteSentToMessageBroker()
        {
            UnitOfWork.PersistAll();
            IActivity obj = new Activity("for test");

            IEventMessage mess1 = mocks.StrictMock<IEventMessage>();
            IEventMessage mess2 = mocks.StrictMock<IEventMessage>();
            IEventMessage mess3 = mocks.StrictMock<IEventMessage>();
            new Repository(uow).Add(obj);

            Assert.IsInstanceOf<IDeleteTag>(obj);
            using (mocks.Record())
            {
                using(mocks.Ordered())
                {
                    Expect.Call(messBroker.IsInitialized).Return(true);
                    Expect.Call(messBroker.CreateEventMessage(Guid.Empty, obj.Id.Value, obj.GetType(), DomainUpdateType.Insert))
                                        .Return(mess1);
                    messBroker.SendEventMessages(null, Guid.Empty, null);
					LastCall.Constraints(Rhino.Mocks.Constraints.Is.Anything(), Rhino.Mocks.Constraints.Is.Anything(), List.Equal(new[] { mess1 }));

                    Expect.Call(messBroker.IsInitialized).Return(true);
                    Expect.Call(messBroker.CreateEventMessage(Guid.Empty, obj.Id.Value, obj.GetType(), DomainUpdateType.Update))
                                        .Return(mess2);
					messBroker.SendEventMessages(null, Guid.Empty, null);
					LastCall.Constraints(Rhino.Mocks.Constraints.Is.Anything(), Rhino.Mocks.Constraints.Is.Anything(), List.Equal(new[] { mess2 }));

                    Expect.Call(messBroker.IsInitialized).Return(true);
                    Expect.Call(messBroker.CreateEventMessage(Guid.Empty, obj.Id.Value, obj.GetType(), DomainUpdateType.Delete))
                                        .Return(mess3);
					messBroker.SendEventMessages(null, Guid.Empty, null);
					LastCall.Constraints(Rhino.Mocks.Constraints.Is.Anything(), Rhino.Mocks.Constraints.Is.Anything(), List.Equal(new[] { mess3 }));
                }
            }
            using (mocks.Playback())
            {
                //insert
                uow.PersistAll();

                //update
                obj = new ActivityRepository(uow).Load(obj.Id.Value);
                obj.Description = new Description("changed");
                uow.PersistAll();

                //delete
                new Repository(uow).Remove(obj);
                uow.PersistAll();
            }
        }

        [Test]
        public void VerifyNotSentToMessageBrokerWhenNotInitialized()
        {
            var theObject = new InternalTestObjectWithNoValidInterface();
            IRootChangeInfo rootChangeInfo = new RootChangeInfo(theObject, DomainUpdateType.Insert);
            Expect.Call(messBroker.IsInitialized).Return(false);
            mocks.ReplayAll();
            NotifyMessageBroker notifier = new NotifyMessageBroker(messBroker);
            notifier.Notify(Guid.Empty, new List<IRootChangeInfo> { rootChangeInfo });
            mocks.VerifyAll();
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldSendMessageFromCustomSource()
		{
			UnitOfWork.PersistAll();
			IPerson per = PersonFactory.CreatePerson("for test");
			IScenario scenario = ScenarioFactory.CreateScenarioAggregate();
			IActivity activity = new Activity("Training");
			new Repository(UnitOfWork).Add(per);
			new Repository(UnitOfWork).Add(scenario);
			new Repository(UnitOfWork).Add(activity);
			UnitOfWork.PersistAll();

			IMeeting obj = new Meeting(per,new [] {new MeetingPerson(per,false)},"subj","location","desc",activity,scenario);
			obj.StartDate = obj.EndDate = DateOnly.Today;
			var period = obj.MeetingPeriod(obj.StartDate);
			IEventMessage mess1 = mocks.StrictMock<IEventMessage>();
			IEventMessage mess2 = mocks.StrictMock<IEventMessage>();
			new Repository(uow).Add(obj);

			using (mocks.Record())
			{
				using (mocks.Ordered())
				{
					Expect.Call(messBroker.IsInitialized).Return(true);
					Expect.Call(messBroker.CreateEventMessage(Guid.Empty, obj.Id.Value, obj.GetType(), DomainUpdateType.Insert))
					.Return(mess1);
					
					Expect.Call(messBroker.CreateEventMessage(period.StartDateTime,period.EndDateTime, Guid.Empty, per.Id.Value, typeof(Person), obj.Id.Value, typeof(MeetingChangedEntity), DomainUpdateType.Insert))
					.Return(mess2);

					messBroker.SendEventMessages(null, Guid.Empty, null);
					LastCall.Constraints(Rhino.Mocks.Constraints.Is.Anything(), Rhino.Mocks.Constraints.Is.Anything(), List.Equal(new[] { mess1, mess2 }));

					Expect.Call(messBroker.IsInitialized).Return(true);
					Expect.Call(messBroker.CreateEventMessage(Guid.Empty, obj.Id.Value, obj.GetType(), DomainUpdateType.Delete))
					.Return(mess1);

					Expect.Call(messBroker.CreateEventMessage(period.StartDateTime, period.EndDateTime, Guid.Empty, per.Id.Value, typeof(Person), obj.Id.Value, typeof(MeetingChangedEntity), DomainUpdateType.Delete))
					.Return(mess2);

					messBroker.SendEventMessages(null, Guid.Empty, null);
					LastCall.Constraints(Rhino.Mocks.Constraints.Is.Anything(), Rhino.Mocks.Constraints.Is.Anything(), List.Equal(new[] { mess1, mess2 }));
				}
			}
			using (mocks.Playback())
			{
				uow.PersistAll();
				//delete
				new Repository(uow).Remove(obj);
				uow.PersistAll();
			}

			//cleanup
			new Repository(UnitOfWork).Remove(activity);
			new Repository(UnitOfWork).Remove(per);
			new Repository(UnitOfWork).Remove(scenario);
			UnitOfWork.PersistAll();
		}

        [Test]
        public void VerifySendMultipleMessages()
        {
            UnitOfWork.PersistAll();
            IPerson per1 = PersonFactory.CreatePerson("1");
            IPerson per2 = PersonFactory.CreatePerson("2");
            IEventMessage mess1 = mocks.StrictMock<IEventMessage>();
            IEventMessage mess2 = mocks.StrictMock<IEventMessage>();
            IEventMessage mess3 = mocks.StrictMock<IEventMessage>();
            IEventMessage mess4 = mocks.StrictMock<IEventMessage>();
            new Repository(uow).Add(per1);
            new Repository(uow).Add(per2);

            using (mocks.Record())
            {
                using (mocks.Ordered())
                {
                    Expect.Call(messBroker.IsInitialized).Return(true);
                    Expect.Call(messBroker.CreateEventMessage(Guid.Empty, per1.Id.Value, per1.GetType(), DomainUpdateType.Insert))
                                        .Return(mess1);
                    Expect.Call(messBroker.CreateEventMessage(Guid.Empty, per2.Id.Value, per2.GetType(), DomainUpdateType.Insert))
                                        .Return(mess2);
					messBroker.SendEventMessages(null, Guid.Empty, null);
					LastCall.Constraints(Rhino.Mocks.Constraints.Is.Anything(), Rhino.Mocks.Constraints.Is.Anything(), List.Equal(new[] { mess1, mess2 }));

                    Expect.Call(messBroker.IsInitialized).Return(true);
                    Expect.Call(messBroker.CreateEventMessage(Guid.Empty, per1.Id.Value, per1.GetType(), DomainUpdateType.Delete))
                                        .Return(mess3);
                    Expect.Call(messBroker.CreateEventMessage(Guid.Empty, per2.Id.Value, per2.GetType(), DomainUpdateType.Delete))
                                        .Return(mess4);
					messBroker.SendEventMessages(null, Guid.Empty, null);
					LastCall.Constraints(Rhino.Mocks.Constraints.Is.Anything(), Rhino.Mocks.Constraints.Is.Anything(), List.Equal(new[] { mess3, mess4 }));
                }
            }

            using (mocks.Playback())
            {
                uow.PersistAll();

                //clean up
                new Repository(uow).Remove(per1);
                new Repository(uow).Remove(per2);
                uow.PersistAll();
            }
        }

        [Test]
        public void VerifyModuleIdWorks()
        {
            UnitOfWork.PersistAll();
            IActivity obj = new Activity("for test");
            Guid moduleId = Guid.NewGuid();

            IEventMessage mess1 = mocks.StrictMock<IEventMessage>();
            IEventMessage mess2 = mocks.StrictMock<IEventMessage>();
            IEventMessage mess3 = mocks.StrictMock<IEventMessage>();
            IInitiatorIdentifier identifier = mocks.StrictMock<IInitiatorIdentifier>();
            new Repository(uow).Add(obj);

            using (mocks.Record())
            {
                using (mocks.Ordered())
                {
                    Expect.Call(identifier.InitiatorId).Return(moduleId).Repeat.Any();
                    Expect.Call(messBroker.IsInitialized).Return(true);
                    Expect.Call(messBroker.CreateEventMessage(moduleId, obj.Id.Value, obj.GetType(), DomainUpdateType.Insert))
                                        .Return(mess1);
					messBroker.SendEventMessages(null, Guid.Empty, null);
					LastCall.Constraints(Rhino.Mocks.Constraints.Is.Anything(), Rhino.Mocks.Constraints.Is.Anything(), List.Equal(new[] { mess1 }));

                    Expect.Call(messBroker.IsInitialized).Return(true);
                    Expect.Call(messBroker.CreateEventMessage(moduleId, obj.Id.Value, obj.GetType(), DomainUpdateType.Update))
                                        .Return(mess2);
					messBroker.SendEventMessages(null, Guid.Empty, null);
					LastCall.Constraints(Rhino.Mocks.Constraints.Is.Anything(), Rhino.Mocks.Constraints.Is.Anything(), List.Equal(new[] { mess2 }));

                    Expect.Call(messBroker.IsInitialized).Return(true);
                    Expect.Call(messBroker.CreateEventMessage(moduleId, obj.Id.Value, obj.GetType(), DomainUpdateType.Delete))
                                        .Return(mess3);
					messBroker.SendEventMessages(null, Guid.Empty, null);
					LastCall.Constraints(Rhino.Mocks.Constraints.Is.Anything(), Rhino.Mocks.Constraints.Is.Anything(), List.Equal(new[] { mess3 }));
                }
            }
            using (mocks.Playback())
            {
                //insert
                uow.PersistAll(identifier);

                //update
                obj = new ActivityRepository(uow).Load(obj.Id.Value);
                obj.Description = new Description("changed");
                uow.PersistAll(identifier);

                //delete
                new Repository(uow).Remove(obj);
                uow.PersistAll(identifier);
            }
        }

        [Test]
        public void VerifyCanSendMessageNotInFilter()
        {
            var theObject = new InternalTestObjectWithNoValidInterface();
            IRootChangeInfo rootChangeInfo = new RootChangeInfo(theObject,DomainUpdateType.Insert);
            Expect.Call(messBroker.IsInitialized).Return(true);
            mocks.ReplayAll();
            NotifyMessageBroker notifier = new NotifyMessageBroker(messBroker);
            notifier.Notify(Guid.Empty,new List<IRootChangeInfo>{rootChangeInfo});
            mocks.VerifyAll();
        }

        [Test]
        public void VerifyPeriodButNoMainReference()
        {
            ExtendedNotifyMessageBroker target= new ExtendedNotifyMessageBroker(messBroker);
            DateTimePeriod period = new DateTimePeriod(1900,1,1,2000,1,1);
            Guid rootId = Guid.NewGuid();
            IAggregateRoot root = new RootWithPeriodButNoMainReference(period);
            root.SetId(rootId);
            Guid module = Guid.NewGuid();
            IRootChangeInfo changeInfo = new RootChangeInfo(root, DomainUpdateType.Update);
            using(mocks.Record())
            {
                Expect.Call(messBroker.CreateEventMessage(period.StartDateTime, period.EndDateTime, module, root.Id.Value, root.GetType(), DomainUpdateType.Update))
                    .Return(null);
            }
            using(mocks.Playback())
            {
                target.EventMessage(changeInfo, module);
            }
        }

        [Test]
        public void VerifyNoPeriodButMainReference()
        {
            ExtendedNotifyMessageBroker target = new ExtendedNotifyMessageBroker(messBroker);
            Guid rootId = Guid.NewGuid();
            Guid refRootId = Guid.NewGuid();
            IAggregateRoot refRoot = new Activity("for test");
            refRoot.SetId(refRootId);
            IAggregateRoot root = new RootWithoutPeriodButMainReference(refRoot);
            root.SetId(rootId);
            Guid module = Guid.NewGuid();
            IRootChangeInfo changeInfo = new RootChangeInfo(root, DomainUpdateType.Update);
            using (mocks.Record())
            {
                Expect.Call(messBroker.CreateEventMessage(module, refRootId, refRoot.GetType(), root.Id.Value, root.GetType(), DomainUpdateType.Update))
                    .Return(null);
            }
            using (mocks.Playback())
            {
                target.EventMessage(changeInfo, module);
            }
        }

        private IUnitOfWork createUnitOfWorkWithMessageMock()
        {
            return ((NHibernateUnitOfWorkFactory)SetupFixtureForAssembly.DataSource.Application).CreateAndOpenUnitOfWork(messBroker, TransactionIsolationLevel.Default, null);
        }

        //denna är gjord här för det finns inga rötter som har denna komb
        private class RootWithPeriodButNoMainReference : VersionedAggregateRootWithBusinessUnit, IPeriodized
        {
            private readonly DateTimePeriod _period;


            public RootWithPeriodButNoMainReference(DateTimePeriod period)
            {
                _period = period;
            }

            public DateTimePeriod Period
            {
                get { return _period; }
            }

        }

        //denna är gjord här för det finns inga rötter som har denna komb
        private class RootWithoutPeriodButMainReference : VersionedAggregateRootWithBusinessUnit, IMainReference
        {
            private readonly IAggregateRoot _refRoot;

            public RootWithoutPeriodButMainReference(IAggregateRoot refRoot)
            {
                _refRoot = refRoot;
            }

            public IAggregateRoot MainRoot
            {
                get { return _refRoot; }
            }
        }

        private class InternalTestObjectWithNoValidInterface : VersionedAggregateRootWithBusinessUnit
        {

        }

        private class ExtendedNotifyMessageBroker : NotifyMessageBroker
        {
            public ExtendedNotifyMessageBroker(IMessageBroker messageBroker)
                : base(messageBroker)
            {
            }

            public IEventMessage EventMessage(IRootChangeInfo changeInfo, Guid module)
            {
                return CreateEventMessage(changeInfo, module);
            }
        }
        
     
    }
}
