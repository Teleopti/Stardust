using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Composite.Events;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Intraday;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCodeTest.Intraday
{
    [TestFixture]
    public class DayLayerViewModelTest
    {
        private IDayLayerViewModel target;
        private IPerson person;
        private MockRepository mocks;
        private DateTimePeriod period;
        private IRtaStateHolder rtaStateHolder;
        private ISchedulingResultStateHolder schedulingResultStateHolder;
        private IScheduleDictionary scheduleDictionary;
        private ITeam team;
        //        private IVisualLayerFactory layerFactory;
        private IEventAggregator eventAggregator;
        private IUnitOfWorkFactory unitOfWorkFactory;
        private IRepositoryFactory repositoryFactory;
        private IDispatcherWrapper testDispatcher;
        private DateOnlyPeriod dateOnlyPeriod;

        [SetUp]
        public void Setup()
        {
            //           layerFactory = new VisualLayerFactory();
            mocks = new MockRepository();
            rtaStateHolder = mocks.StrictMock<IRtaStateHolder>();
            schedulingResultStateHolder = mocks.StrictMock<ISchedulingResultStateHolder>();
            scheduleDictionary = mocks.StrictMock<IScheduleDictionary>();
            eventAggregator = mocks.DynamicMock<IEventAggregator>();
            testDispatcher = new TestDispatcher();
            team = TeamFactory.CreateSimpleTeam();
            period = new DateTimePeriod(2012, 10, 25, 2012, 10, 25);
            dateOnlyPeriod = new DateOnlyPeriod(2012, 10, 25, 2012, 10, 25);
            person = PersonFactory.CreatePerson();
            person.SetId(Guid.NewGuid());
            person.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(new DateOnly(), team));
            person.PermissionInformation.SetDefaultTimeZone(
                (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time")));

            unitOfWorkFactory = mocks.DynamicMock<IUnitOfWorkFactory>();
            repositoryFactory = mocks.DynamicMock<IRepositoryFactory>();

            target = new DayLayerViewModel(rtaStateHolder, eventAggregator, unitOfWorkFactory, repositoryFactory, testDispatcher);
        }

        [Test]
        public void ShouldCreateModels()
        {
            var scheduleRange = mocks.DynamicMock<IScheduleRange>();
            var scheduleDay = mocks.DynamicMock<IScheduleDay>();

            using (mocks.Record())
            {
                Expect.Call(rtaStateHolder.SchedulingResultStateHolder).Return(schedulingResultStateHolder).Repeat.AtLeastOnce();
                Expect.Call(schedulingResultStateHolder.Schedules).Return(scheduleDictionary).Repeat.AtLeastOnce();
                Expect.Call(scheduleDictionary[person]).Return(scheduleRange).Repeat.AtLeastOnce();
                Expect.Call(scheduleRange.Person).Return(person).Repeat.AtLeastOnce();
                Expect.Call(scheduleRange.ScheduledDay(DateOnly.Today)).IgnoreArguments().Return(scheduleDay).Repeat.
                    AtLeastOnce();
                Expect.Call(scheduleDay.HasProjection).Return(false).Repeat.AtLeastOnce();
                Expect.Call(() => scheduleDictionary.PartModified += target.OnScheduleModified);

                expectLoadOfSettings();
            }
            using (mocks.Playback())
            {
                target.CreateModels(new[] { person }, new DateOnlyPeriodAsDateTimePeriod(dateOnlyPeriod, TeleoptiPrincipal.Current.Regional.TimeZone));
                target.Models.Count.Should().Be.EqualTo(1);
            }
        }

		[Test]
		public void ShouldReturnNullWhenNoModel()
		{
			mocks.ReplayAll();
			target.Models.Clear();
			target.RefreshProjection(person);
			rtaStateHolder.AssertWasNotCalled(r => r.ActualAgentStates);
		}

        private void expectLoadOfSettings()
        {
            var unitOfWork = mocks.DynamicMock<IUnitOfWork>();
            var settingDataRepository = mocks.DynamicMock<ISettingDataRepository>();

            Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
            Expect.Call(repositoryFactory.CreateGlobalSettingDataRepository(unitOfWork)).Return(
                settingDataRepository);
            Expect.Call(settingDataRepository.FindValueByKey<CommonNameDescriptionSetting>("CommonNameDescription", null)).IgnoreArguments().
                Return(new CommonNameDescriptionSetting());
        }

		[Test]
		public void VerifyCanRefreshAgentState()
		{
			IActualAgentState agentState = new ActualAgentState();
			agentState.AlarmStart = DateTime.UtcNow.AddMinutes(-45);
			agentState.State = "MyCurrentStateDescription";
			agentState.AlarmName = "MyAlarmName";
			var dictionary = new Dictionary<Guid, IActualAgentState> { { (Guid)person.Id, agentState } };

			target.Models.Add(new DayLayerModel(person, new DateTimePeriod(DateTime.UtcNow.Date, DateTime.UtcNow.Date),
			                                    new Team(), new LayerViewModelCollection(), new CommonNameDescriptionSetting()));
			rtaStateHolder.Expect(r => r.ActualAgentStates).IgnoreArguments().Return(dictionary);
			mocks.ReplayAll();

			target.RefreshElapsedTime(DateTime.UtcNow);
			mocks.VerifyAll();

			Assert.That(target.Models.First().Person.Id, Is.EqualTo(person.Id));
			Assert.That(target.Models.First().CurrentStateDescription, Is.EqualTo(agentState.State));
			Assert.That(target.Models.First().AlarmDescription, Is.EqualTo(agentState.AlarmName));
		}

		//[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		//public void VerifyCanRefreshProjection()
		//{
		//    var scheduleRange = mocks.DynamicMock<IScheduleRange>();
		//    var createLayerViewModelService = mocks.DynamicMock<ICreateLayerViewModelService>();
		//    var agentState = mocks.StrictMock<IAgentState>();
		//    var layerModel = new AbsenceLayerViewModel(new AbsenceLayer(new Absence(), period),
		//                                        new MainShift(new ShiftCategory("MainShift")), eventAggregator);

		//    Expect.Call(rtaStateHolder.SchedulingResultStateHolder).Return(schedulingResultStateHolder).Repeat.AtLeastOnce();
		//    Expect.Call(schedulingResultStateHolder.Schedules).Return(scheduleDictionary).Repeat.Twice();
		//    Expect.Call(scheduleDictionary[person]).Return(scheduleRange).Repeat.AtLeastOnce();
		//    Expect.Call(createLayerViewModelService.CreateProjectionViewModelsFromSchedule(scheduleRange, period,
		//                                                                                   eventAggregator,
		//                                                                                   TimeSpan.FromMinutes(15))).
		//        Return(
		//            new List<ILayerViewModel> {layerModel});

		//    IDictionary<IPerson, IAgentState> agentStates = new Dictionary<IPerson, IAgentState>();
		//    agentStates.Add(person, agentState);
		//    //Expect.Call(rtaStateHolder.AgentStates).Return(agentStates);
		//    Expect.Call(() => agentState.SetSchedule(scheduleDictionary));
			
		//    mocks.ReplayAll();
		//    target.Models.Add(new DayLayerModel(person, period, team, new LayerViewModelCollection(eventAggregator, createLayerViewModelService), null));
		//    target.RefreshProjection(person);
		//    mocks.VerifyAll();
		//}

        //[Test]
        //public void VerifyCanRefreshScheduleData()
        //{
        //    DateTime now = DateTime.UtcNow;
        //    IActivity activity1 = new Activity("act1");
        //    IActivity activity2 = new Activity("act2");
        //    IVisualLayer activityLayer1 = layerFactory.CreateShiftSetupLayer(activity1, new DateTimePeriod(now.AddMinutes(-30), now.AddMinutes(1)),person);
        //    IVisualLayer activityLayer2 = layerFactory.CreateShiftSetupLayer(activity2, new DateTimePeriod(now.AddMinutes(1), now.AddDays(1)),person);
        //    IAgentState agentState = mocks.StrictMock<IAgentState>();
        //    IDictionary<IPerson, IAgentState> agentStates = new Dictionary<IPerson, IAgentState>();
        //    agentStates.Add(person, agentState);
        //    Expect.Call(rtaStateHolder.AgentStates).Return(agentStates);
        //    Expect.Call(agentState.FindCurrentAlarm(now)).Return(null);
        //    Expect.Call(agentState.FindCurrentState(now)).Return(null);
        //    Expect.Call(agentState.FindCurrentSchedule(now)).Return(activityLayer1);
        //    Expect.Call(agentState.FindNextSchedule(now)).Return(activityLayer2);
        //    mocks.ReplayAll();

        //    var model = new DayLayerModel(person, period, team, null, null);
        //    var updatedProperties = new List<string>();

        //    model.PropertyChanged += (sender, e) => updatedProperties.Add(e.PropertyName);

        //    target.Models.Add(model);
        //    target.Refresh(now);

        //    Assert.IsTrue(updatedProperties.Contains("NextActivityLayer"));
        //    Assert.IsTrue(updatedProperties.Contains("CurrentActivityLayer"));

        //    Assert.AreEqual(activityLayer2.Period, model.NextActivityLayer.Period);
        //    Assert.AreEqual(activityLayer2.Payload, model.NextActivityLayer.Payload);
        //    Assert.AreEqual(activityLayer1.Period, model.CurrentActivityLayer.Period);
        //    Assert.AreEqual(activityLayer1.Payload, model.CurrentActivityLayer.Payload);
        //    mocks.VerifyAll();
        //}

        [Test]
        public void ShouldNotHaveHookedEvents()
        {
            ((DayLayerViewModel)target).HookedEvents().Should().Be.EqualTo(0);
        }

        [Test]
        public void ShouldUnregisterFromMessageBroker()
        {
            using (mocks.Record())
            {
                Expect.Call(rtaStateHolder.SchedulingResultStateHolder).Return(schedulingResultStateHolder);
                Expect.Call(schedulingResultStateHolder.Schedules).Return(scheduleDictionary);

                Expect.Call(() => scheduleDictionary.PartModified -= target.OnScheduleModified).Repeat.Once().
                    IgnoreArguments();
            }
            using (mocks.Playback())
            {
                target.UnregisterMessageBrokerEvent();
            }
        }

        [Test]
        public void VerifyMessageBrokerEventTriggersRebuildIfScheduleBelongsToPerson()
        {
            var createLayerViewModelService = mocks.DynamicMock<ICreateLayerViewModelService>();
            var range = new ScheduleRange(scheduleDictionary,
                                          new ScheduleParameters(ScenarioFactory.CreateScenarioAggregate(), person, period));
            using (mocks.Record())
            {
                Expect.Call(rtaStateHolder.SchedulingResultStateHolder).Return(schedulingResultStateHolder);
                Expect.Call(schedulingResultStateHolder.Schedules).Return(scheduleDictionary);
                Expect.Call(scheduleDictionary[person]).Return(range);
                Expect.Call(createLayerViewModelService.CreateProjectionViewModelsFromSchedule(range, period, eventAggregator,
                                                                                               TimeSpan.FromMinutes(15))).Return(
                                                                                                new List<ILayerViewModel>());
            }
            using (mocks.Playback())
            {
                target.Models.Add(new DayLayerModel(person, period, team, new LayerViewModelCollection(eventAggregator, createLayerViewModelService), null));
                target.OnScheduleModified(this, new ModifyEventArgs(ScheduleModifier.MessageBroker, null, period));
                target.OnScheduleModified(this, new ModifyEventArgs(ScheduleModifier.MessageBroker, person, period));
            }
        }

        [Test]
        public void VerifyRebuildHasNullCheck()
        {
            using (mocks.Record())
            {
            }
            using (mocks.Playback())
            {
                target = new DayLayerViewModel(null, eventAggregator, unitOfWorkFactory, repositoryFactory, testDispatcher);
                target.Models.Add(new DayLayerModel(person, period, team, new LayerViewModelCollection(), null));
                target.OnScheduleModified(this, new ModifyEventArgs(ScheduleModifier.MessageBroker, person, period));
            }
        }
    }

    public class TestDispatcher : IDispatcherWrapper
    {
        public void BeginInvoke(Delegate method)
        {
            method.DynamicInvoke();
        }
    }
}
