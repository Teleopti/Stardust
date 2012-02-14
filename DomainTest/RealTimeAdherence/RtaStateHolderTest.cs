using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.RealTimeAdherence
{
    [TestFixture]
    public class RtaStateHolderTest
    {
        private MockRepository _mocks;
        private RtaStateHolder _target;
        private ISchedulingResultStateHolder _schedulingResultStateHolder;
        private IRtaStateGroup _rtaStateGroup;
        private readonly Guid _platformId = Guid.NewGuid();
        private IStateGroupActivityAlarm _stateGroupActivityAlarm;
        private IList<IRtaStateGroup> _rtaStateGroupList;
        private IRangeProjectionService _rangeProjectionService;
        private IRtaStateGroupProvider _rtaStateGroupProvider;
        private IStateGroupActivityAlarmProvider _stateGroupActivityAlarmProvider;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _schedulingResultStateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
            _rtaStateGroup = _mocks.StrictMock<IRtaStateGroup>();
            _stateGroupActivityAlarm = _mocks.StrictMock<IStateGroupActivityAlarm>();
            _rangeProjectionService = _mocks.StrictMock<IRangeProjectionService>();
            _rtaStateGroupProvider = _mocks.StrictMock<IRtaStateGroupProvider>();
            _stateGroupActivityAlarmProvider = _mocks.StrictMock<IStateGroupActivityAlarmProvider>();

            _rtaStateGroupList = new List<IRtaStateGroup> {_rtaStateGroup};

            _target = new RtaStateHolder(_schedulingResultStateHolder, _rtaStateGroupProvider,
                                         _stateGroupActivityAlarmProvider, _rangeProjectionService);
        }

        [Test]
        public void VerifyProperties()
        {
            using (_mocks.Record())
            {
                Expect.Call(_rtaStateGroup.StateCollection).Return(new ReadOnlyCollection<IRtaState>(new IRtaState[] {}));
                createStateGroupAndAlarmExpectation();
            }
            using (_mocks.Playback())
            {
                _target.Initialize();
                Assert.AreEqual(0, _target.AgentStates.Count);
                Assert.AreEqual(_schedulingResultStateHolder, _target.SchedulingResultStateHolder);
                Assert.IsTrue(_target.RtaStateGroups.Contains(_rtaStateGroup));
                Assert.IsTrue(_target.StateGroupActivityAlarms.Contains(_stateGroupActivityAlarm));
            }
        }

        [Test]
        public void VerifyCanAddDummyActivitiesFromConstructor()
        {
            IRtaState state = _mocks.StrictMock<IRtaState>();
            
            createStateGroupAndAlarmExpectation();
            Expect.Call(_rtaStateGroup.StateCollection).Return(
                new ReadOnlyCollection<IRtaState>(new List<IRtaState>{state}));
            Expect.Call(state.StateCode).Return("TEST");
            Expect.Call(state.Name).Return("Testar");

            _mocks.ReplayAll();
            _target = new RtaStateHolder(_schedulingResultStateHolder, _rtaStateGroupProvider, _stateGroupActivityAlarmProvider,_rangeProjectionService);
            _target.Initialize();
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyCanCollectAgentStates()
        {
            DateTime startDate = DateTime.UtcNow;
            IRtaState rtaState1 = _mocks.StrictMock<IRtaState>();
            IRtaState rtaState2 = _mocks.StrictMock<IRtaState>();
            Expect.Call(_rtaStateGroup.StateCollection).Return(
                new ReadOnlyCollection<IRtaState>(new List<IRtaState> { rtaState1, rtaState2 })).Repeat.Twice();
            Expect.Call(rtaState1.PlatformTypeId).Return(_platformId).Repeat.AtLeastOnce();
            Expect.Call(rtaState2.PlatformTypeId).Return(_platformId).Repeat.AtLeastOnce();
            Expect.Call(rtaState1.StateCode).Return("001").Repeat.AtLeastOnce();
            Expect.Call(rtaState2.StateCode).Return("002").Repeat.AtLeastOnce();

            IPerson person1 = createExpectationForPersonLogOn("LOGON44");
            IPerson person2 = createExpectationForPersonLogOn("LOGON45");
            
            IExternalAgentState externalAgentState = getExternalAgentState(startDate, "LOGON45", "002", TimeSpan.FromSeconds(12), DateTime.MinValue,false);

            Expect.Call(rtaState2.StateGroup).Return(_rtaStateGroup).Repeat.AtLeastOnce();
            Expect.Call(rtaState2.Name).Return("Name");
            Expect.Call(rtaState1.Name).Return("Name1");
            Expect.Call(_rtaStateGroup.IsLogOutState).Return(false);

            createStateGroupAndAlarmExpectation();
            createEmptyVisualLayerCollectionExpectation();

            _mocks.ReplayAll();
            _target.Initialize();
            _target.SetFilteredPersons(new List<IPerson> { person1,person2 });
            _target.CollectAgentStates(new List<IExternalAgentState>{externalAgentState});
            _mocks.VerifyAll();

            Assert.AreEqual(1,_target.AgentStates.Count);
            Assert.IsTrue(_target.AgentStates.ContainsKey(person2));
            Assert.AreEqual(_rtaStateGroup, _target.AgentStates[person2].RtaVisualLayerCollection[0].Payload);
        }

        [Test]
        public void VerifyCanHandleDoubleLogOffAgentStates()
        {
            DateTime startDate = DateTime.UtcNow;
            IRtaState rtaState1 = _mocks.StrictMock<IRtaState>();
            IRtaState rtaState2 = _mocks.StrictMock<IRtaState>();
            Expect.Call(_rtaStateGroup.StateCollection).Return(
                new ReadOnlyCollection<IRtaState>(new List<IRtaState> { rtaState1 })).Repeat.AtLeastOnce();
            IRtaStateGroup rtaStateGroup2 = _mocks.StrictMock<IRtaStateGroup>();
            _rtaStateGroupList.Add(rtaStateGroup2);
            Expect.Call(rtaStateGroup2.StateCollection).Return(
                new ReadOnlyCollection<IRtaState>(new List<IRtaState> { rtaState2 })).Repeat.AtLeastOnce();
            Expect.Call(rtaState1.PlatformTypeId).Return(_platformId).Repeat.AtLeastOnce();
            Expect.Call(rtaState2.PlatformTypeId).Return(_platformId).Repeat.AtLeastOnce();
            Expect.Call(rtaState1.StateCode).Return("001").Repeat.AtLeastOnce();
            Expect.Call(rtaState2.StateCode).Return("002").Repeat.AtLeastOnce();

            IPerson person1 = createExpectationForPersonLogOn("LOGON44");

            IExternalAgentState externalAgentState1 = getExternalAgentState(startDate, "LOGON44", "001", TimeSpan.FromSeconds(12), DateTime.MinValue, false);
            IExternalAgentState externalAgentState2 = getExternalAgentState(startDate.AddSeconds(12), "LOGON44", "002", TimeSpan.FromSeconds(6), DateTime.MinValue, false);
            IExternalAgentState externalAgentState3 = getExternalAgentState(startDate.AddSeconds(18), "LOGON44", "002", TimeSpan.Zero, DateTime.MinValue, false);

            Expect.Call(rtaState1.StateGroup).Return(_rtaStateGroup).Repeat.AtLeastOnce();
            Expect.Call(rtaState1.Name).Return("Name").Repeat.AtLeastOnce();
            Expect.Call(_rtaStateGroup.IsLogOutState).Return(false).Repeat.AtLeastOnce();
            Expect.Call(rtaState2.StateGroup).Return(rtaStateGroup2).Repeat.AtLeastOnce();
            Expect.Call(rtaState2.Name).Return("Name").Repeat.AtLeastOnce();
            Expect.Call(rtaStateGroup2.IsLogOutState).Return(true).Repeat.AtLeastOnce();

            createStateGroupAndAlarmExpectation();
            createEmptyVisualLayerCollectionExpectation();

            _mocks.ReplayAll();
            _target.Initialize();
            _target.SetFilteredPersons(new List<IPerson> { person1 });
            _target.CollectAgentStates(new List<IExternalAgentState> { externalAgentState1, externalAgentState2, externalAgentState3 });
            _mocks.VerifyAll();

            Assert.AreEqual(1, _target.AgentStates.Count);
            Assert.IsTrue(_target.AgentStates.ContainsKey(person1));
            Assert.AreEqual(1, _target.AgentStates[person1].RtaVisualLayerCollection.Count);
            Assert.IsTrue(_target.AgentStates[person1].RtaVisualLayerCollection[0].IsLoggedOut);
            Assert.AreEqual(TimeSpan.FromSeconds(18), _target.AgentStates[person1].RtaVisualLayerCollection[0].Period.ElapsedTime());
        }

        [Test]
        public void VerifyCanCollectAgentStatesInBatch()
        {
            DateTime startDate = DateTime.UtcNow;
            IRtaState rtaState1 = _mocks.StrictMock<IRtaState>();
            IRtaState rtaState2 = _mocks.StrictMock<IRtaState>();
            Expect.Call(_rtaStateGroup.StateCollection).Return(
                new ReadOnlyCollection<IRtaState>(new List<IRtaState> { rtaState1, rtaState2 })).Repeat.AtLeastOnce();
            Expect.Call(rtaState1.PlatformTypeId).Return(_platformId).Repeat.AtLeastOnce();
            Expect.Call(rtaState2.PlatformTypeId).Return(_platformId).Repeat.AtLeastOnce();
            Expect.Call(rtaState1.StateCode).Return("001").Repeat.AtLeastOnce();
            Expect.Call(rtaState2.StateCode).Return("002").Repeat.AtLeastOnce();

            IPerson person1 = createExpectationForPersonLogOn("LOGON44");
            IPerson person2 = createExpectationForPersonLogOn("LOGON45"); 
            
            IExternalAgentState externalAgentState1 = getExternalAgentState(startDate, "LOGON45", "002", TimeSpan.Zero, startDate,true);
            IExternalAgentState externalAgentState2 = getExternalAgentState(startDate, "LOGON44", "002", TimeSpan.Zero, startDate,true);
            IExternalAgentState externalAgentState3 = getExternalAgentState(startDate, "", "", TimeSpan.Zero, startDate, true); //End of batch
            IExternalAgentState externalAgentState4 = getExternalAgentState(startDate.AddSeconds(15),"","",TimeSpan.Zero, startDate.AddSeconds(15),true); //End of next batch

            Expect.Call(rtaState2.StateGroup).Return(_rtaStateGroup).Repeat.AtLeastOnce();
            Expect.Call(rtaState2.Name).Return("Name").Repeat.AtLeastOnce();
            Expect.Call(rtaState1.Name).Return("Name1").Repeat.AtLeastOnce();
            Expect.Call(_rtaStateGroup.IsLogOutState).Return(false).Repeat.AtLeastOnce();

            createStateGroupAndAlarmExpectation();
            createEmptyVisualLayerCollectionExpectation();

            _mocks.ReplayAll();
            _target.Initialize();
            _target.SetFilteredPersons(new List<IPerson> { person1, person2 });
            _target.CollectAgentStates(new List<IExternalAgentState> { externalAgentState1, externalAgentState2 });
            _target.CollectAgentStates(new List<IExternalAgentState> { externalAgentState3 });
            _target.CollectAgentStates(new List<IExternalAgentState> { externalAgentState4 });
            _mocks.VerifyAll();

            Assert.AreEqual(2, _target.AgentStates.Count);
            Assert.IsTrue(_target.AgentStates.ContainsKey(person1));
            Assert.IsTrue(_target.AgentStates.ContainsKey(person2));
            Assert.IsTrue(_target.AgentStates[person2].RtaVisualLayerCollection[0].IsLoggedOut);
            Assert.IsTrue(_target.AgentStates[person1].RtaVisualLayerCollection[0].IsLoggedOut);
        }

        [Test]
        public void VerifyCanCollectAgentStatesInBatchWithUpdatesBetween()
        {
            DateTime startDate = DateTime.UtcNow;
            IRtaState rtaState1 = _mocks.StrictMock<IRtaState>();
            IRtaState rtaState2 = _mocks.StrictMock<IRtaState>();
            Expect.Call(_rtaStateGroup.StateCollection).Return(
                new ReadOnlyCollection<IRtaState>(new List<IRtaState> { rtaState1, rtaState2 })).Repeat.AtLeastOnce();
            Expect.Call(rtaState1.PlatformTypeId).Return(_platformId).Repeat.AtLeastOnce();
            Expect.Call(rtaState2.PlatformTypeId).Return(_platformId).Repeat.AtLeastOnce();
            Expect.Call(rtaState1.StateCode).Return("001").Repeat.AtLeastOnce();
            Expect.Call(rtaState2.StateCode).Return("002").Repeat.AtLeastOnce();

            IPerson person1 = createExpectationForPersonLogOn("LOGON44");
            IPerson person2 = createExpectationForPersonLogOn("LOGON45");
            
            IExternalAgentState externalAgentState1 = getExternalAgentState(startDate, "", "", TimeSpan.Zero, startDate, true); //End of batch
            IExternalAgentState externalAgentState2 = getExternalAgentState(startDate.AddSeconds(15), "LOGON45", "002", TimeSpan.Zero, startDate.AddSeconds(15), false);
            IExternalAgentState externalAgentState3 = getExternalAgentState(startDate.AddSeconds(15), "LOGON44", "002", TimeSpan.Zero, startDate.AddSeconds(15), false);
            IExternalAgentState externalAgentState4 = getExternalAgentState(startDate.AddSeconds(30), "", "", TimeSpan.Zero, startDate.AddSeconds(30), true); //End of next batch

            Expect.Call(rtaState2.StateGroup).Return(_rtaStateGroup).Repeat.AtLeastOnce();
            Expect.Call(rtaState2.Name).Return("Name").Repeat.AtLeastOnce();
            Expect.Call(rtaState1.Name).Return("Name1");
            Expect.Call(_rtaStateGroup.IsLogOutState).Return(false).Repeat.AtLeastOnce();

            createStateGroupAndAlarmExpectation();
            createEmptyVisualLayerCollectionExpectation();

            _mocks.ReplayAll();
            _target.Initialize();
            _target.SetFilteredPersons(new List<IPerson> { person1, person2 });
            _target.CollectAgentStates(new List<IExternalAgentState> { externalAgentState1});
            _target.CollectAgentStates(new List<IExternalAgentState> { externalAgentState2, externalAgentState3 });

            Assert.AreEqual(2, _target.AgentStates.Count);
            Assert.IsTrue(_target.AgentStates.ContainsKey(person1));
            Assert.IsTrue(_target.AgentStates.ContainsKey(person2));
            Assert.IsFalse(_target.AgentStates[person2].RtaVisualLayerCollection[0].IsLoggedOut);
            Assert.IsFalse(_target.AgentStates[person1].RtaVisualLayerCollection[0].IsLoggedOut);
            
            _target.CollectAgentStates(new List<IExternalAgentState> { externalAgentState4 });

            Assert.IsTrue(_target.AgentStates[person2].RtaVisualLayerCollection[0].IsLoggedOut);
            Assert.IsTrue(_target.AgentStates[person1].RtaVisualLayerCollection[0].IsLoggedOut);
            _mocks.VerifyAll();
        }

        private IExternalAgentState getExternalAgentState(DateTime startDate, string externalLogOn, string stateCode, TimeSpan timeInState, DateTime batchIdentifier, bool isSnapShot)
        {
            return new ExternalAgentState(externalLogOn, stateCode, timeInState,
                                                                            startDate, _platformId, 1, batchIdentifier,
                                                                            isSnapShot);
        }

        [Test]
        public void VerifyCanUpdateCurrentLayers()
        {
            DateTime startDate = DateTime.UtcNow;
            IRtaState rtaState1 = _mocks.StrictMock<IRtaState>();
            IRtaState rtaState2 = _mocks.StrictMock<IRtaState>();
            Expect.Call(_rtaStateGroup.StateCollection).Return(
                new ReadOnlyCollection<IRtaState>(new List<IRtaState> { rtaState1, rtaState2 })).Repeat.Twice();
            Expect.Call(rtaState1.PlatformTypeId).Return(_platformId).Repeat.AtLeastOnce();
            Expect.Call(rtaState2.PlatformTypeId).Return(_platformId).Repeat.AtLeastOnce();
            Expect.Call(rtaState1.StateCode).Return("001").Repeat.AtLeastOnce();
            Expect.Call(rtaState2.StateCode).Return("002").Repeat.AtLeastOnce();

            IPerson person1 = createExpectationForPersonLogOn("LOGON44");
            IPerson person2 = createExpectationForPersonLogOn("LOGON45");
            
            IExternalAgentState externalAgentState = getExternalAgentState(startDate, "LOGON45", "002", TimeSpan.FromSeconds(12), DateTime.MinValue, false);

            Expect.Call(rtaState2.StateGroup).Return(_rtaStateGroup).Repeat.AtLeastOnce();
            Expect.Call(rtaState2.Name).Return("Name").Repeat.AtLeastOnce();
            Expect.Call(rtaState1.Name).Return("Name1").Repeat.AtLeastOnce();
            Expect.Call(_rtaStateGroup.IsLogOutState).Return(false).Repeat.AtLeastOnce();

            createStateGroupAndAlarmExpectation();
            createEmptyVisualLayerCollectionExpectation();

            _mocks.ReplayAll();
            _target.Initialize();
            _target.SetFilteredPersons(new List<IPerson> { person1, person2 });
            _target.CollectAgentStates(new List<IExternalAgentState> { externalAgentState });
            _target.UpdateCurrentLayers(startDate.AddHours(7),TimeSpan.FromSeconds(5));
            _mocks.VerifyAll();

            Assert.AreEqual(2, _target.AgentStates.Count);
            Assert.IsTrue(_target.AgentStates.ContainsKey(person2));
            Assert.AreEqual(_rtaStateGroup, _target.AgentStates[person2].RtaVisualLayerCollection[0].Payload);
        }

        [Test]
        public void VerifyCannotUpdateCurrentLayersBeforeScheduleIsInitialized()
        {
            DateTime startDate = DateTime.UtcNow;
            IRtaState rtaState1 = _mocks.StrictMock<IRtaState>();
            IRtaState rtaState2 = _mocks.StrictMock<IRtaState>();
            IActivity activity = ActivityFactory.CreateActivity("test");
            Expect.Call(_rtaStateGroup.StateCollection).Return(
                new ReadOnlyCollection<IRtaState>(new List<IRtaState> { rtaState1, rtaState2 })).Repeat.Twice();
            Expect.Call(rtaState1.PlatformTypeId).Return(_platformId).Repeat.AtLeastOnce();
            Expect.Call(rtaState2.PlatformTypeId).Return(_platformId).Repeat.AtLeastOnce();
            Expect.Call(rtaState1.StateCode).Return("001").Repeat.AtLeastOnce();
            Expect.Call(rtaState2.StateCode).Return("002").Repeat.AtLeastOnce();
            Expect.Call(_stateGroupActivityAlarm.Activity).Return(activity).Repeat.AtLeastOnce();

            IPerson person1 = createExpectationForPersonLogOn("LOGON44");
            IPerson person2 = createExpectationForPersonLogOn("LOGON45");

            IExternalAgentState externalAgentState = getExternalAgentState(startDate, "LOGON45", "002", TimeSpan.FromSeconds(12), DateTime.MinValue, false);

            Expect.Call(rtaState2.StateGroup).Return(_rtaStateGroup).Repeat.AtLeastOnce();
            Expect.Call(rtaState2.Name).Return("Name");
            Expect.Call(rtaState1.Name).Return("Name1");
            Expect.Call(_rtaStateGroup.IsLogOutState).Return(false).Repeat.AtLeastOnce();

            createStateGroupAndAlarmExpectation();
            createEmptyVisualLayerCollectionExpectation();

            _mocks.ReplayAll();
            _target.Initialize();
            _target.SetFilteredPersons(new List<IPerson> { person1, person2 });
            _target.CollectAgentStates(new List<IExternalAgentState> { externalAgentState });
            _target.UpdateCurrentLayers(startDate.AddHours(7), TimeSpan.FromSeconds(5));
            _target.AnalyzeAlarmSituations(startDate.AddHours(7));
            _mocks.VerifyAll();

            Assert.AreEqual(2, _target.AgentStates.Count);
            Assert.IsTrue(_target.AgentStates.ContainsKey(person2));
            Assert.AreEqual(_rtaStateGroup, _target.AgentStates[person2].RtaVisualLayerCollection[0].Payload);
        }

        [Test]
        public void VerifyCanCollectAgentStatesAndNotCreateNewAgentStateForEachEvent()
        {
            DateTime startDate = DateTime.UtcNow;
            IRtaState rtaState2 = _mocks.StrictMock<IRtaState>();
            Expect.Call(_rtaStateGroup.StateCollection).Return(
                new ReadOnlyCollection<IRtaState>(new List<IRtaState> {  rtaState2 })).Repeat.AtLeastOnce();
            Expect.Call(rtaState2.PlatformTypeId).Return(_platformId).Repeat.AtLeastOnce();
            Expect.Call(rtaState2.StateCode).Return("002").Repeat.AtLeastOnce();

            IPerson person1 = createExpectationForPersonLogOn("LOGON44");
            IPerson person2 = createExpectationForPersonLogOn("LOGON45");
            
            IExternalAgentState externalAgentState1 = getExternalAgentState(startDate, "LOGON45", "002", TimeSpan.FromSeconds(12), DateTime.MinValue,false);
            IExternalAgentState externalAgentState2 = getExternalAgentState(startDate, "LOGON45", "002", TimeSpan.FromSeconds(12), DateTime.MinValue,false);

            Expect.Call(rtaState2.StateGroup).Return(_rtaStateGroup).Repeat.AtLeastOnce();
            Expect.Call(rtaState2.Name).Return("State 2").Repeat.AtLeastOnce();
            Expect.Call(_rtaStateGroup.IsLogOutState).Return(false).Repeat.AtLeastOnce();
            Expect.Call(_rtaStateGroup.Equals(_rtaStateGroup)).Return(true).Repeat.AtLeastOnce();

            createStateGroupAndAlarmExpectation();
            createEmptyVisualLayerCollectionExpectation();

            _mocks.ReplayAll();
            _target.Initialize();
            _target.SetFilteredPersons(new List<IPerson> { person1, person2 });
            _target.CollectAgentStates(new List<IExternalAgentState> { externalAgentState1, externalAgentState2 });
            _mocks.VerifyAll();

            Assert.AreEqual(1, _target.AgentStates.Count);
            Assert.IsTrue(_target.AgentStates.ContainsKey(person2));
            Assert.AreEqual(1, _target.AgentStates[person2].RtaVisualLayerCollection.Count);
            Assert.AreEqual(_rtaStateGroup, _target.AgentStates[person2].RtaVisualLayerCollection[0].Payload);
        }

        [Test]
        public void VerifyCanCollectAgentStatesWithNewRtaState()
        {
            DateTime startDate = DateTime.UtcNow;
            IRtaState rtaState1 = _mocks.StrictMock<IRtaState>();
            IRtaState rtaState2 = _mocks.StrictMock<IRtaState>();
            IRtaState rtaState3 = _mocks.StrictMock<IRtaState>();
            Expect.Call(_rtaStateGroup.StateCollection).Return(
                new ReadOnlyCollection<IRtaState>(new List<IRtaState> { rtaState1, rtaState2 })).Repeat.Twice();
            Expect.Call(_rtaStateGroup.DefaultStateGroup).Return(true);
            Expect.Call(_rtaStateGroup.AddState("003", "003", _platformId)).Return(rtaState3);
            Expect.Call(rtaState1.PlatformTypeId).Return(_platformId).Repeat.AtLeastOnce();
            Expect.Call(rtaState2.PlatformTypeId).Return(_platformId).Repeat.AtLeastOnce();
            Expect.Call(rtaState1.StateCode).Return("001").Repeat.AtLeastOnce();
            Expect.Call(rtaState2.StateCode).Return("002").Repeat.AtLeastOnce();

            IPerson person1 = createExpectationForPersonLogOn("LOGON44");
            IPerson person2 = createExpectationForPersonLogOn("LOGON45");
            
            IExternalAgentState externalAgentState = getExternalAgentState(startDate, "LOGON45", "003", TimeSpan.FromSeconds(12), DateTime.MinValue,false);

            Expect.Call(rtaState1.Name).Return("Name1");
            Expect.Call(rtaState2.Name).Return("Name2");
            Expect.Call(rtaState3.StateGroup).Return(_rtaStateGroup).Repeat.AtLeastOnce();
            Expect.Call(rtaState3.Name).Return("State 3");
            Expect.Call(rtaState3.StateCode).Return("003").Repeat.AtLeastOnce();
            Expect.Call(_rtaStateGroup.IsLogOutState).Return(false).Repeat.AtLeastOnce();

            createStateGroupAndAlarmExpectation();
            createEmptyVisualLayerCollectionExpectation();

            _mocks.ReplayAll();
            bool succeeded = false;
            _target.Initialize();
            _target.RtaStateCreated += (x, y) => { succeeded = true; };
            _target.SetFilteredPersons(new List<IPerson> { person1, person2 });
            _target.CollectAgentStates(new List<IExternalAgentState> { externalAgentState });
            _mocks.VerifyAll();

            Assert.IsTrue(succeeded);
            Assert.AreEqual(1, _target.AgentStates.Count);
            Assert.IsTrue(_target.AgentStates.ContainsKey(person2));
            Assert.AreEqual(_rtaStateGroup, _target.AgentStates[person2].RtaVisualLayerCollection[0].Payload);
        }

        [Test]
        public void VerifyCanCollectAgentStatesWithNoRtaStateGroupAsDefault()
        {
            IRtaState rtaState1 = _mocks.StrictMock<IRtaState>();
            IRtaState rtaState2 = _mocks.StrictMock<IRtaState>();
            Expect.Call(_rtaStateGroup.StateCollection).Return(
                new ReadOnlyCollection<IRtaState>(new List<IRtaState> { rtaState1, rtaState2 })).Repeat.Twice();
            Expect.Call(_rtaStateGroup.DefaultStateGroup).Return(false);
            Expect.Call(rtaState1.PlatformTypeId).Return(_platformId).Repeat.AtLeastOnce();
            Expect.Call(rtaState2.PlatformTypeId).Return(_platformId).Repeat.AtLeastOnce();
            Expect.Call(rtaState1.StateCode).Return("001").Repeat.AtLeastOnce();
            Expect.Call(rtaState2.StateCode).Return("002").Repeat.AtLeastOnce();
            Expect.Call(rtaState1.Name).Return("Name1");
            Expect.Call(rtaState2.Name).Return("Name2");

            IPerson person1 = createExpectationForPersonLogOn("LOGON44");
            IPerson person2 = createExpectationForPersonLogOn("LOGON45");
            
            IExternalAgentState externalAgentState = _mocks.StrictMock<IExternalAgentState>();
            Expect.Call(externalAgentState.ExternalLogOn).Return("LOGON45").Repeat.AtLeastOnce();
            Expect.Call(externalAgentState.StateCode).Return("003").Repeat.AtLeastOnce();
            Expect.Call(externalAgentState.PlatformTypeId).Return(_platformId).Repeat.AtLeastOnce();
            Expect.Call(externalAgentState.DataSourceId).Return(1).Repeat.AtLeastOnce();

            createStateGroupAndAlarmExpectation();

            _mocks.ReplayAll();
            _target.Initialize();
            _target.SetFilteredPersons(new List<IPerson> { person1, person2 });
            _target.CollectAgentStates(new List<IExternalAgentState> { externalAgentState });
            _mocks.VerifyAll();

            Assert.AreEqual(0, _target.AgentStates.Count);
        }

        [Test]
        public void VerifyCanCollectAgentStatesWithNoMatchingLogOn()
        {
            IPerson person1 = createExpectationForPersonLogOn("LOGON44");
            IPerson person2 = createExpectationForPersonLogOn("LOGON45");
            
            IExternalAgentState externalAgentState = _mocks.StrictMock<IExternalAgentState>();
            Expect.Call(externalAgentState.ExternalLogOn).Return("LOGON46").Repeat.AtLeastOnce();

            _mocks.ReplayAll();
            _target.SetFilteredPersons(new List<IPerson> { person1, person2 });
            _target.CollectAgentStates(new List<IExternalAgentState> { externalAgentState });
            _mocks.VerifyAll();

            Assert.AreEqual(0, _target.AgentStates.Count);
        }

        [Test]
        public void VerifyCanExternalLogOnPersons()
        {
            IPerson person1 = createExpectationForPersonLogOn("LOGON44");
            IPerson person2 = createExpectationForPersonLogOn("LOGON45");

            _mocks.ReplayAll();
            _target.SetFilteredPersons(new List<IPerson> { person1, person2 });
            Assert.AreEqual(2,_target.ExternalLogOnPersons.Count);
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyCanAnalyzeAlarms()
        {
            DateTime startDate = DateTime.UtcNow;
            IRtaState rtaState2 = _mocks.StrictMock<IRtaState>();
            Expect.Call(_rtaStateGroup.StateCollection).Return(
                new ReadOnlyCollection<IRtaState>(new List<IRtaState> { rtaState2 })).Repeat.AtLeastOnce();
            Expect.Call(rtaState2.PlatformTypeId).Return(_platformId).Repeat.AtLeastOnce();
            Expect.Call(rtaState2.StateCode).Return("002").Repeat.AtLeastOnce();

            IPerson person1 = createExpectationForPersonLogOn("LOGON44");
            IPerson person2 = createExpectationForPersonLogOn("LOGON45");
            
            IExternalAgentState externalAgentState = getExternalAgentState(startDate, "LOGON45", "002", TimeSpan.Zero, DateTime.MinValue,false);

            Expect.Call(rtaState2.StateGroup).Return(_rtaStateGroup).Repeat.AtLeastOnce();
            Expect.Call(rtaState2.Name).Return("Name");
            Expect.Call(_rtaStateGroup.IsLogOutState).Return(false);

            createStateGroupAndAlarmExpectation();
            createEmptyVisualLayerCollectionExpectation();

            _mocks.ReplayAll();
            _target.Initialize();
            _target.SetFilteredPersons(new List<IPerson> { person1, person2 });
            _target.CollectAgentStates(new List<IExternalAgentState> { externalAgentState });
            _target.InitializeSchedules();
            _target.AnalyzeAlarmSituations(startDate);
            _mocks.VerifyAll();

            Assert.AreEqual(2, _target.AgentStates.Count);
            Assert.IsTrue(_target.AgentStates.ContainsKey(person2));
            Assert.AreEqual(_rtaStateGroup, _target.AgentStates[person2].RtaVisualLayerCollection[0].Payload);
        }

        private void createEmptyVisualLayerCollectionExpectation()
        {
            IScheduleDictionary scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
            IScheduleRange scheduleRange = _mocks.StrictMock<IScheduleRange>();
            Expect.Call(_schedulingResultStateHolder.Schedules).Return(scheduleDictionary).Repeat.AtLeastOnce();
            Expect.Call(scheduleDictionary[null]).IgnoreArguments().Return(scheduleRange).Repeat.AtLeastOnce();
            Expect.Call(_rangeProjectionService.CreateProjection(scheduleRange, new DateTimePeriod())).IgnoreArguments().
                Return(new List<IVisualLayer>()).Repeat.AtLeastOnce();
        }

        private void createStateGroupAndAlarmExpectation()
        {
            //Expect.Call(_rtaStateGroup.StateCollection).Return(new ReadOnlyCollection<IRtaState>(new List<IRtaState>()));
            Expect.Call(_rtaStateGroupProvider.StateGroups()).Return(_rtaStateGroupList);
            Expect.Call(_stateGroupActivityAlarmProvider.StateGroupActivityAlarms()).Return(new [] { _stateGroupActivityAlarm });
        }

        [Test, ExpectedException(typeof(DefaultStateGroupException))]
        public void VerifyCannotCreateInstanceWithNoRtaStateGroups()
        {
            using (_mocks.Record())
            {
                Expect.Call(_rtaStateGroupProvider.StateGroups()).Return(new IRtaStateGroup[] { });
                Expect.Call(_stateGroupActivityAlarmProvider.StateGroupActivityAlarms()).Return(new IStateGroupActivityAlarm[] { });
            }
            using (_mocks.Playback())
            {
                _target.Initialize();
                _target.VerifyDefaultStateGroupExists();
            }
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void VerifyCannotCreateInstanceWithNullRtaStateGroups()
        {
            _target = new RtaStateHolder(_schedulingResultStateHolder, null, _stateGroupActivityAlarmProvider, new RangeProjectionService());
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void VerifyCannotCreateInstanceWithNullStateGroupActivityAlarms()
        {
            _target = new RtaStateHolder(_schedulingResultStateHolder, _rtaStateGroupProvider, null, new RangeProjectionService());
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void VerifyCannotCreateInstanceWithNullStateHolder()
        {
            _target = new RtaStateHolder(null, _rtaStateGroupProvider, _stateGroupActivityAlarmProvider, new RangeProjectionService());
        }

        private IPerson createExpectationForPersonLogOn(string logon)
        {
            IPerson person1 = _mocks.StrictMock<IPerson>();
            IPersonPeriod personPeriod1 = _mocks.StrictMock<IPersonPeriod>();
            IExternalLogOn externalLogOn1 = _mocks.StrictMock<IExternalLogOn>();
            Expect.Call(person1.PersonPeriods(new DateOnlyPeriod())).IgnoreArguments().Return(
                new List<IPersonPeriod> {personPeriod1}).Repeat.AtLeastOnce();
            Expect.Call(personPeriod1.ExternalLogOnCollection).Return(
                new ReadOnlyCollection<IExternalLogOn>(new List<IExternalLogOn> { externalLogOn1 })).Repeat.AtLeastOnce();
            Expect.Call(externalLogOn1.AcdLogOnOriginalId).Return(logon).Repeat.AtLeastOnce();
            Expect.Call(externalLogOn1.DataSourceId).Return(1).Repeat.AtLeastOnce();
            return person1;
        }
    }
}
