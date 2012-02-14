using System;
using System.Collections.Generic;
using System.Drawing;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.RealTimeAdherence
{
    [TestFixture]
    public class AgentStateTest
    {
        private IAgentState target;
        private IPerson person;
        private IRtaVisualLayer rtaVisualLayer;
        private IScheduleRange scheduleRange;
        private MockRepository mocks;
        private DateTime timeStamp = new DateTime(2008, 11, 17, 0, 0, 0, DateTimeKind.Utc);
        private IRtaStateGroup stateGroup;
        private IAlarmType alarmType1;
        private IAlarmType alarmType2;
        private IAlarmType alarmType3;
        private IActivity activity;
        private IVisualLayerFactory layerFactory;
        private IRangeProjectionService _rangeProjectionService;

        [SetUp]
        public void Setup()
        {
            layerFactory = new VisualLayerFactory();
            mocks = new MockRepository();
            person = mocks.StrictMock<IPerson>();
            rtaVisualLayer = mocks.StrictMock<IRtaVisualLayer>();
            scheduleRange = mocks.StrictMock<IScheduleRange>();
            stateGroup = mocks.StrictMock<IRtaStateGroup>();
            alarmType1 = mocks.StrictMock<IAlarmType>();
            alarmType2 = mocks.StrictMock<IAlarmType>();
            alarmType3 = mocks.StrictMock<IAlarmType>();
            activity = ActivityFactory.CreateActivity("Phone");
            _rangeProjectionService = mocks.StrictMock<IRangeProjectionService>();
            
            target = new AgentState(person, _rangeProjectionService);
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.AreEqual(0,target.AlarmSituationCollection.Count);
            Assert.AreEqual(0,target.RtaVisualLayerCollection.Count);
            Assert.AreEqual(person, target.Person);
        }

        [Test]
        public void VerifyAddNewRtaVisualLayer()
        {
            target.AddRtaVisualLayer(rtaVisualLayer);
            Assert.IsTrue(target.RtaVisualLayerCollection.Contains(rtaVisualLayer));
        }

        [Test]
        public void VerifyCanFindCurrentState()
        {
            IRtaStateGroup g = new RtaStateGroup("sdf", false, true);
            g.AddState("sd", "sdfsdf", Guid.NewGuid());
            rtaVisualLayer = new RtaVisualLayer(g.StateCollection[0], DateTimeFactory.CreateDateTimePeriod(timeStamp, 0), new Activity("dsf"));

            mocks.ReplayAll();
            target.AddRtaVisualLayer(rtaVisualLayer);
            Assert.AreEqual(rtaVisualLayer,target.FindCurrentState(timeStamp.AddMinutes(5)));
            Assert.IsNull(target.FindCurrentState(timeStamp.AddMinutes(-5)));
            mocks.VerifyAll();
        }

        [Test]
        public void VerifyCanGetStartTimeForNextLayerWithTimeInState()
        {
            IRtaStateGroup g = new RtaStateGroup("sdf", false, true);
            g.AddState("sd", "sdfsdf", Guid.NewGuid());

            IExternalAgentState externalAgentState = new ExternalAgentState("007","AUX1",TimeSpan.FromSeconds(123),timeStamp,Guid.Empty,1,DateTime.Today,false);
            TimeSpan timeInState = TimeSpan.FromSeconds(123);
            target.LengthenOrCreateLayer(externalAgentState, g.StateCollection[0], activity);

            Assert.AreEqual(timeStamp.Add(timeInState.Negate()), target.RtaVisualLayerCollection[0].Period.StartDateTime);
        }

        [Test]
        public void VerifyCanGetStartTimeForLayerWithNoPreviousLayers()
        {
            IRtaStateGroup g = new RtaStateGroup("sdf", false, true);
            g.AddState("sd", "sdfsdf", Guid.NewGuid());

            IExternalAgentState externalAgentState = new ExternalAgentState("007", "AUX1", TimeSpan.Zero, timeStamp, Guid.Empty, 1, DateTime.Today, false);
            target.LengthenOrCreateLayer(externalAgentState, g.StateCollection[0], activity);

            Assert.AreEqual(timeStamp, target.RtaVisualLayerCollection[0].Period.StartDateTime);
        }

        [Test]
        public void VerifyCanGetStartTimeForNextLayerWithPreviousLayer()
        {
            IRtaStateGroup g = new RtaStateGroup("sdf", false, true);
            g.AddState("sd", "sdfsdf", Guid.NewGuid());

            Expect.Call(rtaVisualLayer.Period).Return(new DateTimePeriod(timeStamp, timeStamp.AddHours(5))).Repeat.AtLeastOnce();
            Expect.Call(rtaVisualLayer.IsLoggedOut).Return(false);
            Expect.Call(rtaVisualLayer.Payload).Return(g);
            Expect.Call(rtaVisualLayer.Period = new DateTimePeriod(timeStamp, timeStamp.AddHours(10)));

            IExternalAgentState externalAgentState = new ExternalAgentState("007", "AUX1", TimeSpan.Zero, timeStamp.AddHours(10), Guid.Empty, 1, DateTime.Today, false);
            
            mocks.ReplayAll();
            target.AddRtaVisualLayer(rtaVisualLayer);
            target.LengthenOrCreateLayer(externalAgentState, g.StateCollection[0], activity);
            mocks.VerifyAll();
        }

        [Test]
        public void VerifyCanGetStartTimeForNextLayerWithPreviousLayerEndingLaterThanNextLayer()
        {
            IRtaStateGroup g1 = new RtaStateGroup("sdf1", false, true);
            g1.AddState("AUX1", "sdfsdf 1", Guid.NewGuid());

            IRtaStateGroup g2 = new RtaStateGroup("sdf2", false, true);
            g2.AddState("AUX2", "sdfsdf 2", Guid.NewGuid());

            IRtaVisualLayer newRtaVisualLayer = new RtaVisualLayer(g1.StateCollection[0],
                                                                new DateTimePeriod(timeStamp, timeStamp.AddHours(5)),
                                                                activity);

            IExternalAgentState externalAgentState = new ExternalAgentState("007", "AUX2", TimeSpan.Zero, timeStamp.AddHours(4), Guid.Empty, 1, DateTime.Today, false);

            mocks.ReplayAll();
            target.AddRtaVisualLayer(newRtaVisualLayer);
            target.LengthenOrCreateLayer(externalAgentState, g2.StateCollection[0], activity);

            Assert.AreEqual(2,target.RtaVisualLayerCollection.Count);
            Assert.AreEqual(timeStamp.AddHours(4),target.FindCurrentState(timeStamp.AddHours(2)).Period.EndDateTime);
            Assert.AreEqual(timeStamp.AddHours(5), target.FindCurrentState(timeStamp.AddHours(4.5)).Period.EndDateTime);
            mocks.VerifyAll();
        }

        [Test]
        public void VerifyCanGetStartTimeForNextLayerWithPreviousLayerProblems()
        {
            IRtaStateGroup g1 = new RtaStateGroup("sdf1", false, true);
            g1.AddState("AUX1", "sdfsdf 1", Guid.NewGuid());

            IRtaStateGroup g2 = new RtaStateGroup("sdf2", false, true);
            g2.AddState("AUX2", "sdfsdf 2", Guid.NewGuid());

            IRtaVisualLayer newRtaVisualLayer = new RtaVisualLayer(g1.StateCollection[0],
                                                                new DateTimePeriod(timeStamp.AddSeconds(1), timeStamp.AddSeconds(2)),
                                                                activity);

            IExternalAgentState externalAgentState = new ExternalAgentState("007", "AUX2", TimeSpan.Zero, timeStamp, Guid.Empty, 1, DateTime.Today, false);

            mocks.ReplayAll();
            target.AddRtaVisualLayer(newRtaVisualLayer);
            target.LengthenOrCreateLayer(externalAgentState, g2.StateCollection[0], activity);

            Assert.AreEqual(2, target.RtaVisualLayerCollection.Count);
            mocks.VerifyAll();
        }

        [Test]
        public void VerifyCanGetStartTimeForNextLayerWithNegativeTimeInState()
        {
            IRtaStateGroup g1 = new RtaStateGroup("sdf1", false, true);
            g1.AddState("AUX1", "sdfsdf 1", Guid.NewGuid());

            IRtaStateGroup g2 = new RtaStateGroup("sdf2", false, true);
            g2.AddState("AUX2", "sdfsdf 2", Guid.NewGuid());

            IRtaVisualLayer newRtaVisualLayer = new RtaVisualLayer(g1.StateCollection[0],
                                                                new DateTimePeriod(timeStamp.AddSeconds(1), timeStamp.AddSeconds(2)),
                                                                activity);

            IExternalAgentState externalAgentState = new ExternalAgentState("007", "AUX2", TimeSpan.FromSeconds(-5), timeStamp, Guid.Empty, 1, DateTime.Today, false);

            mocks.ReplayAll();
            target.AddRtaVisualLayer(newRtaVisualLayer);
            target.LengthenOrCreateLayer(externalAgentState, g2.StateCollection[0], activity);

            Assert.AreEqual(2, target.RtaVisualLayerCollection.Count);
            mocks.VerifyAll();
        }

        [Test]
        public void VerifyCanLogOffWithPreviousLayer()
        {
            Expect.Call(rtaVisualLayer.Period).Return(new DateTimePeriod(timeStamp, timeStamp.AddHours(5))).Repeat.AtLeastOnce();
            Expect.Call(rtaVisualLayer.IsLoggedOut).Return(false);
            Expect.Call(rtaVisualLayer.IsLoggedOut = true);
            Expect.Call(rtaVisualLayer.Period = new DateTimePeriod(timeStamp, timeStamp.AddHours(10)));

            mocks.ReplayAll();
            target.AddRtaVisualLayer(rtaVisualLayer);
            target.LogOff(timeStamp.AddHours(10));
            Assert.AreEqual(1, target.RtaVisualLayerCollection.Count);
            mocks.VerifyAll();
        }

        [Test]
        public void VerifyCanLogOffWithLaterPreviousLayer()
        {
            Expect.Call(rtaVisualLayer.Period).Return(new DateTimePeriod(timeStamp, timeStamp.AddHours(5))).Repeat.AtLeastOnce();
            Expect.Call(rtaVisualLayer.IsLoggedOut).Return(false);
            Expect.Call(rtaVisualLayer.IsLoggedOut = true);
            Expect.Call(rtaVisualLayer.Period = new DateTimePeriod(timeStamp, timeStamp.AddHours(5)));

            mocks.ReplayAll();
            target.AddRtaVisualLayer(rtaVisualLayer);
            target.LogOff(timeStamp.AddHours(-1));
            Assert.AreEqual(1, target.RtaVisualLayerCollection.Count);
            mocks.VerifyAll();
        }

        [Test]
        public void VerifyCanLogOffWithPreviousLayerLoggedOff()
        {
            Expect.Call(rtaVisualLayer.Period).Return(new DateTimePeriod(timeStamp, timeStamp.AddHours(5))).Repeat.AtLeastOnce();
            Expect.Call(rtaVisualLayer.IsLoggedOut).Return(true).Repeat.AtLeastOnce();

            mocks.ReplayAll();
            target.AddRtaVisualLayer(rtaVisualLayer);
            target.LogOff(timeStamp.AddHours(10));
            Assert.AreEqual(1, target.RtaVisualLayerCollection.Count);
            Assert.AreEqual(TimeSpan.FromHours(5), target.RtaVisualLayerCollection[0].Period.ElapsedTime());
            Assert.IsTrue(target.RtaVisualLayerCollection[0].IsLoggedOut);
            mocks.VerifyAll();
        }

        [Test]
        public void VerifyCanGetStartTimeForNextLayerWithPreviousLayerLogOff()
        {
            IRtaStateGroup g = new RtaStateGroup("sdf", false, true);
            g.AddState("sd", "sdfsdf", Guid.NewGuid());

            Expect.Call(rtaVisualLayer.Period).Return(new DateTimePeriod(timeStamp, timeStamp.AddHours(5))).Repeat.AtLeastOnce();
            Expect.Call(rtaVisualLayer.IsLoggedOut).Return(true).Repeat.AtLeastOnce();
            Expect.Call(rtaVisualLayer.Payload).Return(g);

            IExternalAgentState externalAgentState = new ExternalAgentState("007", "AUX1", TimeSpan.Zero, timeStamp.AddHours(10), Guid.Empty, 1, DateTime.Today, false);

            mocks.ReplayAll();
            target.AddRtaVisualLayer(rtaVisualLayer);
            target.LengthenOrCreateLayer(externalAgentState, g.StateCollection[0],activity);
            Assert.AreEqual(timeStamp.AddHours(10), target.RtaVisualLayerCollection[1].Period.StartDateTime);
            mocks.VerifyAll();
        }

        [Test]
        public void VerifyCanGetStartTimeForNextLayerWithTooOldPreviousLayer()
        {
            IRtaStateGroup g = new RtaStateGroup("sdf", false, true);
            g.AddState("sd", "sdfsdf", Guid.NewGuid());

            Expect.Call(rtaVisualLayer.Period).Return(new DateTimePeriod(timeStamp, timeStamp.AddHours(5))).Repeat.AtLeastOnce();

            IExternalAgentState externalAgentState = new ExternalAgentState("007", "AUX1", TimeSpan.Zero, timeStamp.AddHours(12), Guid.Empty, 1, DateTime.Today, false);

            mocks.ReplayAll();
            target.AddRtaVisualLayer(rtaVisualLayer);
            target.LengthenOrCreateLayer(externalAgentState, g.StateCollection[0], activity);
            Assert.AreEqual(timeStamp.AddHours(12), target.RtaVisualLayerCollection[1].Period.StartDateTime);
            mocks.VerifyAll();
        }

        [Test]
        public void VerifyCanCreateAlarmSituations()
        {
            IVisualLayerCollection visualLayerCollection =
                new VisualLayerCollection(null, new List<IVisualLayer>
                                                	{
                                                		layerFactory.CreateShiftSetupLayer(activity, 
                                                		                                   new DateTimePeriod(timeStamp.AddMinutes(5), timeStamp.AddMinutes(60)))
                                                	}, new ProjectionPayloadMerger());

            IScheduleDictionary scheduleDictionary = GetScheduleDictionary(visualLayerCollection);
            IRtaState rtaState = mocks.StrictMock<IRtaState>();
            Expect.Call(alarmType1.ThresholdTime).Return(TimeSpan.FromSeconds(15)).Repeat.AtLeastOnce();
            Expect.Call(alarmType2.ThresholdTime).Return(TimeSpan.Zero).Repeat.AtLeastOnce();
            Expect.Call(alarmType1.Equals(alarmType2)).Return(false);
            Expect.Call(rtaState.StateGroup).Return(stateGroup);
            
            mocks.ReplayAll();
            rtaVisualLayer = new RtaVisualLayer(rtaState, new DateTimePeriod(timeStamp, timeStamp.AddMinutes(60)), activity);
            target.AddRtaVisualLayer(rtaVisualLayer);
            Assert.AreEqual(0,target.AlarmSituationCollection.Count);
            target.SetSchedule(scheduleDictionary);
            target.AnalyzeAlarmSituations(CreateStateGroupActivityAlarm(),timeStamp.AddHours(1));
            Assert.AreEqual(2, target.AlarmSituationCollection.Count);
            Assert.AreEqual(alarmType1, target.FindCurrentAlarm(timeStamp.AddMinutes(4)).Payload);
            Assert.AreEqual(alarmType2, target.FindCurrentAlarm(timeStamp.AddMinutes(6)).Payload);
            Assert.IsNull(target.FindCurrentAlarm(timeStamp.AddMinutes(61)));
            mocks.VerifyAll();
        }

		[Test]
		public void VerifyCanCreateAlarmSituationsWithMeeting()
		{
			var meetingPayload = mocks.StrictMock<IMeetingPayload>();
			var meetingLayer = mocks.StrictMock<IVisualLayer>();
			IVisualLayerCollection visualLayerCollection = new VisualLayerCollection(null, new List<IVisualLayer> {meetingLayer},
			                                                                         new ProjectionPayloadMerger());

			IScheduleDictionary scheduleDictionary = GetScheduleDictionary(visualLayerCollection);
			IRtaState rtaState = mocks.StrictMock<IRtaState>();
			Expect.Call(meetingPayload.UnderlyingPayload).Return(activity).Repeat.AtLeastOnce();
			Expect.Call(alarmType1.ThresholdTime).Return(TimeSpan.FromSeconds(15)).Repeat.AtLeastOnce();
			Expect.Call(alarmType2.ThresholdTime).Return(TimeSpan.Zero).Repeat.AtLeastOnce();
			Expect.Call(alarmType1.Equals(alarmType2)).Return(false);
			Expect.Call(rtaState.StateGroup).Return(stateGroup);
			Expect.Call(meetingLayer.Period).Return(new DateTimePeriod(timeStamp.AddMinutes(5), timeStamp.AddMinutes(60))).Repeat
				.AtLeastOnce();
			Expect.Call(meetingLayer.Payload).Return(meetingPayload);
			Expect.Call(meetingLayer.EntityClone()).Return(meetingLayer);

			mocks.ReplayAll();
			rtaVisualLayer = new RtaVisualLayer(rtaState, new DateTimePeriod(timeStamp, timeStamp.AddMinutes(60)), activity);
			target.AddRtaVisualLayer(rtaVisualLayer);
			Assert.AreEqual(0, target.AlarmSituationCollection.Count);
			target.SetSchedule(scheduleDictionary);
			target.AnalyzeAlarmSituations(CreateStateGroupActivityAlarm(), timeStamp.AddHours(1));
			Assert.AreEqual(2, target.AlarmSituationCollection.Count);
			Assert.AreEqual(alarmType1, target.FindCurrentAlarm(timeStamp.AddMinutes(4)).Payload);
			Assert.AreEqual(alarmType2, target.FindCurrentAlarm(timeStamp.AddMinutes(6)).Payload);
			Assert.IsNull(target.FindCurrentAlarm(timeStamp.AddMinutes(61)));
			mocks.VerifyAll();
		}

        [Test]
        public void VerifyCanCreateAlarmSituationsAndGetMergedTimeInAlarm()
        {
            IVisualLayerCollection visualLayerCollection =
                new VisualLayerCollection(null, new List<IVisualLayer>
                                                	{
                                                		layerFactory.CreateShiftSetupLayer(activity, 
                                                		                                   new DateTimePeriod(timeStamp.AddMinutes(5), timeStamp.AddMinutes(60)))
                                                	}, new ProjectionPayloadMerger());

            IScheduleDictionary scheduleDictionary = GetScheduleDictionary(visualLayerCollection);
            IRtaState rtaState = mocks.StrictMock<IRtaState>();
            alarmType1 = new AlarmType(new Description("Alarma!"), Color.DimGray, TimeSpan.Zero,
                                       AlarmTypeMode.UserDefined, 1);
            alarmType2 = alarmType1;
            Expect.Call(rtaState.StateGroup).Return(stateGroup);

            mocks.ReplayAll();
            rtaVisualLayer = new RtaVisualLayer(rtaState, new DateTimePeriod(timeStamp, timeStamp.AddMinutes(60)), activity);
            target.AddRtaVisualLayer(rtaVisualLayer);
            Assert.AreEqual(0, target.AlarmSituationCollection.Count);
            target.SetSchedule(scheduleDictionary);
            target.AnalyzeAlarmSituations(CreateStateGroupActivityAlarm(), timeStamp.AddHours(1));
            Assert.AreEqual(1, target.AlarmSituationCollection.Count);
            Assert.AreEqual(alarmType1, target.FindCurrentAlarm(timeStamp.AddMinutes(4)).Payload);
            Assert.AreEqual(alarmType1, target.FindCurrentAlarm(timeStamp.AddMinutes(6)).Payload);
            Assert.IsNull(target.FindCurrentAlarm(timeStamp.AddMinutes(61)));
            mocks.VerifyAll();
        }

        [Test]
        public void VerifyCanGetScheduleInformation()
        {
            IActivity activity1 = ActivityFactory.CreateActivity("act1");
            IActivity activity2 = ActivityFactory.CreateActivity("act2");
            IVisualLayer layer1 = layerFactory.CreateShiftSetupLayer(activity1,
                                                                     new DateTimePeriod(timeStamp.AddMinutes(5),
                                                                                        timeStamp.AddMinutes(60)));
            IVisualLayer layer2 = layerFactory.CreateShiftSetupLayer(activity2,
                                                                     new DateTimePeriod(timeStamp.AddMinutes(60),
                                                                                        timeStamp.AddMinutes(65)));

            IVisualLayerCollection visualLayerCollection =
                new VisualLayerCollection(null, new List<IVisualLayer> { layer1, layer2 }, new ProjectionPayloadMerger());

            IScheduleDictionary scheduleDictionary = GetScheduleDictionary(visualLayerCollection);
            
            mocks.ReplayAll();
            target.SetSchedule(scheduleDictionary);
            Assert.IsNull(target.FindCurrentSchedule(timeStamp));
            Assert.AreEqual(layer1.Period, target.FindCurrentSchedule(timeStamp.AddMinutes(6)).Period);
            Assert.AreEqual(layer2.Period, target.FindNextSchedule(timeStamp.AddMinutes(6)).Period);
            Assert.AreEqual(layer2.Period, target.FindCurrentSchedule(timeStamp.AddMinutes(61)).Period);
            Assert.IsNull(target.FindNextSchedule(timeStamp.AddMinutes(61)));
            
            mocks.VerifyAll();
        }

        [Test]
        public void VerifyCanCreateAlarmSituationsWhenHavingShortenedWorkTime()
        {
            IVisualLayerCollection visualLayerCollection =
                new VisualLayerCollection(null, new List<IVisualLayer>
                                                	{
                                                		layerFactory.CreateShiftSetupLayer(activity,
                                                		                                   new DateTimePeriod(timeStamp.AddMinutes(5),
                                                		                                                      timeStamp.AddMinutes(55)))
                                                	}, new ProjectionPayloadMerger());

            IScheduleDictionary scheduleDictionary = GetScheduleDictionary(visualLayerCollection);
            IRtaState rtaState = mocks.StrictMock<IRtaState>();
            alarmType1 = new AlarmType(new Description("alarm1"), Color.DodgerBlue, TimeSpan.FromSeconds(15),
                                       AlarmTypeMode.UserDefined,-1d);
            alarmType2 = new AlarmType(new Description("alarm2"), Color.ForestGreen, TimeSpan.Zero,
                                       AlarmTypeMode.Ok, 0);
            Expect.Call(rtaState.StateGroup).Return(stateGroup);

            mocks.ReplayAll();
            rtaVisualLayer = new RtaVisualLayer(rtaState, new DateTimePeriod(timeStamp, timeStamp.AddMinutes(60)),activity);
            target.AddRtaVisualLayer(rtaVisualLayer);
            Assert.AreEqual(0, target.AlarmSituationCollection.Count);
            target.SetSchedule(scheduleDictionary);
            target.AnalyzeAlarmSituations(CreateStateGroupActivityAlarm(),timeStamp.AddHours(1));
            Assert.AreEqual(3, target.AlarmSituationCollection.Count);
            mocks.VerifyAll();
        }

        [Test]
        public void VerifyCanCreateAlarmSituationsWithNoRtaData()
        {
            IVisualLayerCollection visualLayerCollection =
                new VisualLayerCollection(null, new List<IVisualLayer>
                                                	{
                                                		layerFactory.CreateShiftSetupLayer(activity,
                                                		                                   new DateTimePeriod(timeStamp.AddMinutes(5),
                                                		                                                      timeStamp.AddMinutes(60)))
                                                	}, new ProjectionPayloadMerger());

            IScheduleDictionary scheduleDictionary = GetScheduleDictionary(visualLayerCollection);
            Expect.Call(alarmType3.ThresholdTime).Return(TimeSpan.FromSeconds(15)).Repeat.AtLeastOnce();
            
            mocks.ReplayAll();
            Assert.AreEqual(0, target.AlarmSituationCollection.Count);
            target.SetSchedule(scheduleDictionary);
            target.AnalyzeAlarmSituations(CreateStateGroupActivityAlarm(),timeStamp.AddHours(1));
            Assert.AreEqual(1, target.AlarmSituationCollection.Count);
            mocks.VerifyAll();
        }

        [Test]
        public void VerifyCanClearAlarmSituations()
        {
            IVisualLayerCollection visualLayerCollection =
                new VisualLayerCollection(null, new List<IVisualLayer>
                                                	{
                                                		layerFactory.CreateShiftSetupLayer(activity,
                                                		                                   new DateTimePeriod(timeStamp.AddMinutes(5),
                                                		                                                      timeStamp.AddMinutes(60)))
                                                	}, new ProjectionPayloadMerger());

            IScheduleDictionary scheduleDictionary = GetScheduleDictionary(visualLayerCollection);
            Expect.Call(alarmType3.ThresholdTime).Return(TimeSpan.FromSeconds(15)).Repeat.AtLeastOnce();
            
            mocks.ReplayAll();
            Assert.AreEqual(0, target.AlarmSituationCollection.Count);
            target.SetSchedule(scheduleDictionary);
            target.AnalyzeAlarmSituations(CreateStateGroupActivityAlarm(), timeStamp.AddHours(1));
            Assert.AreEqual(1, target.AlarmSituationCollection.Count);
            target.ClearAlarmSituations();
            Assert.AreEqual(0, target.AlarmSituationCollection.Count);
            mocks.VerifyAll();
        }

        [Test]
        public void VerifyCanCreateAlarmSituationsWhenHavingNoSchedule()
        {
            IVisualLayerCollection visualLayerCollection =
                new VisualLayerCollection(null, new List<IVisualLayer>(), new ProjectionPayloadMerger());

            IScheduleDictionary scheduleDictionary = GetScheduleDictionary(visualLayerCollection);
            IRtaState rtaState = mocks.StrictMock<IRtaState>();
            Expect.Call(alarmType1.ThresholdTime).Return(TimeSpan.FromSeconds(15)).Repeat.AtLeastOnce();
            Expect.Call(rtaState.StateGroup).Return(stateGroup);

            mocks.ReplayAll();
            rtaVisualLayer = new RtaVisualLayer(rtaState, new DateTimePeriod(timeStamp, timeStamp.AddMinutes(60)),activity);
            target.AddRtaVisualLayer(rtaVisualLayer);
            Assert.AreEqual(0, target.AlarmSituationCollection.Count);
            target.SetSchedule(scheduleDictionary);
            target.AnalyzeAlarmSituations(CreateStateGroupActivityAlarm(), timeStamp.AddHours(1));
            Assert.AreEqual(1, target.AlarmSituationCollection.Count);
            mocks.VerifyAll();
        }

        [Test]
        public void VerifyCanCreateAlarmSituationsAndCutAlarmByTimestamp()
        {
            IVisualLayerCollection visualLayerCollection =
                new VisualLayerCollection(null, new List<IVisualLayer>(), new ProjectionPayloadMerger());

            IScheduleDictionary scheduleDictionary = GetScheduleDictionary(visualLayerCollection);
            IRtaState rtaState = mocks.StrictMock<IRtaState>();
            Expect.Call(alarmType1.ThresholdTime).Return(TimeSpan.FromSeconds(15)).Repeat.AtLeastOnce();
            Expect.Call(rtaState.StateGroup).Return(stateGroup);

            mocks.ReplayAll();
            rtaVisualLayer = new RtaVisualLayer(rtaState, new DateTimePeriod(timeStamp, timeStamp.AddMinutes(60)), activity);
            target.AddRtaVisualLayer(rtaVisualLayer);
            Assert.AreEqual(0, target.AlarmSituationCollection.Count);
            target.SetSchedule(scheduleDictionary);
            target.AnalyzeAlarmSituations(CreateStateGroupActivityAlarm(), timeStamp.AddMinutes(30));
            Assert.AreEqual(1, target.AlarmSituationCollection.Count);
            mocks.VerifyAll();
        }

        [Test]
        public void VerifyCanLengthenLayerWithoutNewMessage()
        {
            Expect.Call(rtaVisualLayer.Period).Return(new DateTimePeriod(timeStamp, timeStamp.AddHours(5))).Repeat.AtLeastOnce();
            Expect.Call(rtaVisualLayer.IsLoggedOut).Return(false);
            rtaVisualLayer.Period =
                new DateTimePeriod(timeStamp, timeStamp.AddHours(6).Add(TimeSpan.FromSeconds(5)).AddTicks(1));

            mocks.ReplayAll();
            target.AddRtaVisualLayer(rtaVisualLayer);
            target.UpdateCurrentLayer(timeStamp.AddHours(6), TimeSpan.FromSeconds(5));
            mocks.VerifyAll();
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void VerifyScheduleMustBeSetPriorToAlarmCalculations()
        {
            mocks.ReplayAll();
            target.AnalyzeAlarmSituations(CreateStateGroupActivityAlarm(), timeStamp);
            mocks.VerifyAll();
        }

        [Test]
        public void VerifyNoLengthenLayerWithoutNewMessageAndNotAvailable()
        {
            Expect.Call(rtaVisualLayer.Period).Return(new DateTimePeriod(timeStamp, timeStamp.AddHours(5))).Repeat.AtLeastOnce();
            Expect.Call(rtaVisualLayer.IsLoggedOut).Return(true);

            mocks.ReplayAll();
            target.AddRtaVisualLayer(rtaVisualLayer);
            target.UpdateCurrentLayer(timeStamp.AddHours(6), TimeSpan.FromSeconds(5));
            mocks.VerifyAll();
        }

        [Test]
        public void VerifyNoLengthenLayerWhenNoPreviousLayerFound()
        {
            Expect.Call(rtaVisualLayer.Period).Return(new DateTimePeriod(timeStamp, timeStamp.AddHours(7))).Repeat.AtLeastOnce();
            Expect.Call(rtaVisualLayer.IsLoggedOut).Return(false);

            mocks.ReplayAll();
            target.AddRtaVisualLayer(rtaVisualLayer);
            target.UpdateCurrentLayer(timeStamp.AddHours(6), TimeSpan.FromSeconds(5));
            mocks.VerifyAll();
        }

        [Test]
        public void VerifyCompleteCaseWithScheduleEightToSeventeen()
        {
            IActivity phoneActivity = ActivityFactory.CreateActivity("Phone");
            IActivity lunchActivity = ActivityFactory.CreateActivity("Lunch");
            IActivity shortBreakActivity = ActivityFactory.CreateActivity("Short break");
            VisualLayerFactory visualLayerFactory = new VisualLayerFactory();
            IList<IVisualLayer> visualLayers = new List<IVisualLayer>();
            visualLayers.Add(visualLayerFactory.CreateShiftSetupLayer(phoneActivity,new DateTimePeriod(timeStamp.AddHours(8),timeStamp.AddHours(10))));
            visualLayers.Add(visualLayerFactory.CreateShiftSetupLayer(shortBreakActivity, new DateTimePeriod(timeStamp.AddHours(10), timeStamp.AddHours(10).AddMinutes(30))));
            visualLayers.Add(visualLayerFactory.CreateShiftSetupLayer(phoneActivity, new DateTimePeriod(timeStamp.AddHours(10).AddMinutes(30), timeStamp.AddHours(12))));
            visualLayers.Add(visualLayerFactory.CreateShiftSetupLayer(lunchActivity, new DateTimePeriod(timeStamp.AddHours(12), timeStamp.AddHours(13))));
            visualLayers.Add(visualLayerFactory.CreateShiftSetupLayer(phoneActivity, new DateTimePeriod(timeStamp.AddHours(13), timeStamp.AddHours(17))));
            IVisualLayerCollection visualLayerCollection =
                new VisualLayerCollection(null, visualLayers, new ProjectionPayloadMerger());

            IRtaStateGroup rtaStateGroupIn = new RtaStateGroup("Phone", true, true);
            rtaStateGroupIn.AddState("Ready","Ready",Guid.Empty);
            rtaStateGroupIn.AddState("In call", "In call", Guid.Empty);
            IRtaStateGroup rtaStateGroupPause = new RtaStateGroup("Pause", false, false);
            rtaStateGroupPause.AddState("Pause", "Pause", Guid.Empty);
            IRtaStateGroup rtaStateGroupLunch = new RtaStateGroup("Lunch", false, false);
            rtaStateGroupLunch.AddState("Lunch", "Lunch", Guid.Empty);
            IRtaStateGroup rtaStateGroupOut = new RtaStateGroup("Out", false, false);
            rtaStateGroupOut.AddState("Out", "Out", Guid.Empty);
            rtaStateGroupOut.IsLogOutState = true;

            IActivity dummyIn = ActivityFactory.CreateActivity(rtaStateGroupIn.Name);
            IActivity dummyPause = ActivityFactory.CreateActivity(rtaStateGroupPause.Name);
            IActivity dummyLunch = ActivityFactory.CreateActivity(rtaStateGroupLunch.Name);

            IAlarmType alarmTypeOut = new AlarmType(new Description("Out of adherence"), Color.Empty, TimeSpan.Zero,
                                                 AlarmTypeMode.UserDefined, -1);
            IAlarmType alarmTypeIn = new AlarmType(new Description("In adherence"), Color.Empty, TimeSpan.Zero,
                                                 AlarmTypeMode.Ok, 0);
            IAlarmType alarmTypeOutside = new AlarmType(new Description("Outside work"), Color.Empty, TimeSpan.Zero,
                                                 AlarmTypeMode.UserDefined, 1);

            IList<IStateGroupActivityAlarm> stateGroupActivityAlarms = new List<IStateGroupActivityAlarm>
                                                                           {
                                                                               new StateGroupActivityAlarm(
                                                                                   rtaStateGroupIn, phoneActivity)
                                                                                   {AlarmType = alarmTypeIn},
                                                                               new StateGroupActivityAlarm(
                                                                                   rtaStateGroupIn, lunchActivity)
                                                                                   {AlarmType = alarmTypeOutside},
                                                                               new StateGroupActivityAlarm(
                                                                                   rtaStateGroupIn, null)
                                                                                   {AlarmType = alarmTypeOutside},
                                                                               new StateGroupActivityAlarm(
                                                                                   rtaStateGroupIn, shortBreakActivity)
                                                                                   {AlarmType = alarmTypeOutside},
                                                                               new StateGroupActivityAlarm(
                                                                                   rtaStateGroupLunch, phoneActivity)
                                                                                   {AlarmType = alarmTypeOut},
                                                                               new StateGroupActivityAlarm(
                                                                                   rtaStateGroupLunch, lunchActivity)
                                                                                   {AlarmType = alarmTypeIn},
                                                                               new StateGroupActivityAlarm(
                                                                                   rtaStateGroupLunch, null)
                                                                                   {AlarmType = alarmTypeIn},
                                                                               new StateGroupActivityAlarm(
                                                                                   rtaStateGroupLunch,
                                                                                   shortBreakActivity)
                                                                                   {AlarmType = alarmTypeIn},
                                                                               new StateGroupActivityAlarm(
                                                                                   rtaStateGroupPause, phoneActivity)
                                                                                   {AlarmType = alarmTypeOut},
                                                                               new StateGroupActivityAlarm(
                                                                                   rtaStateGroupPause, lunchActivity)
                                                                                   {AlarmType = alarmTypeIn},
                                                                               new StateGroupActivityAlarm(
                                                                                   rtaStateGroupPause, null)
                                                                                   {AlarmType = alarmTypeIn},
                                                                               new StateGroupActivityAlarm(
                                                                                   rtaStateGroupPause,
                                                                                   shortBreakActivity)
                                                                                   {AlarmType = alarmTypeIn},
                                                                               new StateGroupActivityAlarm(
                                                                                   rtaStateGroupOut, phoneActivity)
                                                                                   {AlarmType = alarmTypeOut},
                                                                               new StateGroupActivityAlarm(
                                                                                   rtaStateGroupOut, lunchActivity)
                                                                                   {AlarmType = alarmTypeIn},
                                                                               new StateGroupActivityAlarm(
                                                                                   rtaStateGroupOut, null)
                                                                                   {AlarmType = alarmTypeIn},
                                                                               new StateGroupActivityAlarm(
                                                                                   rtaStateGroupOut, shortBreakActivity)
                                                                                   {AlarmType = alarmTypeIn},
                                                                               new StateGroupActivityAlarm(null,
                                                                                                           phoneActivity)
                                                                                   {AlarmType = alarmTypeOut},
                                                                               new StateGroupActivityAlarm(null,
                                                                                                           lunchActivity)
                                                                                   {AlarmType = alarmTypeOut},
                                                                               new StateGroupActivityAlarm(null,
                                                                                                           shortBreakActivity)
                                                                                   {AlarmType = alarmTypeOut}
                                                                           };

            IScheduleDictionary scheduleDictionary = GetScheduleDictionary(visualLayerCollection);
            mocks.ReplayAll();

            target.SetSchedule(scheduleDictionary);

            target.LengthenOrCreateLayer(new ExternalAgentState("10","Ready",TimeSpan.Zero,timeStamp.AddHours(7).AddMinutes(43),Guid.Empty,1,DateTime.MinValue,false), rtaStateGroupIn.StateCollection[0], dummyIn);
            //17 min : Outside
            target.LengthenOrCreateLayer(new ExternalAgentState("10", "Call", TimeSpan.Zero, timeStamp.AddHours(7).AddMinutes(56), Guid.Empty, 1, DateTime.MinValue, false), rtaStateGroupIn.StateCollection[1], dummyIn);
            target.AnalyzeAlarmSituations(stateGroupActivityAlarms, timeStamp.AddHours(7).AddMinutes(59));
            target.LengthenOrCreateLayer(new ExternalAgentState("10", "Ready", TimeSpan.Zero, timeStamp.AddHours(8).AddMinutes(15), Guid.Empty, 1, DateTime.MinValue, false), rtaStateGroupIn.StateCollection[0], dummyIn);
            target.LengthenOrCreateLayer(new ExternalAgentState("10", "Call", TimeSpan.Zero, timeStamp.AddHours(8).AddMinutes(44), Guid.Empty, 1, DateTime.MinValue, false), rtaStateGroupIn.StateCollection[1], dummyIn);
            target.LengthenOrCreateLayer(new ExternalAgentState("10", "Ready", TimeSpan.Zero, timeStamp.AddHours(9).AddMinutes(40), Guid.Empty, 1, DateTime.MinValue, false), rtaStateGroupIn.StateCollection[0], dummyIn);
            target.AnalyzeAlarmSituations(stateGroupActivityAlarms, timeStamp.AddHours(9).AddMinutes(43));
            target.LengthenOrCreateLayer(new ExternalAgentState("10", "Pause", TimeSpan.Zero, timeStamp.AddHours(9).AddMinutes(47), Guid.Empty, 1, DateTime.MinValue, false), rtaStateGroupPause.StateCollection[0], dummyPause);
            target.AnalyzeAlarmSituations(stateGroupActivityAlarms, timeStamp.AddHours(10).AddMinutes(5));
            //13 min : Out
            target.LengthenOrCreateLayer(new ExternalAgentState("10", "Ready", TimeSpan.Zero, timeStamp.AddHours(10).AddMinutes(18), Guid.Empty, 1, DateTime.MinValue, false), rtaStateGroupIn.StateCollection[0], dummyIn);
            //12 min : Outside
            target.LengthenOrCreateLayer(new ExternalAgentState("10", "Call", TimeSpan.Zero, timeStamp.AddHours(11).AddMinutes(43), Guid.Empty, 1, DateTime.MinValue, false), rtaStateGroupIn.StateCollection[1], dummyIn);
            target.LengthenOrCreateLayer(new ExternalAgentState("10", "Ready", TimeSpan.Zero, timeStamp.AddHours(11).AddMinutes(56), Guid.Empty, 1, DateTime.MinValue, false), rtaStateGroupIn.StateCollection[0], dummyIn);
            target.AnalyzeAlarmSituations(stateGroupActivityAlarms, timeStamp.AddHours(11).AddMinutes(58));
            target.LengthenOrCreateLayer(new ExternalAgentState("10", "Call", TimeSpan.Zero, timeStamp.AddHours(12).AddMinutes(14), Guid.Empty, 1, DateTime.MinValue, false), rtaStateGroupIn.StateCollection[1], dummyIn);
            target.LengthenOrCreateLayer(new ExternalAgentState("10", "Lunch", TimeSpan.Zero, timeStamp.AddHours(12).AddMinutes(16), Guid.Empty, 1, DateTime.MinValue, false), rtaStateGroupLunch.StateCollection[0], dummyLunch);
            //16 min : Outside
            target.LengthenOrCreateLayer(new ExternalAgentState("10", "Call", TimeSpan.Zero, timeStamp.AddHours(13).AddMinutes(2), Guid.Empty, 1, DateTime.MinValue, false), rtaStateGroupIn.StateCollection[1], dummyIn);
            target.AnalyzeAlarmSituations(stateGroupActivityAlarms, timeStamp.AddHours(13).AddMinutes(28));
            //2 min : Out
            target.LengthenOrCreateLayer(new ExternalAgentState("10", "Ready", TimeSpan.Zero, timeStamp.AddHours(13).AddMinutes(37), Guid.Empty, 1, DateTime.MinValue, false), rtaStateGroupIn.StateCollection[0], dummyIn);
            target.LengthenOrCreateLayer(new ExternalAgentState("10", "Call", TimeSpan.Zero, timeStamp.AddHours(14).AddMinutes(24), Guid.Empty, 1, DateTime.MinValue, false), rtaStateGroupIn.StateCollection[1], dummyIn);
            target.AnalyzeAlarmSituations(stateGroupActivityAlarms, timeStamp.AddHours(15).AddMinutes(20));
            target.LengthenOrCreateLayer(new ExternalAgentState("10", "Ready", TimeSpan.Zero, timeStamp.AddHours(15).AddMinutes(59), Guid.Empty, 1, DateTime.MinValue, false), rtaStateGroupIn.StateCollection[0], dummyIn);
            target.LengthenOrCreateLayer(new ExternalAgentState("10", "Call", TimeSpan.Zero, timeStamp.AddHours(16).AddMinutes(16), Guid.Empty, 1, DateTime.MinValue, false), rtaStateGroupIn.StateCollection[1], dummyIn);
            target.LogOff(timeStamp.AddHours(16).AddMinutes(56));
            //4 min : Out

            target.AnalyzeAlarmSituations(stateGroupActivityAlarms, timeStamp.AddHours(17).AddMinutes(10));
            Assert.AreEqual(11,target.AlarmSituationCollection.Count);
            mocks.VerifyAll();
        }


        [Test]
        public void VerifyCompleteCaseFromCustomerRegardingTimeInState()
        {
            IActivity phoneActivity = ActivityFactory.CreateActivity("Phone");
            VisualLayerFactory visualLayerFactory = new VisualLayerFactory();
            IList<IVisualLayer> visualLayers = new List<IVisualLayer>();
            visualLayers.Add(visualLayerFactory.CreateShiftSetupLayer(phoneActivity, new DateTimePeriod(timeStamp, timeStamp.AddHours(4))));
            IVisualLayerCollection visualLayerCollection =
                new VisualLayerCollection(null, visualLayers, new ProjectionPayloadMerger());

            IRtaStateGroup rtaStateGroupIn = new RtaStateGroup("Phone", true, true);
            rtaStateGroupIn.AddState("Ready", "Ready", Guid.Empty);
            rtaStateGroupIn.AddState("In call", "In call", Guid.Empty);
            IRtaStateGroup rtaStateGroupPause = new RtaStateGroup("Pause", false, false);
            rtaStateGroupPause.AddState("Pause", "Pause", Guid.Empty);

            IActivity dummyIn = ActivityFactory.CreateActivity(rtaStateGroupIn.Name);
            IActivity dummyPause = ActivityFactory.CreateActivity(rtaStateGroupPause.Name);

            IAlarmType alarmTypeOut = new AlarmType(new Description("Out of adherence"), Color.Empty, TimeSpan.Zero,
                                                 AlarmTypeMode.UserDefined, -1);
            IAlarmType alarmTypeIn = new AlarmType(new Description("In adherence"), Color.Empty, TimeSpan.Zero,
                                                 AlarmTypeMode.Ok, 0);

            IList<IStateGroupActivityAlarm> stateGroupActivityAlarms = new List<IStateGroupActivityAlarm>
                                                                           {
                                                                               new StateGroupActivityAlarm(
                                                                                   rtaStateGroupIn, phoneActivity)
                                                                                   {AlarmType = alarmTypeIn},
                                                                               new StateGroupActivityAlarm(
                                                                                   rtaStateGroupPause, phoneActivity)
                                                                                   {AlarmType = alarmTypeOut},
                                                                               new StateGroupActivityAlarm(
                                                                                   rtaStateGroupPause, null)
                                                                                   {AlarmType = alarmTypeIn},
                                                                               new StateGroupActivityAlarm(null,
                                                                                                           phoneActivity)
                                                                                   {AlarmType = alarmTypeOut},
                                                                           };

            IScheduleDictionary scheduleDictionary = GetScheduleDictionary(visualLayerCollection);
            mocks.ReplayAll();

            target.SetSchedule(scheduleDictionary);

            target.LengthenOrCreateLayer(new ExternalAgentState("10", "Ready", TimeSpan.Zero, timeStamp, Guid.Empty, 1, DateTime.MinValue, false), rtaStateGroupIn.StateCollection[0], dummyIn);
            target.AnalyzeAlarmSituations(stateGroupActivityAlarms, timeStamp);
            target.LengthenOrCreateLayer(new ExternalAgentState("10", "Pause", TimeSpan.FromSeconds(30), timeStamp.AddSeconds(40), Guid.Empty, 1, DateTime.MinValue, false), rtaStateGroupPause.StateCollection[0], dummyPause);
            target.AnalyzeAlarmSituations(stateGroupActivityAlarms, timeStamp.AddSeconds(40));
            rtaVisualLayer = target.FindCurrentState(timeStamp.AddSeconds(9));
            Assert.AreEqual(rtaStateGroupIn,rtaVisualLayer.Payload);
            rtaVisualLayer = target.FindCurrentState(timeStamp.AddSeconds(39));
            Assert.AreEqual(rtaStateGroupPause, rtaVisualLayer.Payload);
            target.UpdateCurrentLayer(timeStamp.AddSeconds(40),TimeSpan.FromSeconds(1));
            rtaVisualLayer = target.FindCurrentState(timeStamp.AddSeconds(40));
            Assert.AreEqual(rtaStateGroupPause, rtaVisualLayer.Payload);

            mocks.VerifyAll();
        }

        private IScheduleDictionary GetScheduleDictionary(IEnumerable<IVisualLayer> visualLayerCollection)
        {
            IScheduleDictionary scheduleDictionary = mocks.StrictMock<IScheduleDictionary>();
            Expect.Call(scheduleDictionary[person]).Return(scheduleRange);
            Expect.Call(_rangeProjectionService.CreateProjection(scheduleRange, new DateTimePeriod())).IgnoreArguments()
                .Return(visualLayerCollection);
            return scheduleDictionary;
        }

        private IList<IStateGroupActivityAlarm> CreateStateGroupActivityAlarm()
        {
            IStateGroupActivityAlarm stateGroupActivityAlarm1 = new StateGroupActivityAlarm(stateGroup, null);
            stateGroupActivityAlarm1.AlarmType = alarmType1;
            IStateGroupActivityAlarm stateGroupActivityAlarm2 = new StateGroupActivityAlarm(stateGroup, activity);
            stateGroupActivityAlarm2.AlarmType = alarmType2;
            IStateGroupActivityAlarm stateGroupActivityAlarm3 = new StateGroupActivityAlarm(null, activity);
            stateGroupActivityAlarm3.AlarmType = alarmType3;

            return new List<IStateGroupActivityAlarm>
                       {stateGroupActivityAlarm1, stateGroupActivityAlarm2, stateGroupActivityAlarm3};
        }
    }
}
