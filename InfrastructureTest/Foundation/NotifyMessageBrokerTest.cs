using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Client.Composite;
using Teleopti.Interfaces.MessageBroker.Events;


namespace Teleopti.Ccc.InfrastructureTest.Foundation
{
    [TestFixture]
    [Category("LongRunning")]
    public class NotifyMessageBrokerTest : DatabaseTest
    {
        private IMessageBrokerComposite messBroker;
        private MockRepository mocks;
        private IUnitOfWork uow;

	    protected override void SetupForRepositoryTest()
        {
            mocks = new MockRepository();
            messBroker = mocks.StrictMock<IMessageBrokerComposite>();
            CleanUpAfterTest();
            uow = createUnitOfWorkWithMessageMock();
        }

        [Test]
        public void VerifyRootWithDeleteSentToMessageBroker()
        {
            UnitOfWork.PersistAll();
            IActivity obj = new Activity("for test");

            new ActivityRepository(uow).Add(obj);

            Assert.IsInstanceOf<IDeleteTag>(obj);
            using (mocks.Record())
            {
                using(mocks.Ordered())
                {
                    Expect.Call(messBroker.IsAlive).Return(true);
                    messBroker.Send(null, Guid.Empty, null);
					LastCall.Constraints(Rhino.Mocks.Constraints.Is.Anything(), Rhino.Mocks.Constraints.Is.Anything(), Rhino.Mocks.Constraints.Is.Matching<IEventMessage[]>(x => x[0].DomainUpdateType == DomainUpdateType.Insert));

                    Expect.Call(messBroker.IsAlive).Return(true);
					messBroker.Send(null, Guid.Empty, null);
					LastCall.Constraints(Rhino.Mocks.Constraints.Is.Anything(), Rhino.Mocks.Constraints.Is.Anything(), Rhino.Mocks.Constraints.Is.Matching<IEventMessage[]>(x => x[0].DomainUpdateType == DomainUpdateType.Update));

                    Expect.Call(messBroker.IsAlive).Return(true);
					messBroker.Send(null, Guid.Empty, null);
					LastCall.Constraints(Rhino.Mocks.Constraints.Is.Anything(), Rhino.Mocks.Constraints.Is.Anything(), Rhino.Mocks.Constraints.Is.Matching<IEventMessage[]>(x => x[0].DomainUpdateType == DomainUpdateType.Delete));
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
                new ActivityRepository(uow).Remove(obj);
                uow.PersistAll();
            }
        }

        [Test]
        public void VerifyNotSentToMessageBrokerWhenNotInitialized()
        {
            var theObject = new InternalTestObjectWithNoValidInterface();
            IRootChangeInfo rootChangeInfo = new RootChangeInfo(theObject, DomainUpdateType.Insert);
            Expect.Call(messBroker.IsAlive).Return(false);
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
			new PersonRepository(new ThisUnitOfWork(UnitOfWork)).Add(per);
			new ScenarioRepository(UnitOfWork).Add(scenario);
			new ActivityRepository(UnitOfWork).Add(activity);
			UnitOfWork.PersistAll();

			IMeeting obj = new Meeting(per,new [] {new MeetingPerson(per,false)},"subj","location","desc",activity,scenario);
			obj.StartDate = obj.EndDate = DateOnly.Today;
			new MeetingRepository(uow).Add(obj);

			using (mocks.Record())
			{
				using (mocks.Ordered())
				{
					Expect.Call(messBroker.IsAlive).Return(true);

					messBroker.Send(null, Guid.Empty, null);
					LastCall.Constraints(Rhino.Mocks.Constraints.Is.Anything(), Rhino.Mocks.Constraints.Is.Anything(), 
						Rhino.Mocks.Constraints.Is.Matching<IEventMessage[]>(x =>
							x[0].DomainUpdateType == DomainUpdateType.Insert && 
							x[1].DomainObjectType.Contains("MeetingChangedEntity") &&  x[1].DomainUpdateType == DomainUpdateType.Insert
						));

					Expect.Call(messBroker.IsAlive).Return(true);

					messBroker.Send(null, Guid.Empty, null);
					LastCall.Constraints(Rhino.Mocks.Constraints.Is.Anything(), Rhino.Mocks.Constraints.Is.Anything(),
						Rhino.Mocks.Constraints.Is.Matching<IEventMessage[]>(x =>
							x[0].DomainUpdateType == DomainUpdateType.Delete &&
							x[1].DomainObjectType.Contains("MeetingChangedEntity") && x[1].DomainUpdateType == DomainUpdateType.Delete
						));
				}
			}
			using (mocks.Playback())
			{
				uow.PersistAll();
				//delete
				new MeetingRepository(uow).Remove(obj);
				uow.PersistAll();
			}

			//cleanup
			new ActivityRepository(UnitOfWork).Remove(activity);
			new PersonRepository(new ThisUnitOfWork(UnitOfWork)).Remove(per);
			new ScenarioRepository(UnitOfWork).Remove(scenario);
			UnitOfWork.PersistAll();
		}

        [Test]
        public void VerifySendMultipleMessages()
        {
            UnitOfWork.PersistAll();
            IPerson per1 = PersonFactory.CreatePerson("1");
            IPerson per2 = PersonFactory.CreatePerson("2");
            new PersonRepository(new ThisUnitOfWork(uow)).Add(per1);
            new PersonRepository(new ThisUnitOfWork(uow)).Add(per2);

            using (mocks.Record())
            {
                using (mocks.Ordered())
                {
                    Expect.Call(messBroker.IsAlive).Return(true);
					messBroker.Send(null, Guid.Empty, null);
					LastCall.Constraints(Rhino.Mocks.Constraints.Is.Anything(), Rhino.Mocks.Constraints.Is.Anything(),
						Rhino.Mocks.Constraints.Is.Matching<IEventMessage[]>(x =>
							x[0].DomainUpdateType == DomainUpdateType.Insert && 
							x[1].DomainUpdateType == DomainUpdateType.Insert
							));

                    Expect.Call(messBroker.IsAlive).Return(true);
					messBroker.Send(null, Guid.Empty, null);
					LastCall.Constraints(Rhino.Mocks.Constraints.Is.Anything(), Rhino.Mocks.Constraints.Is.Anything(),
						Rhino.Mocks.Constraints.Is.Matching<IEventMessage[]>(x =>
							x[0].DomainUpdateType == DomainUpdateType.Delete &&
							x[1].DomainUpdateType == DomainUpdateType.Delete
							));
				}
            }

            using (mocks.Playback())
            {
                uow.PersistAll();

                //clean up
                new PersonRepository(new ThisUnitOfWork(uow)).Remove(per1);
                new PersonRepository(new ThisUnitOfWork(uow)).Remove(per2);
                uow.PersistAll();
            }
        }

        [Test]
        public void VerifyModuleIdWorks()
        {
            UnitOfWork.PersistAll();
            IActivity obj = new Activity("for test");
            Guid moduleId = Guid.NewGuid();

            IInitiatorIdentifier identifier = mocks.StrictMock<IInitiatorIdentifier>();
            new ActivityRepository(uow).Add(obj);

            using (mocks.Record())
            {
                using (mocks.Ordered())
                {
                    Expect.Call(identifier.InitiatorId).Return(moduleId).Repeat.Any();
                    Expect.Call(messBroker.IsAlive).Return(true);
					messBroker.Send(null, Guid.Empty, null);
	                LastCall.Constraints(Rhino.Mocks.Constraints.Is.Anything(), Rhino.Mocks.Constraints.Is.Anything(),
		                Rhino.Mocks.Constraints.Is.Matching<IEventMessage[]>(x =>
			                x[0].DomainUpdateType == DomainUpdateType.Insert && x[0].ModuleId == moduleId
			                ));

                    Expect.Call(messBroker.IsAlive).Return(true);
					messBroker.Send(null, Guid.Empty, null);
					LastCall.Constraints(Rhino.Mocks.Constraints.Is.Anything(), Rhino.Mocks.Constraints.Is.Anything(),
						Rhino.Mocks.Constraints.Is.Matching<IEventMessage[]>(x =>
							x[0].DomainUpdateType == DomainUpdateType.Update && x[0].ModuleId == moduleId
							));

                    Expect.Call(messBroker.IsAlive).Return(true);
					messBroker.Send(null, Guid.Empty, null);
					LastCall.Constraints(Rhino.Mocks.Constraints.Is.Anything(), Rhino.Mocks.Constraints.Is.Anything(),
						Rhino.Mocks.Constraints.Is.Matching<IEventMessage[]>(x =>
							x[0].DomainUpdateType == DomainUpdateType.Delete && x[0].ModuleId == moduleId
							));
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
                new ActivityRepository(uow).Remove(obj);
                uow.PersistAll(identifier);
            }
        }

        [Test]
        public void VerifyCanSendMessageNotInFilter()
        {
            var theObject = new InternalTestObjectWithNoValidInterface();
            IRootChangeInfo rootChangeInfo = new RootChangeInfo(theObject,DomainUpdateType.Insert);
            Expect.Call(messBroker.IsAlive).Return(true);
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
            }
            using (mocks.Playback())
            {
                target.EventMessage(changeInfo, module);
            }
        }

        private IUnitOfWork createUnitOfWorkWithMessageMock()
        {
            return ((NHibernateUnitOfWorkFactory)SetupFixtureForAssembly.DataSource.Application).CreateAndOpenUnitOfWork(messBroker);
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
            public ExtendedNotifyMessageBroker(IMessageBrokerComposite messageBroker)
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
