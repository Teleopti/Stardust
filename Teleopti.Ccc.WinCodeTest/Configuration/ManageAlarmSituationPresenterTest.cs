using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using NUnit.Framework;
using Rhino.Mocks;
using Syncfusion.Styles;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Common.Configuration;
using Teleopti.Ccc.WinCode.Intraday;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.WinCodeTest.Configuration
{
    [TestFixture]
    public class ManageAlarmSituationPresenterTest : IDisposable
    {
        private MockRepository mocks;
        private ManageAlarmSituationPresenter _target;
        private IActivityRepository _activityRepository;                                                            
        private IAlarmTypeRepository _alarmTypeRepository;
        private IRtaStateGroupRepository _rtaStateGroupRepository;
        private IStateGroupActivityAlarmRepository _stateGroupActvityAlarmRepository;
        private IMessageBroker _messageBroker;
        private IManageAlarmSituationView _manageAlarmSituationView;

        [SetUp]
        public void CreateManageSituationPresenter()
        {
            mocks = new MockRepository();

            _messageBroker = mocks.StrictMock<IMessageBroker>();
            _activityRepository = mocks.StrictMock<IActivityRepository>();
            _alarmTypeRepository = mocks.StrictMock<IAlarmTypeRepository>();
            _rtaStateGroupRepository = mocks.StrictMock<IRtaStateGroupRepository>();
            _stateGroupActvityAlarmRepository = mocks.StrictMock<IStateGroupActivityAlarmRepository>();
            _manageAlarmSituationView = mocks.StrictMock<IManageAlarmSituationView>();
        }

        [Test]
        public void VerifyAddAlarmType()
        {
            Guid alarmTypeId = Guid.NewGuid();
            IAlarmType alarmType = mocks.StrictMock<IAlarmType>();
            IEventMessage eventMessage = mocks.StrictMock<IEventMessage>();
            Expect.Call(eventMessage.DomainUpdateType).Return(DomainUpdateType.Insert).Repeat.AtLeastOnce();
            Expect.Call(eventMessage.DomainObjectId).Return(alarmTypeId);

            IList<IAlarmType> alarmTypes = new List<IAlarmType>();
            PrepareMockForLoad(new List<IRtaStateGroup>(), alarmTypes, new List<IActivity>(), new List<IStateGroupActivityAlarm>());
            Expect.Call(_alarmTypeRepository.Get(alarmTypeId)).Return(alarmType);
            _manageAlarmSituationView.RefreshGrid();

            mocks.ReplayAll();

            _target = new ManageAlarmSituationPresenter(_alarmTypeRepository, _rtaStateGroupRepository,
                                                        _activityRepository, _stateGroupActvityAlarmRepository,
                                                        _messageBroker, _manageAlarmSituationView);
            _target.Load();

            _target.OnAlarmEvent(null, new EventMessageArgs(eventMessage));
            
            mocks.VerifyAll();
        }

        [Test]
        public void VerifyUpdateAlarmType()
        {
            Guid alarmTypeId = Guid.NewGuid();
            IAlarmType alarmType = mocks.StrictMock<IAlarmType>();
            IEventMessage eventMessage = mocks.StrictMock<IEventMessage>();
            Expect.Call(eventMessage.DomainUpdateType).Return(DomainUpdateType.Update).Repeat.AtLeastOnce();
            Expect.Call(eventMessage.DomainObjectId).Return(alarmTypeId).Repeat.AtLeastOnce();
            Expect.Call(alarmType.Id).Return(alarmTypeId).Repeat.AtLeastOnce();

			IList<IAlarmType> alarmTypes = new List<IAlarmType>();
			alarmTypes.Add(alarmType);
            PrepareMockForLoad(new List<IRtaStateGroup>(), alarmTypes, new List<IActivity>(), new List<IStateGroupActivityAlarm>());
            Expect.Call(_alarmTypeRepository.Get(alarmTypeId)).Return(alarmType);
            _manageAlarmSituationView.RefreshGrid();

            mocks.ReplayAll();

            _target = new ManageAlarmSituationPresenter(_alarmTypeRepository, _rtaStateGroupRepository,
                                                        _activityRepository, _stateGroupActvityAlarmRepository,
                                                        _messageBroker, _manageAlarmSituationView);
            _target.Load();

            Assert.AreEqual(1, alarmTypes.Count);
            _target.OnAlarmEvent(null, new EventMessageArgs(eventMessage));
            Assert.AreEqual(1, alarmTypes.Count);

            mocks.VerifyAll();
        }

        [Test]
        public void VerifyDeleteAlarmType()
        {
            Guid alarmTypeId = Guid.NewGuid();
            IAlarmType alarmType = mocks.StrictMock<IAlarmType>();
            IEventMessage eventMessage = mocks.StrictMock<IEventMessage>();
            Expect.Call(eventMessage.DomainUpdateType).Return(DomainUpdateType.Delete).Repeat.AtLeastOnce();
            Expect.Call(eventMessage.DomainObjectId).Return(alarmTypeId).Repeat.AtLeastOnce();
            Expect.Call(alarmType.Id).Return(alarmTypeId).Repeat.AtLeastOnce();

            IList<IAlarmType> alarmTypes = new List<IAlarmType> { alarmType };
            PrepareMockForLoad(new List<IRtaStateGroup>(), alarmTypes, new List<IActivity>(), new List<IStateGroupActivityAlarm>());
            _manageAlarmSituationView.RefreshGrid();

            mocks.ReplayAll();

            _target = new ManageAlarmSituationPresenter(_alarmTypeRepository, _rtaStateGroupRepository,
                                                        _activityRepository, _stateGroupActvityAlarmRepository,
                                                        _messageBroker, _manageAlarmSituationView);
            _target.Load();

            _target.OnAlarmEvent(null, new EventMessageArgs(eventMessage));
            
            mocks.VerifyAll();
        }

        [Test]
        public void VerifyAddRtaStateGroup()
        {
            Guid rtaStateGroupId = Guid.NewGuid();
            IRtaStateGroup rtaStateGroup = mocks.StrictMock<IRtaStateGroup>();
            IEventMessage eventMessage = mocks.StrictMock<IEventMessage>();
            Expect.Call(eventMessage.DomainUpdateType).Return(DomainUpdateType.Insert).Repeat.AtLeastOnce();
            Expect.Call(eventMessage.DomainObjectId).Return(rtaStateGroupId);

            IList<IRtaStateGroup> rtaStateGroups = new List<IRtaStateGroup>();
            PrepareMockForLoad(rtaStateGroups, new List<IAlarmType>(), new List<IActivity>(), new List<IStateGroupActivityAlarm>());
            Expect.Call(_rtaStateGroupRepository.Get(rtaStateGroupId)).Return(rtaStateGroup);
            _manageAlarmSituationView.RefreshGrid();

            mocks.ReplayAll();

            _target = new ManageAlarmSituationPresenter(_alarmTypeRepository, _rtaStateGroupRepository,
                                                        _activityRepository, _stateGroupActvityAlarmRepository,
                                                        _messageBroker, _manageAlarmSituationView);
            _target.Load();

            Assert.AreEqual(1, _target.ColCount);
            _target.OnRtaStateGroupEvent(null, new EventMessageArgs(eventMessage));
            Assert.AreEqual(2, _target.ColCount);

			_target.OnSave();

            mocks.VerifyAll();
        }

        [Test]
        public void VerifyUpdateRtaStateGroup()
        {
            Guid rtaStateGroupId = Guid.NewGuid();
            IRtaStateGroup rtaStateGroup = mocks.StrictMock<IRtaStateGroup>();
            IEventMessage eventMessage = mocks.StrictMock<IEventMessage>();
            Expect.Call(eventMessage.DomainUpdateType).Return(DomainUpdateType.Update).Repeat.AtLeastOnce();
            Expect.Call(eventMessage.DomainObjectId).Return(rtaStateGroupId).Repeat.AtLeastOnce();
            Expect.Call(rtaStateGroup.Id).Return(rtaStateGroupId).Repeat.AtLeastOnce();

            IList<IRtaStateGroup> rtaStateGroups = new List<IRtaStateGroup>();
			rtaStateGroups.Add(rtaStateGroup);
			PrepareMockForLoad(rtaStateGroups, new List<IAlarmType>(), new List<IActivity>(), new List<IStateGroupActivityAlarm>());
            Expect.Call(_rtaStateGroupRepository.Get(rtaStateGroupId)).Return(rtaStateGroup);
            _manageAlarmSituationView.RefreshGrid();

            mocks.ReplayAll();

            _target = new ManageAlarmSituationPresenter(_alarmTypeRepository, _rtaStateGroupRepository,
                                                        _activityRepository, _stateGroupActvityAlarmRepository,
                                                        _messageBroker, _manageAlarmSituationView);
            _target.Load();
			
            Assert.AreEqual(2, _target.ColCount);
            _target.OnRtaStateGroupEvent(null, new EventMessageArgs(eventMessage));
            Assert.AreEqual(2, _target.ColCount);

            mocks.VerifyAll();
        }

        [Test]
        public void VerifyDeleteRtaStateGroup()
        {
            Guid rtaStateGroupId = Guid.NewGuid();
            IRtaStateGroup rtaStateGroup = mocks.StrictMock<IRtaStateGroup>();
            IEventMessage eventMessage = mocks.StrictMock<IEventMessage>();
            Expect.Call(eventMessage.DomainUpdateType).Return(DomainUpdateType.Delete).Repeat.AtLeastOnce();
            Expect.Call(eventMessage.DomainObjectId).Return(rtaStateGroupId).Repeat.AtLeastOnce();
            Expect.Call(rtaStateGroup.Id).Return(rtaStateGroupId).Repeat.AtLeastOnce();

            IList<IRtaStateGroup> rtaStateGroups = new List<IRtaStateGroup> { rtaStateGroup };
            PrepareMockForLoad(rtaStateGroups, new List<IAlarmType>(), new List<IActivity>(), new List<IStateGroupActivityAlarm>());
            _manageAlarmSituationView.RefreshGrid();

            mocks.ReplayAll();

            _target = new ManageAlarmSituationPresenter(_alarmTypeRepository, _rtaStateGroupRepository,
                                                        _activityRepository, _stateGroupActvityAlarmRepository,
                                                        _messageBroker, _manageAlarmSituationView);
            _target.Load();

            Assert.AreEqual(2, _target.ColCount);
            _target.OnRtaStateGroupEvent(null, new EventMessageArgs(eventMessage));
            Assert.AreEqual(1, _target.ColCount);

            mocks.VerifyAll();
        }

        [Test]
        public void VerifyAddActivity()
        {
            Guid activityId = Guid.NewGuid();
            IActivity activity = mocks.StrictMock<IActivity>();
            IEventMessage eventMessage = mocks.StrictMock<IEventMessage>();
            Expect.Call(eventMessage.DomainUpdateType).Return(DomainUpdateType.Insert).Repeat.AtLeastOnce();
            Expect.Call(eventMessage.DomainObjectId).Return(activityId);

            IList<IActivity> activities = new List<IActivity>();
            PrepareMockForLoad(new List<IRtaStateGroup>(), new List<IAlarmType>(), activities, new List<IStateGroupActivityAlarm>());
            Expect.Call(_activityRepository.Get(activityId)).Return(activity);
            _manageAlarmSituationView.RefreshGrid();

            mocks.ReplayAll();

            _target = new ManageAlarmSituationPresenter(_alarmTypeRepository, _rtaStateGroupRepository,
                                                        _activityRepository, _stateGroupActvityAlarmRepository,
                                                        _messageBroker, _manageAlarmSituationView);
            _target.Load();

            Assert.AreEqual(1,_target.RowCount);
            _target.OnActivityEvent(null,new EventMessageArgs(eventMessage));
            Assert.AreEqual(2, _target.RowCount);

            mocks.VerifyAll();
        }

        [Test]
        public void VerifyUpdateActivity()
        {
            Guid activityId = Guid.NewGuid();
            IActivity activity = mocks.StrictMock<IActivity>();
            IEventMessage eventMessage = mocks.StrictMock<IEventMessage>();
            Expect.Call(eventMessage.DomainUpdateType).Return(DomainUpdateType.Update).Repeat.AtLeastOnce();
            Expect.Call(eventMessage.DomainObjectId).Return(activityId).Repeat.AtLeastOnce();
            Expect.Call(activity.Id).Return(activityId).Repeat.AtLeastOnce();

			IList<IActivity> activities = new List<IActivity> { activity };
            PrepareMockForLoad(new List<IRtaStateGroup>(), new List<IAlarmType>(), activities, new List<IStateGroupActivityAlarm>());
            Expect.Call(_activityRepository.Get(activityId)).Return(activity);
            _manageAlarmSituationView.RefreshGrid();

            mocks.ReplayAll();

            _target = new ManageAlarmSituationPresenter(_alarmTypeRepository, _rtaStateGroupRepository,
                                                        _activityRepository, _stateGroupActvityAlarmRepository,
                                                        _messageBroker, _manageAlarmSituationView);
            _target.Load();

			Assert.AreEqual(2, _target.RowCount);
            _target.OnActivityEvent(null, new EventMessageArgs(eventMessage));
			Assert.AreEqual(2, _target.RowCount);

            mocks.VerifyAll();
        }

        [Test]
        public void VerifyDeleteActivity()
        {
            Guid activityId = Guid.NewGuid();
            IActivity activity = mocks.StrictMock<IActivity>();
            IEventMessage eventMessage = mocks.StrictMock<IEventMessage>();
            Expect.Call(eventMessage.DomainUpdateType).Return(DomainUpdateType.Delete).Repeat.AtLeastOnce();
            Expect.Call(eventMessage.DomainObjectId).Return(activityId).Repeat.AtLeastOnce();
            Expect.Call(activity.Id).Return(activityId).Repeat.AtLeastOnce();

            IList<IActivity> activities = new List<IActivity>{activity};
            PrepareMockForLoad(new List<IRtaStateGroup>(), new List<IAlarmType>(), activities, new List<IStateGroupActivityAlarm>());
            _manageAlarmSituationView.RefreshGrid();

            mocks.ReplayAll();

            _target = new ManageAlarmSituationPresenter(_alarmTypeRepository, _rtaStateGroupRepository,
                                                        _activityRepository, _stateGroupActvityAlarmRepository,
                                                        _messageBroker, _manageAlarmSituationView);
            _target.Load();

            Assert.AreEqual(2, _target.RowCount);
            _target.OnActivityEvent(null, new EventMessageArgs(eventMessage));
            Assert.AreEqual(1, _target.RowCount);

            mocks.VerifyAll();
        }

        private void PrepareMockForLoad(IList<IRtaStateGroup> rtaStateGroups, IList<IAlarmType> alarmTypes, IList<IActivity> activities, IList<IStateGroupActivityAlarm> stateGroupActivityAlarms)
        {
#pragma warning disable 618
            Expect.Call(_rtaStateGroupRepository.LoadAllCompleteGraph()).Return(rtaStateGroups);
            Expect.Call(_alarmTypeRepository.LoadAll()).Return(alarmTypes);
            Expect.Call(_activityRepository.LoadAll()).Return(activities);
            Expect.Call(_stateGroupActvityAlarmRepository.LoadAllCompleteGraph()).Return(stateGroupActivityAlarms);
#pragma warning restore 618
            _messageBroker.RegisterEventSubscription(null, Guid.Empty, null, typeof(IActivity));
            LastCall.IgnoreArguments().Repeat.Times(3);
        }

        [Test]
        public void QueryRowCountShouldReturnOne()
        {
            PrepareMockForLoad(new List<IRtaStateGroup>(), new List<IAlarmType>(), new List<IActivity>(), new List<IStateGroupActivityAlarm>());

            mocks.ReplayAll();

            _target = new ManageAlarmSituationPresenter(_alarmTypeRepository, _rtaStateGroupRepository, _activityRepository, _stateGroupActvityAlarmRepository, _messageBroker, _manageAlarmSituationView);
            _target.Load();

            var e = new GridRowColCountEventArgs();
            _target.QueryRowCount(null, e);
            mocks.VerifyAll();
            Assert.AreEqual(1,e.Count );//with no activities added the constructor adds the default no activity present null object
            Assert.AreEqual(1,_target.RowCount);
        }

        [Test]
        public void QueryColumnCountShouldReturnOne()
        {
            PrepareMockForLoad(new List<IRtaStateGroup>(), new List<IAlarmType>(), new List<IActivity>(), new List<IStateGroupActivityAlarm>());
            mocks.ReplayAll();

            _target = new ManageAlarmSituationPresenter(_alarmTypeRepository, _rtaStateGroupRepository, _activityRepository, _stateGroupActvityAlarmRepository, _messageBroker, _manageAlarmSituationView);
            _target.Load();

            var e = new GridRowColCountEventArgs();
            _target.QueryColCount(null, e);
            mocks.VerifyAll();
            Assert.AreEqual(1, e.Count);//with no state groups added the constructor adds the default no state group present null object

        }


        [Test]
        public void QueryCountReturnCount()
        {
            var activities = new List<IActivity>();
            var rtagroups = new List<IRtaStateGroup>();

			activities.Add(new Activity("Test"));
			rtagroups.Add(new RtaStateGroup("in",false,true));
			rtagroups.Add(new RtaStateGroup("out",false,false));
            using (mocks.Record())
            {
                PrepareMockForLoad(rtagroups, new List<IAlarmType>(), activities, new List<IStateGroupActivityAlarm>());
            }

            using (mocks.Playback())
            {
                _target = new ManageAlarmSituationPresenter(_alarmTypeRepository, _rtaStateGroupRepository,
                                                            _activityRepository, _stateGroupActvityAlarmRepository, _messageBroker, _manageAlarmSituationView);
                _target.Load();
                var e = new GridRowColCountEventArgs();

                _target.QueryColCount(null, e);
                var e2 = new GridRowColCountEventArgs();
                _target.QueryRowCount(null, e2);
                
                Assert.AreEqual(3, e.Count);
                Assert.AreEqual(2, e2.Count);
                Assert.AreEqual(2, _target.RowCount);
                Assert.AreEqual(3, _target.ColCount);

                mocks.VerifyAll();
            }
        }

        [Test]
        public void QueryCellInfoWithBadIndex()
        {
            PrepareMockForLoad(new List<IRtaStateGroup>(), new List<IAlarmType>(), new List<IActivity>(), new List<IStateGroupActivityAlarm>());
            mocks.ReplayAll();

            _target = new ManageAlarmSituationPresenter(_alarmTypeRepository, _rtaStateGroupRepository, _activityRepository, _stateGroupActvityAlarmRepository, _messageBroker, _manageAlarmSituationView);
            _target.Load();
            var style = new GridStyleInfo();
            var e = new GridQueryCellInfoEventArgs(-1, -1, style);
            _target.QueryCellInfo(null, e);
        }

        [Test]
        public void QueryCellInfoShouldReturnGrid()
        {
            var activity = ActivityFactory.CreateActivity("activity");

            var rtaitem1 = new RtaStateGroup("stategroup1", true, false);
            var rtaitem2 = new RtaStateGroup("stategroup2", true, false);

            var alarms = new List<IAlarmType>();
            alarms.Add(new AlarmType(new Description("userALARM"), Color.Blue, new TimeSpan(0, 0, 1),
                                     AlarmTypeMode.UserDefined, 0.8));
            alarms.Add(new AlarmType(new Description("ok"), Color.Blue, new TimeSpan(0, 0, 1), AlarmTypeMode.Ok, 0.8));
            alarms.Add(new AlarmType(new Description("unknown"), Color.Blue, new TimeSpan(0, 0, 1),
                                     AlarmTypeMode.Unknown, 0.8));

            var stategroupactivityalarm1 = new StateGroupActivityAlarm(rtaitem1, activity);
            stategroupactivityalarm1.AlarmType = alarms[1];

            var stategroupactivityalarm2 = new StateGroupActivityAlarm(null, null);
            stategroupactivityalarm2.AlarmType = alarms[1];

            PrepareMockForLoad(new List<IRtaStateGroup> {rtaitem1, rtaitem2}, alarms, new List<IActivity> {activity},
                               new List<IStateGroupActivityAlarm> {stategroupactivityalarm1,stategroupactivityalarm2});

            var list = new StringCollection();
            foreach (IAlarmType type in alarms)
            {
                list.Add(type.Description.Name);
            }

            mocks.ReplayAll();

            _target = new ManageAlarmSituationPresenter(_alarmTypeRepository, _rtaStateGroupRepository,
                                                        _activityRepository, _stateGroupActvityAlarmRepository, _messageBroker, _manageAlarmSituationView);
            _target.Load();
            var style = new GridStyleInfo();
            var e = new GridQueryCellInfoEventArgs(0, 0, style);
            _target.QueryCellInfo(null, e);

            style = new GridStyleInfo();
            e = new GridQueryCellInfoEventArgs(0, 1, style);
            _target.QueryCellInfo(null, e);
            Assert.AreEqual("stategroup1", style.Text);

            style = new GridStyleInfo();
            e = new GridQueryCellInfoEventArgs(0, 2, style);
            _target.QueryCellInfo(null, e);
            Assert.AreEqual("stategroup2", style.Text);

            style = new GridStyleInfo();
            e = new GridQueryCellInfoEventArgs(0, 3, style);
            _target.QueryCellInfo(null, e);
            Assert.AreEqual(UserTexts.Resources.NoStateGroupPresent, style.Text);

            style = new GridStyleInfo();
            e = new GridQueryCellInfoEventArgs(1, 0, style);
            _target.QueryCellInfo(null, e);
            Assert.AreEqual("activity", style.Text);

            style = new GridStyleInfo();
            e = new GridQueryCellInfoEventArgs(1, 1, style);
            _target.QueryCellInfo(null, e);
            Assert.AreEqual("", style.ChoiceList[0]);
            Assert.AreEqual("ok", style.Text);

            style = new GridStyleInfo();
            e = new GridQueryCellInfoEventArgs(1, 2, style);
            _target.QueryCellInfo(null, e);
            Assert.AreEqual("", style.Text);

            style = new GridStyleInfo();
            e = new GridQueryCellInfoEventArgs(2, 0, style);
            _target.QueryCellInfo(null, e);
            Assert.AreEqual(UserTexts.Resources.NoScheduledActivity , style.Text);
        }


        [Test]
        public void SaveCellInfoInsert()
        {
            var alarms = new List<IAlarmType>();
            alarms.Add(new AlarmType(new Description("userALARM"), Color.Blue, new TimeSpan(0, 0, 1),
                                     AlarmTypeMode.UserDefined, 0.8));
            alarms.Add(new AlarmType(new Description("ok"), Color.Blue, new TimeSpan(0, 0, 1), AlarmTypeMode.Ok, 0.8));
            alarms.Add(new AlarmType(new Description("unknown"), Color.Blue, new TimeSpan(0, 0, 1),
                                     AlarmTypeMode.Unknown, 0.8));

            using (mocks.Record())
            {
                _stateGroupActvityAlarmRepository.Add(null);
                LastCall.IgnoreArguments();

				var activity = mocks.StrictMock<IActivity>();
				Expect.Call(activity.Name).Return("activity").Repeat.AtLeastOnce();
				
				var activities = new List<IActivity>();
				activities.Add(activity);

				var rtaitem = mocks.StrictMock<IRtaStateGroup>();
				Expect.Call(rtaitem.Name).Return("stategroup").Repeat.AtLeastOnce();
				
				var rtagroups = new List<IRtaStateGroup>();
                rtagroups.Add(rtaitem);
                
				PrepareMockForLoad(rtagroups, alarms, activities, new List<IStateGroupActivityAlarm>());
            }

            mocks.ReplayAll();

            _target = new ManageAlarmSituationPresenter(_alarmTypeRepository, _rtaStateGroupRepository,
                                                        _activityRepository, _stateGroupActvityAlarmRepository,
                                                        _messageBroker, _manageAlarmSituationView);
            _target.Load();
            var style = new GridStyleInfo();
            style.Text = alarms[1].Description.Name;

            var e = new GridSaveCellInfoEventArgs(1, 1, style, StyleModifyType.Changes);

            _target.SaveCellInfo(null, e);
        }

        [Test]
        public void SaveCellInfoRemove()
        {
            var alarms = new List<IAlarmType>();
            alarms.Add(new AlarmType(new Description("userALARM"), Color.Blue, new TimeSpan(0, 0, 1),
                                     AlarmTypeMode.UserDefined, 0.8));
            alarms.Add(new AlarmType(new Description("ok"), Color.Blue, new TimeSpan(0, 0, 1), AlarmTypeMode.Ok, 0.8));
            alarms.Add(new AlarmType(new Description("unknown"), Color.Blue, new TimeSpan(0, 0, 1),
                                     AlarmTypeMode.Unknown, 0.8));

			var activity = ActivityFactory.CreateActivity("activity");
			var activities = new List<IActivity>();
			activities.Add(activity);

			var rtaitem = new RtaStateGroup("stategroup", true, false);
			var rtagroups = new List<IRtaStateGroup>();
	        rtagroups.Add(rtaitem);

            using (mocks.Record())
            {
                var stategroupactivityalarm = new StateGroupActivityAlarm(rtaitem, activity);
                stategroupactivityalarm.AlarmType = alarms[1];

                _stateGroupActvityAlarmRepository.Remove(null);
                LastCall.IgnoreArguments();

                PrepareMockForLoad(rtagroups, alarms, activities, new List<IStateGroupActivityAlarm> { stategroupactivityalarm });
            }
            mocks.ReplayAll();

            _target = new ManageAlarmSituationPresenter(_alarmTypeRepository, _rtaStateGroupRepository,
                                                        _activityRepository, _stateGroupActvityAlarmRepository, _messageBroker, _manageAlarmSituationView);
            _target.Load();
            var style = new GridStyleInfo();
            style.Text = "";

            var e = new GridSaveCellInfoEventArgs(1, 1, style, StyleModifyType.Changes);

            _target.SaveCellInfo(null, e);

            style = new GridStyleInfo();
             e = new GridSaveCellInfoEventArgs(0, 1, style, StyleModifyType.Changes);
            _target.SaveCellInfo(null, e);

            style = new GridStyleInfo();
            e = new GridSaveCellInfoEventArgs(1, 0, style, StyleModifyType.Changes);
            _target.SaveCellInfo(null, e);
        }

        [Test]
        public void SaveCellInfoUpdate()
        {
#pragma warning disable 618

            var alarms = new List<IAlarmType>();
            alarms.Add(new AlarmType(new Description("userALARM"), Color.Blue, new TimeSpan(0, 0, 1), AlarmTypeMode.UserDefined, 0.8));
            alarms.Add(new AlarmType(new Description("ok"), Color.Blue, new TimeSpan(0, 0, 1), AlarmTypeMode.Ok, 0.8));
            alarms.Add(new AlarmType(new Description("unknown"), Color.Blue, new TimeSpan(0, 0, 1), AlarmTypeMode.Unknown, 0.8));

            var activities = new List<IActivity>();
            var activity = ActivityFactory.CreateActivity("activity");
			activities.Add(activity);

            var rtagroups = new List<IRtaStateGroup>();
            var rtaitem = new RtaStateGroup("stategroup", true, false);
            rtagroups.Add(rtaitem);

            var stategroupactivityalarm = new StateGroupActivityAlarm(rtaitem, activity);
            stategroupactivityalarm.AlarmType = alarms[1];

            PrepareMockForLoad(rtagroups, alarms, activities, new List<IStateGroupActivityAlarm> { stategroupactivityalarm });
            mocks.ReplayAll();

            _target = new ManageAlarmSituationPresenter(_alarmTypeRepository, _rtaStateGroupRepository, _activityRepository, _stateGroupActvityAlarmRepository, _messageBroker, _manageAlarmSituationView);
            _target.Load();
            var style = new GridStyleInfo();
            style.Text = "userALARM";

            var e = new GridSaveCellInfoEventArgs(1, 1, style, StyleModifyType.Changes);

            _target.SaveCellInfo(null, e);
            Assert.AreEqual("userALARM", stategroupactivityalarm.AlarmType.Description.Name);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void SaveCellInfoUpdateWithNullRtaStateGroup()
        {
#pragma warning disable 618

            var alarms = new List<IAlarmType>();
            alarms.Add(new AlarmType(new Description("userALARM"), Color.Blue, new TimeSpan(0, 0, 1), AlarmTypeMode.UserDefined, 0.8));
            alarms.Add(new AlarmType(new Description("ok"), Color.Blue, new TimeSpan(0, 0, 1), AlarmTypeMode.Ok, 0.8));
            alarms.Add(new AlarmType(new Description("unknown"), Color.Blue, new TimeSpan(0, 0, 1), AlarmTypeMode.Unknown, 0.8));

            var activities = new List<IActivity>();
            var activity = ActivityFactory.CreateActivity("activity");
			activities.Add(activity);
            
            var rtagroups = new List<IRtaStateGroup>();
            
            var stategroupactivityalarm = new StateGroupActivityAlarm(null, activity);
            stategroupactivityalarm.AlarmType = alarms[1];

            PrepareMockForLoad(rtagroups, alarms, activities, new List<IStateGroupActivityAlarm> { stategroupactivityalarm });
            mocks.ReplayAll();

            _target = new ManageAlarmSituationPresenter(_alarmTypeRepository, _rtaStateGroupRepository, _activityRepository, _stateGroupActvityAlarmRepository, _messageBroker, _manageAlarmSituationView);
            _target.Load();
            var style = new GridStyleInfo();
            style.Text = "userALARM";

            var e = new GridSaveCellInfoEventArgs(1, 1, style, StyleModifyType.Changes);

            _target.SaveCellInfo(null, e);
            Assert.AreEqual("userALARM", stategroupactivityalarm.AlarmType.Description.Name);
        }

        [Test]
        public void SaveCellInfoUpdateWithNullActivity()
        {
#pragma warning disable 618

            var alarms = new List<IAlarmType>();
            alarms.Add(new AlarmType(new Description("userALARM"), Color.Blue, new TimeSpan(0, 0, 1), AlarmTypeMode.UserDefined, 0.8));
            alarms.Add(new AlarmType(new Description("ok"), Color.Blue, new TimeSpan(0, 0, 1), AlarmTypeMode.Ok, 0.8));
            alarms.Add(new AlarmType(new Description("unknown"), Color.Blue, new TimeSpan(0, 0, 1), AlarmTypeMode.Unknown, 0.8));

            var activities = new List<IActivity>();
        
			var rtagroups = new List<IRtaStateGroup>();
            var rtaitem = new RtaStateGroup("stategroup", true, false);
            rtagroups.Add(rtaitem);
            
            var stategroupactivityalarm = new StateGroupActivityAlarm(rtaitem, null);
            stategroupactivityalarm.AlarmType = alarms[1];

            PrepareMockForLoad(rtagroups, alarms, activities, new List<IStateGroupActivityAlarm> { stategroupactivityalarm });
            mocks.ReplayAll();

            _target = new ManageAlarmSituationPresenter(_alarmTypeRepository, _rtaStateGroupRepository, _activityRepository, _stateGroupActvityAlarmRepository, _messageBroker, _manageAlarmSituationView);
            _target.Load();
            var style = new GridStyleInfo();
            style.Text = "userALARM";

            var e = new GridSaveCellInfoEventArgs(1, 1, style, StyleModifyType.Changes);

            _target.SaveCellInfo(null, e);
            Assert.AreEqual("userALARM", stategroupactivityalarm.AlarmType.Description.Name);
        }

        [TearDown]
        public void Teardown()
        {
            _target = new ManageAlarmSituationPresenter(_alarmTypeRepository, _rtaStateGroupRepository,
                                                        _activityRepository, _stateGroupActvityAlarmRepository, null,
                                                        _manageAlarmSituationView);
            _target.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Virtual dispose method
        /// </summary>
        /// <param name="disposing">
        /// If set to <c>true</c>, explicitly called.
        /// If set to <c>false</c>, implicitly called from finalizer.
        /// </param>
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                ReleaseManagedResources();

            }
            ReleaseUnmanagedResources();
        }

        /// <summary>
        /// Releases the unmanaged resources.
        /// </summary>
        protected virtual void ReleaseUnmanagedResources()
        {
        }

        /// <summary>
        /// Releases the managed resources.
        /// </summary>
        protected virtual void ReleaseManagedResources()
        {
            _target.Dispose();
        }
    }
}
