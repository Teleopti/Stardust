using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void VerifySendMessageWithDatePeriodAndMainReferenceWithProxy()
        {
            UnitOfWork.PersistAll();
            DateTimePeriod period = new DateTimePeriod(2000, 1, 1, 2001, 1, 1);
            IPerson per = PersonFactory.CreatePerson("for test");
            new Repository(UnitOfWork).Add(per);
            UnitOfWork.PersistAll();
            UnitOfWork.Clear();
            per = Session.Load<Person>(per.Id);
            IScenario scenario = ScenarioFactory.CreateScenarioAggregate();
            new Repository(UnitOfWork).Add(scenario);
            IPersonAbsence pAbs = PersonAbsenceFactory.CreatePersonAbsence(per, scenario, period);
            new Repository(UnitOfWork).Add(pAbs.Layer.Payload);
            UnitOfWork.PersistAll();

            IPersonAbsence obj = pAbs;
            IEventMessage mess1 = mocks.StrictMock<IEventMessage>();
            new Repository(uow).Add(pAbs);

            using (mocks.Record())
            {
                using (mocks.Ordered())
                {
                    Expect.Call(messBroker.IsInitialized).Return(true);
                    Expect.Call(messBroker.CreateEventMessage(period.StartDateTime, period.EndDateTime, Guid.Empty, per.Id.Value, typeof(Person), obj.Id.Value, typeof(PersonAbsence), DomainUpdateType.Insert))
                    .Return(mess1);
					messBroker.SendEventMessages(null, Guid.Empty, null);
					LastCall.Constraints(Rhino.Mocks.Constraints.Is.Anything(), Rhino.Mocks.Constraints.Is.Anything(), List.Equal(new[] { mess1 }));

                    Expect.Call(messBroker.IsInitialized).Return(true);
                    Expect.Call(messBroker.CreateEventMessage(period.StartDateTime, period.EndDateTime, Guid.Empty, per.Id.Value, typeof(Person), obj.Id.Value, typeof(PersonAbsence), DomainUpdateType.Delete))
                    .Return(mess1);
					messBroker.SendEventMessages(null, Guid.Empty, null);
					LastCall.Constraints(Rhino.Mocks.Constraints.Is.Anything(), Rhino.Mocks.Constraints.Is.Anything(), List.Equal(new[] { mess1 }));
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
            new Repository(UnitOfWork).Remove(pAbs.Layer.Payload);
            new Repository(UnitOfWork).Remove(per);
            new Repository(UnitOfWork).Remove(scenario);
            UnitOfWork.PersistAll();
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

        [Test]
        public void VerifyRootWithoutDeleteSentToMessageBroker()
        {
            var person = PersonFactory.CreatePerson("Person1");
            var dayOff = DayOffFactory.CreateDayOff();
            var scenario = ScenarioFactory.CreateScenarioAggregate();
            new PersonRepository(UnitOfWork).Add(person);
            new DayOffRepository(UnitOfWork).Add(dayOff);
            new ScenarioRepository(UnitOfWork).Add(scenario);
            UnitOfWork.PersistAll();
            IPersonDayOff obj = new PersonDayOff(person, scenario, dayOff, new DateOnly(2010,1,1));
            
            IEventMessage mess1 = mocks.StrictMock<IEventMessage>();
            IEventMessage mess2 = mocks.StrictMock<IEventMessage>();

            new Repository(uow).Add(obj);

            Assert.IsNotInstanceOf<IDeleteTag>(obj);
            using (mocks.Record())
            {
                using (mocks.Ordered())
                {
                    Expect.Call(messBroker.IsInitialized).Return(true);
                    Expect.Call(messBroker.CreateEventMessage(new DateTime(2010, 1, 1), new DateTime(2010, 1, 1).AddTicks(1), Guid.Empty, person.Id.Value, typeof(Person), obj.Id.Value, obj.GetType(), DomainUpdateType.Insert))
                                        .Return(mess1);
					messBroker.SendEventMessages(null, Guid.Empty, null);
					LastCall.Constraints(Rhino.Mocks.Constraints.Is.Anything(), Rhino.Mocks.Constraints.Is.Anything(), List.Equal(new[] { mess1 }));

                    Expect.Call(messBroker.IsInitialized).Return(true);
                    Expect.Call(messBroker.CreateEventMessage(new DateTime(2010, 1, 1), new DateTime(2010, 1, 1).AddTicks(1), Guid.Empty, person.Id.Value, typeof(Person), obj.Id.Value, obj.GetType(), DomainUpdateType.Delete))
                                        .Return(mess2);
					messBroker.SendEventMessages(null, Guid.Empty, null);
					LastCall.Constraints(Rhino.Mocks.Constraints.Is.Anything(), Rhino.Mocks.Constraints.Is.Anything(), List.Equal(new[] { mess2 }));
                }
            }
            using (mocks.Playback())
            {
                //insert
                uow.PersistAll();

                //delete
                new Repository(uow).Remove(obj);
                uow.PersistAll();
            }

            //clean up
            new DayOffRepository(UnitOfWork).Remove(dayOff);
            new PersonRepository(UnitOfWork).Remove(person);
            new ScenarioRepository(UnitOfWork).Remove(scenario);
            UnitOfWork.PersistAll();
        }

        [Test]
        public void VerifySendMessageWithDatePeriodAndMainReference()
        {
            UnitOfWork.PersistAll();
            DateTimePeriod period = new DateTimePeriod(2000,1,1,2001,1,1);
            IPerson per = PersonFactory.CreatePerson("for test");
            IScenario scenario = ScenarioFactory.CreateScenarioAggregate();
            new Repository(UnitOfWork).Add(per);
            new Repository(UnitOfWork).Add(scenario);
            IPersonAbsence pAbs = PersonAbsenceFactory.CreatePersonAbsence(per, scenario, period);
            new Repository(UnitOfWork).Add(pAbs.Layer.Payload);
            UnitOfWork.PersistAll();

            IPersonAbsence obj = pAbs;
            IEventMessage mess1 = mocks.StrictMock<IEventMessage>();
            new Repository(uow).Add(pAbs);

            using (mocks.Record())
            {
                using (mocks.Ordered())
                {
                    Expect.Call(messBroker.IsInitialized).Return(true);
                    Expect.Call(messBroker.CreateEventMessage(period.StartDateTime, period.EndDateTime, Guid.Empty, per.Id.Value, typeof(Person), obj.Id.Value, obj.GetType(), DomainUpdateType.Insert))
                    .Return(mess1);
					messBroker.SendEventMessages(null, Guid.Empty, null);
					LastCall.Constraints(Rhino.Mocks.Constraints.Is.Anything(), Rhino.Mocks.Constraints.Is.Anything(), List.Equal(new[] { mess1 }));

                    Expect.Call(messBroker.IsInitialized).Return(true);
                    Expect.Call(messBroker.CreateEventMessage(period.StartDateTime, period.EndDateTime, Guid.Empty, per.Id.Value, typeof(Person), obj.Id.Value, obj.GetType(), DomainUpdateType.Delete))
                    .Return(mess1);
					messBroker.SendEventMessages(null, Guid.Empty, null);
					LastCall.Constraints(Rhino.Mocks.Constraints.Is.Anything(), Rhino.Mocks.Constraints.Is.Anything(), List.Equal(new[] { mess1 }));
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
            new Repository(UnitOfWork).Remove(pAbs.Layer.Payload);
            new Repository(UnitOfWork).Remove(per);
            new Repository(UnitOfWork).Remove(scenario);
            UnitOfWork.PersistAll();
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldSendMessageFromCustomSource()
		{
			UnitOfWork.PersistAll();
			IPerson per = PersonFactory.CreatePerson("for test");
			IScenario scenario = ScenarioFactory.CreateScenarioAggregate();
			IActivity activity = ActivityFactory.CreateActivity("Training");
			new Repository(UnitOfWork).Add(per);
			new Repository(UnitOfWork).Add(scenario);
			new Repository(UnitOfWork).Add(activity.GroupingActivity);
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
			new Repository(UnitOfWork).Remove(activity.GroupingActivity);
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
            IMessageBrokerModule module = mocks.StrictMock<IMessageBrokerModule>();
            new Repository(uow).Add(obj);

            using (mocks.Record())
            {
                using (mocks.Ordered())
                {
                    Expect.Call(module.ModuleId).Return(moduleId).Repeat.Any();
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
                uow.PersistAll(module);

                //update
                obj = new ActivityRepository(uow).Load(obj.Id.Value);
                obj.Description = new Description("changed");
                uow.PersistAll(module);

                //delete
                new Repository(uow).Remove(obj);
                uow.PersistAll(module);
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
            return ((NHibernateUnitOfWorkFactory)SetupFixtureForAssembly.DataSource.Application).CreateAndOpenUnitOfWork(messBroker);
        }

        //denna är gjord här för det finns inga rötter som har denna komb
        private class RootWithPeriodButNoMainReference : AggregateRootWithBusinessUnit, IPeriodized
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
        private class RootWithoutPeriodButMainReference : AggregateRootWithBusinessUnit, IMainReference
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

        private class InternalTestObjectWithNoValidInterface : AggregateRootWithBusinessUnit
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
