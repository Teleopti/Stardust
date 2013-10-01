using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.Practices.Composite.Events;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Intraday;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Is = NUnit.Framework.Is;

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
        private IEventAggregator eventAggregator;
        private IUnitOfWorkFactory unitOfWorkFactory;
        private IRepositoryFactory repositoryFactory;
        private IDispatcherWrapper testDispatcher;
        private DateOnlyPeriod dateOnlyPeriod;
		private DayLayerModel daylayerModel;
		private Guid guid;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            rtaStateHolder = mocks.DynamicMock<IRtaStateHolder>();
            schedulingResultStateHolder = mocks.StrictMock<ISchedulingResultStateHolder>();
            scheduleDictionary = mocks.StrictMock<IScheduleDictionary>();
            eventAggregator = mocks.DynamicMock<IEventAggregator>();
            testDispatcher = new TestDispatcher();
            team = TeamFactory.CreateSimpleTeam();
            period = new DateTimePeriod(2012, 10, 25, 2012, 10, 25);
            dateOnlyPeriod = new DateOnlyPeriod(2012, 10, 25, 2012, 10, 25);
			guid = Guid.NewGuid();
            person = PersonFactory.CreatePerson();
            person.SetId(guid);
            person.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(new DateOnly(), team));
            person.PermissionInformation.SetDefaultTimeZone(
                (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time")));

            unitOfWorkFactory = mocks.DynamicMock<IUnitOfWorkFactory>();
            repositoryFactory = mocks.DynamicMock<IRepositoryFactory>();
	        daylayerModel = new DayLayerModel(person, new DateTimePeriod(2013, 01, 01, 2059, 01, 01), team,
	                                          new LayerViewModelCollection(new EventAggregator(),
	                                                                       new CreateLayerViewModelService(),
	                                                                       new RemoveLayerFromSchedule(), null), null);

            target = new DayLayerViewModel(rtaStateHolder, eventAggregator, unitOfWorkFactory, repositoryFactory, testDispatcher);
        }

        [Test]
        public void ShouldCreateModels()
        {
			rtaStateHolder.BackToRecord();
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
				Expect.Call(scheduleDay.HasProjection()).Return(false).Repeat.AtLeastOnce();
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
			rtaStateHolder.BackToRecord();
			IActualAgentState agentState = new ActualAgentState();
			agentState.AlarmStart = DateTime.UtcNow.AddMinutes(-45);
			agentState.State = "MyCurrentStateDescription";
			agentState.AlarmName = "MyAlarmName";
			var dictionary = new Dictionary<Guid, IActualAgentState> { { (Guid)person.Id, agentState } };

			target.Models.Add(new DayLayerModel(person, new DateTimePeriod(DateTime.UtcNow.Date, DateTime.UtcNow.Date),
												new Team(), new LayerViewModelCollection(new EventAggregator(), new CreateLayerViewModelService(), new RemoveLayerFromSchedule(), null), new CommonNameDescriptionSetting()));
			rtaStateHolder.Expect(r => r.ActualAgentStates).IgnoreArguments().Return(dictionary);
			mocks.ReplayAll();

			target.RefreshElapsedTime(DateTime.UtcNow);
			mocks.VerifyAll();

			Assert.That(target.Models.First().Person.Id, Is.EqualTo(person.Id));
			Assert.That(target.Models.First().AlarmDescription, Is.EqualTo(agentState.AlarmName));
		}
		
        [Test]
        public void ShouldNotHaveHookedEvents()
        {
            ((DayLayerViewModel)target).HookedEvents().Should().Be.EqualTo(0);
        }

        [Test]
        public void ShouldUnregisterFromMessageBroker()
		{
			rtaStateHolder.BackToRecord();
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
			rtaStateHolder.BackToRecord();
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
				target.Models.Add(new DayLayerModel(person, period, team, new LayerViewModelCollection(eventAggregator, createLayerViewModelService, new RemoveLayerFromSchedule(), null), null));
                target.OnScheduleModified(this, new ModifyEventArgs(ScheduleModifier.MessageBroker, null, period));
                target.OnScheduleModified(this, new ModifyEventArgs(ScheduleModifier.MessageBroker, person, period));
            }
        }

        [Test]
        public void VerifyRebuildHasNullCheck()
		{
			rtaStateHolder.BackToRecord();

            using (mocks.Record())
            {
            }
            using (mocks.Playback())
            {
                target = new DayLayerViewModel(null, eventAggregator, unitOfWorkFactory, repositoryFactory, testDispatcher);
				target.Models.Add(new DayLayerModel(person, period, team, new LayerViewModelCollection(new EventAggregator(), new CreateLayerViewModelService(), new RemoveLayerFromSchedule(), null), null));
                target.OnScheduleModified(this, new ModifyEventArgs(ScheduleModifier.MessageBroker, person, period));
            }
        }

		[Test]
		public void ShouldUpdateAgentState()
		{
			var actualAgentState = new ActualAgentState
				{
					PersonId = guid,
					ScheduledNext = "New Next Activity",
					AlarmStart = DateTime.UtcNow.AddMinutes(-10),
					AlarmName = "New Alarm Name"
				};
			var customEventArgs = new CustomEventArgs<IActualAgentState>(actualAgentState);
			person.SetId(guid);

			daylayerModel.NextActivityDescription = "New Next Activity";
			daylayerModel.AlarmDescription = "New Alarm Name";
			target.Models.Add(daylayerModel);
			rtaStateHolder.Raise(r => r.AgentstateUpdated += null, this, customEventArgs);

			var result = target.Models.FirstOrDefault(d => d.Person.Id == guid);
			result.NextActivityDescription.Should().Be.EqualTo(actualAgentState.ScheduledNext);
			result.AlarmDescription.Should().Be.EqualTo(actualAgentState.AlarmName);
		}

		[Test]
		public void ShouldUpdateAgentState_EmptyUpdate()
		{
			rtaStateHolder.Raise(r => r.AgentstateUpdated += null, this,
			                     new CustomEventArgs<IActualAgentState>(new ActualAgentState()));
		}

		[Test]
		public void ShouldInitializeRows()
		{
			target.Models.Add(daylayerModel);
			IActualAgentState actualAgentState;
			rtaStateHolder.Expect(r => r.ActualAgentStates.TryGetValue(guid, out actualAgentState)).Return(false);
			target.InitializeRows();
		}

		[Test]
		public void ShouldSetSortDirection()
		{
			target.SetCurrentSortDescription(new SortDescription("propertyName", ListSortDirection.Ascending));
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
