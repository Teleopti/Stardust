using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Composite.Events;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Intraday;
using Teleopti.Ccc.TestCommon.FakeData;

using Teleopti.Wfm.Adherence.Domain.Configuration;
using Teleopti.Wfm.Adherence.Domain.Service;
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
	                                                                       new RemoveLayerFromSchedule(), null, new FullPermission()), null);

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
                target.CreateModels(new[] { person }, new DateOnlyPeriodAsDateTimePeriod(dateOnlyPeriod, TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone));
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
			AgentStateReadModel agentStateReadModel = new AgentStateReadModel();
			agentStateReadModel.RuleStartTime = DateTime.UtcNow.AddMinutes(-45);
			agentStateReadModel.StateName = "MyCurrentStateDescription";
			agentStateReadModel.RuleName = "MyAlarmName";
			var dictionary = new Dictionary<Guid, AgentStateReadModel> { { (Guid)person.Id, agentStateReadModel } };

			target.Models.Add(new DayLayerModel(person, new DateTimePeriod(DateTime.UtcNow.Date, DateTime.UtcNow.Date),
												new Team(), new LayerViewModelCollection(new EventAggregator(), new CreateLayerViewModelService(), new RemoveLayerFromSchedule(), null, new FullPermission()), new CommonNameDescriptionSetting()));
			rtaStateHolder.Expect(r => r.ActualAgentStates).IgnoreArguments().Return(dictionary);
			mocks.ReplayAll();

			target.RefreshElapsedTime(DateTime.UtcNow);
			mocks.VerifyAll();

			Assert.That(target.Models.First().Person.Id, Is.EqualTo(person.Id));
			Assert.That(target.Models.First().AlarmDescription, Is.EqualTo(agentStateReadModel.RuleName));
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
                                          new ScheduleParameters(ScenarioFactory.CreateScenarioAggregate(), person, period), new PersistableScheduleDataPermissionChecker(CurrentAuthorization.Make()), CurrentAuthorization.Make());
			var authorization = CurrentAuthorization.Make().Current();
			using (mocks.Record())
            {
                Expect.Call(rtaStateHolder.SchedulingResultStateHolder).Return(schedulingResultStateHolder);
                Expect.Call(schedulingResultStateHolder.Schedules).Return(scheduleDictionary);
                Expect.Call(scheduleDictionary[person]).Return(range);
                Expect.Call(createLayerViewModelService.CreateProjectionViewModelsFromSchedule(range, period, eventAggregator,
                                                                                               TimeSpan.FromMinutes(15), authorization)).Return(
                                                                                                new List<ILayerViewModel>());
            }
            using (mocks.Playback())
            {
				target.Models.Add(new DayLayerModel(person, period, team, new LayerViewModelCollection(eventAggregator, createLayerViewModelService, new RemoveLayerFromSchedule(), null, authorization), null));
                target.OnScheduleModified(this, new ModifyEventArgs(ScheduleModifier.MessageBroker, null, period, null));
                target.OnScheduleModified(this, new ModifyEventArgs(ScheduleModifier.MessageBroker, person, period, null));
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
				target.Models.Add(new DayLayerModel(person, period, team, new LayerViewModelCollection(new EventAggregator(), new CreateLayerViewModelService(), new RemoveLayerFromSchedule(), null, new FullPermission()), null));
                target.OnScheduleModified(this, new ModifyEventArgs(ScheduleModifier.MessageBroker, person, period, null));
            }
        }

		[Test]
		public void ShouldUpdateAgentState()
		{
			var actualAgentState = new AgentStateReadModel
				{
					PersonId = guid,
					NextActivity = "New Next Activity",
					RuleStartTime = DateTime.UtcNow.AddMinutes(-10),
					RuleName = "New Alarm Name"
				};
			var customEventArgs = new CustomEventArgs<AgentStateReadModel>(actualAgentState);
			person.SetId(guid);

			daylayerModel.NextActivityDescription = "New Next Activity";
			daylayerModel.AlarmDescription = "New Alarm Name";
			target.Models.Add(daylayerModel);
			rtaStateHolder.Raise(r => r.AgentstateUpdated += null, this, customEventArgs);

			var result = target.Models.FirstOrDefault(d => d.Person.Id == guid);
			result.NextActivityDescription.Should().Be.EqualTo(actualAgentState.NextActivity);
			result.AlarmDescription.Should().Be.EqualTo(actualAgentState.RuleName);
		}

		[Test]
		public void ShouldUpdateAgentState_EmptyUpdate()
		{
			rtaStateHolder.Raise(r => r.AgentstateUpdated += null, this,
			                     new CustomEventArgs<AgentStateReadModel>(new AgentStateReadModel()));
		}

		[Test]
		public void ShouldInitializeRows()
		{
			target.Models.Add(daylayerModel);
			AgentStateReadModel agentStateReadModel;
			rtaStateHolder.Expect(r => r.ActualAgentStates.TryGetValue(guid, out agentStateReadModel)).Return(false);
			target.InitializeRows();
		}

		[Test]
		public void ShouldIndicateAlarm_WhenAlarmIdIsNotEmpty()
		{
			var person = PersonFactory.CreatePerson();
			person.SetId(Guid.NewGuid());
			var actualAgentState = new AgentStateReadModel
			{
				PersonId = person.Id.Value,
				RuleName = "Alarm"
			};
			var rtaStateHolder = new RtaStateHolder(new SchedulingResultStateHolder(), MockRepository.GenerateMock<IRtaStateGroupRepository>());
			rtaStateHolder.SetFilteredPersons(new [] {person});
			rtaStateHolder.SetActualAgentState(actualAgentState);
			var target1 = new DayLayerViewModel(rtaStateHolder, null, null, null, null);
			var dayLayerModel = new DayLayerModel(person, new DateTimePeriod(), null, null, null);
			target1.Models.Add(dayLayerModel);

			target1.InitializeRows();

			target1.Models.Single().HasAlarm.Should().Be.True();

		}

		[Test]
		public void ShouldIndicateNoAlarm_WhenAlarmIdIsEmpty()
		{
			var person = PersonFactory.CreatePerson();
			person.SetId(Guid.NewGuid());
			var actualAgentState = new AgentStateReadModel
			{
				PersonId = person.Id.Value,
			};
			var rtaStateHolder = new RtaStateHolder(new SchedulingResultStateHolder(), MockRepository.GenerateMock<IRtaStateGroupRepository>());
			rtaStateHolder.SetFilteredPersons(new[] { person });
			rtaStateHolder.SetActualAgentState(actualAgentState);
			var target = new DayLayerViewModel(rtaStateHolder, null, null, null, null);
			var dayLayerModel = new DayLayerModel(person, new DateTimePeriod(), null, null, null);
			target.Models.Add(dayLayerModel);

			target.InitializeRows();

			daylayerModel.HasAlarm.Should().Be.False();
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
