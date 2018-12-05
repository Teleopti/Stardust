using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Syncfusion.Styles;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.MessageBroker.Legacy;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Configuration;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Intraday;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Wfm.Adherence.Configuration;

namespace Teleopti.Ccc.WinCodeTest.Configuration
{
	[TestFixture]
	public class ManageAlarmSituationPresenterTest : IDisposable
	{
		private ManageAlarmSituationPresenter _target;
		private IActivityRepository _activityRepository;
		private IRtaRuleRepository _rtaRuleRepository;
		private IRtaStateGroupRepository _rtaStateGroupRepository;
		private IRtaMapRepository _stateGroupActvityAlarmRepository;
		private IMessageBrokerComposite _messageBroker;
		private IManageAlarmSituationView _manageAlarmSituationView;
		private IUnitOfWorkFactory _unitOfWorkFactory;

		[SetUp]
		public void CreateManageSituationPresenter()
		{
			_messageBroker = MockRepository.GenerateMock<IMessageBrokerComposite>();
			_activityRepository = MockRepository.GenerateMock<IActivityRepository>();
			_rtaRuleRepository = MockRepository.GenerateMock<IRtaRuleRepository>();
			_rtaStateGroupRepository = MockRepository.GenerateMock<IRtaStateGroupRepository>();
			_stateGroupActvityAlarmRepository = MockRepository.GenerateMock<IRtaMapRepository>();
			_manageAlarmSituationView = MockRepository.GenerateMock<IManageAlarmSituationView>();
			_unitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
		}

		[Test]
		public void VerifyAddRules()
		{
			var ruleId = Guid.NewGuid();
			var rule = MockRepository.GenerateMock<IRtaRule>();
			var eventMessage = MockRepository.GenerateMock<IEventMessage>();
			eventMessage.Stub(x => x.DomainUpdateType).Return(DomainUpdateType.Insert).Repeat.AtLeastOnce();
			eventMessage.Stub(x => x.DomainObjectId).Return(ruleId);

			IList<IRtaRule> rules = new List<IRtaRule>();
			prepareMockForLoad(new List<IRtaStateGroup>(), rules, new List<IActivity>(),
			                   new List<IRtaMap>());
			_rtaRuleRepository.Stub(x => x.Get(ruleId)).Return(rule);

			_target = new ManageAlarmSituationPresenter(_unitOfWorkFactory, _rtaRuleRepository, _rtaStateGroupRepository,
			                                            _activityRepository, _stateGroupActvityAlarmRepository,
			                                            _messageBroker, _manageAlarmSituationView);
			_target.Load();

			_target.OnAlarmEvent(null, new EventMessageArgs(eventMessage));

			_manageAlarmSituationView.AssertWasCalled(x => x.RefreshGrid());
		}

		[Test]
		public void VerifyUpdateRule()
		{
			Guid ruleId = Guid.NewGuid();
			var rule = MockRepository.GenerateMock<IRtaRule>();
			var eventMessage = MockRepository.GenerateMock<IEventMessage>();
			eventMessage.Stub(x => x.DomainUpdateType).Return(DomainUpdateType.Update).Repeat.AtLeastOnce();
			eventMessage.Stub(x => x.DomainObjectId).Return(ruleId).Repeat.AtLeastOnce();
			rule.Stub(x => x.Id).Return(ruleId).Repeat.AtLeastOnce();

			IList<IRtaRule> rules = new List<IRtaRule>();
			rules.Add(rule);
			prepareMockForLoad(new List<IRtaStateGroup>(), rules, new List<IActivity>(),
			                   new List<IRtaMap>());
			_rtaRuleRepository.Stub(x => x.Get(ruleId)).Return(rule);
			_manageAlarmSituationView.RefreshGrid();

			_target = new ManageAlarmSituationPresenter(_unitOfWorkFactory, _rtaRuleRepository, _rtaStateGroupRepository,
			                                            _activityRepository, _stateGroupActvityAlarmRepository,
			                                            _messageBroker, _manageAlarmSituationView);
			_target.Load();

			Assert.AreEqual(1, rules.Count);
			_target.OnAlarmEvent(null, new EventMessageArgs(eventMessage));
			Assert.AreEqual(1, rules.Count);
		}

		[Test]
		public void VerifyDeleteRule()
		{
			Guid ruleId = Guid.NewGuid();
			var rule = MockRepository.GenerateMock<IRtaRule>();
			var eventMessage = MockRepository.GenerateMock<IEventMessage>();
			eventMessage.Stub(x => x.DomainUpdateType).Return(DomainUpdateType.Delete).Repeat.AtLeastOnce();
			eventMessage.Stub(x => x.DomainObjectId).Return(ruleId).Repeat.AtLeastOnce();
			rule.Stub(x => x.Id).Return(ruleId).Repeat.AtLeastOnce();

			IList<IRtaRule> rules = new List<IRtaRule> {rule};
			prepareMockForLoad(new List<IRtaStateGroup>(), rules, new List<IActivity>(),
			                   new List<IRtaMap>());
			

			_target = new ManageAlarmSituationPresenter(_unitOfWorkFactory, _rtaRuleRepository, _rtaStateGroupRepository,
			                                            _activityRepository, _stateGroupActvityAlarmRepository,
			                                            _messageBroker, _manageAlarmSituationView);
			_target.Load();

			
			_target.OnAlarmEvent(null, new EventMessageArgs(eventMessage));
			
			_manageAlarmSituationView.AssertWasCalled(x => x.RefreshGrid());
		}

		[Test]
		public void VerifyAddRtaStateGroup()
		{
			Guid rtaStateGroupId = Guid.NewGuid();
			var rtaStateGroup = MockRepository.GenerateMock<IRtaStateGroup>();
			var eventMessage = MockRepository.GenerateMock<IEventMessage>();
			eventMessage.Stub(x => x.DomainUpdateType).Return(DomainUpdateType.Insert).Repeat.AtLeastOnce();
			eventMessage.Stub(x => x.DomainObjectId).Return(rtaStateGroupId);

			IList<IRtaStateGroup> rtaStateGroups = new List<IRtaStateGroup>();
			prepareMockForLoad(rtaStateGroups, new List<IRtaRule>(), new List<IActivity>(),
			                   new List<IRtaMap>());
			var uow = MockRepository.GenerateStub<IUnitOfWork>();
			_unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(uow);
			_rtaStateGroupRepository.Stub(x => x.Get(rtaStateGroupId)).Return(rtaStateGroup);
			_manageAlarmSituationView.RefreshGrid();

			_target = new ManageAlarmSituationPresenter(_unitOfWorkFactory, _rtaRuleRepository, _rtaStateGroupRepository,
			                                            _activityRepository, _stateGroupActvityAlarmRepository,
			                                            _messageBroker, _manageAlarmSituationView);
			_target.Load();

			Assert.AreEqual(1, _target.ColCount);
			_target.OnRtaStateGroupEvent(null, new EventMessageArgs(eventMessage));
			Assert.AreEqual(2, _target.ColCount);

			_target.OnSave();
		}

		[Test]
		public void VerifyUpdateRtaStateGroup()
		{
			Guid rtaStateGroupId = Guid.NewGuid();
			var rtaStateGroup = MockRepository.GenerateMock<IRtaStateGroup>();
			var eventMessage = MockRepository.GenerateMock<IEventMessage>();
			eventMessage.Stub(x => x.DomainUpdateType).Return(DomainUpdateType.Update).Repeat.AtLeastOnce();
			eventMessage.Stub(x => x.DomainObjectId).Return(rtaStateGroupId).Repeat.AtLeastOnce();
			rtaStateGroup.Stub(x => x.Id).Return(rtaStateGroupId).Repeat.AtLeastOnce();

			IList<IRtaStateGroup> rtaStateGroups = new List<IRtaStateGroup>();
			rtaStateGroups.Add(rtaStateGroup);
			prepareMockForLoad(rtaStateGroups, new List<IRtaRule>(), new List<IActivity>(),
			                   new List<IRtaMap>());
			_rtaStateGroupRepository.Stub(x => x.Get(rtaStateGroupId)).Return(rtaStateGroup);
			_manageAlarmSituationView.RefreshGrid();

			_target = new ManageAlarmSituationPresenter(_unitOfWorkFactory, _rtaRuleRepository, _rtaStateGroupRepository,
			                                            _activityRepository, _stateGroupActvityAlarmRepository,
			                                            _messageBroker, _manageAlarmSituationView);
			_target.Load();

			Assert.AreEqual(2, _target.ColCount);
			_target.OnRtaStateGroupEvent(null, new EventMessageArgs(eventMessage));
			Assert.AreEqual(2, _target.ColCount);
		}

		[Test]
		public void VerifyDeleteRtaStateGroup()
		{
			Guid rtaStateGroupId = Guid.NewGuid();
			var rtaStateGroup = MockRepository.GenerateMock<IRtaStateGroup>();
			var eventMessage = MockRepository.GenerateMock<IEventMessage>();
			eventMessage.Stub(x => x.DomainUpdateType).Return(DomainUpdateType.Delete).Repeat.AtLeastOnce();
			eventMessage.Stub(x => x.DomainObjectId).Return(rtaStateGroupId).Repeat.AtLeastOnce();
			rtaStateGroup.Stub(x => x.Id).Return(rtaStateGroupId).Repeat.AtLeastOnce();

			IList<IRtaStateGroup> rtaStateGroups = new List<IRtaStateGroup> {rtaStateGroup};
			prepareMockForLoad(rtaStateGroups, new List<IRtaRule>(), new List<IActivity>(),
			                   new List<IRtaMap>());
			_manageAlarmSituationView.RefreshGrid();

			_target = new ManageAlarmSituationPresenter(_unitOfWorkFactory, _rtaRuleRepository, _rtaStateGroupRepository,
			                                            _activityRepository, _stateGroupActvityAlarmRepository,
			                                            _messageBroker, _manageAlarmSituationView);
			_target.Load();

			Assert.AreEqual(2, _target.ColCount);
			_target.OnRtaStateGroupEvent(null, new EventMessageArgs(eventMessage));
			Assert.AreEqual(1, _target.ColCount);
		}

		[Test]
		public void VerifyAddActivity()
		{
			Guid activityId = Guid.NewGuid();
			var activity = MockRepository.GenerateMock<IActivity>();
			var eventMessage = MockRepository.GenerateMock<IEventMessage>();
			eventMessage.Stub(x => x.DomainUpdateType).Return(DomainUpdateType.Insert).Repeat.AtLeastOnce();
			eventMessage.Stub(x => x.DomainObjectId).Return(activityId);

			IList<IActivity> activities = new List<IActivity>();
			prepareMockForLoad(new List<IRtaStateGroup>(), new List<IRtaRule>(), activities,
			                   new List<IRtaMap>());
			_activityRepository.Stub(x => x.Get(activityId)).Return(activity);
			_manageAlarmSituationView.RefreshGrid();

			_target = new ManageAlarmSituationPresenter(_unitOfWorkFactory, _rtaRuleRepository, _rtaStateGroupRepository,
			                                            _activityRepository, _stateGroupActvityAlarmRepository,
			                                            _messageBroker, _manageAlarmSituationView);
			_target.Load();

			Assert.AreEqual(1, _target.RowCount);
			_target.OnActivityEvent(null, new EventMessageArgs(eventMessage));
			Assert.AreEqual(2, _target.RowCount);
		}

		[Test]
		public void VerifyUpdateActivity()
		{
			Guid activityId = Guid.NewGuid();
			var activity = MockRepository.GenerateMock<IActivity>();
			var eventMessage = MockRepository.GenerateMock<IEventMessage>();
			eventMessage.Stub(x => x.DomainUpdateType).Return(DomainUpdateType.Update).Repeat.AtLeastOnce();
			eventMessage.Stub(x => x.DomainObjectId).Return(activityId).Repeat.AtLeastOnce();
			activity.Stub(x => x.Id).Return(activityId).Repeat.AtLeastOnce();

			IList<IActivity> activities = new List<IActivity> {activity};
			prepareMockForLoad(new List<IRtaStateGroup>(), new List<IRtaRule>(), activities,
			                   new List<IRtaMap>());
			_activityRepository.Stub(x => x.Get(activityId)).Return(activity);
			_manageAlarmSituationView.RefreshGrid();

			_target = new ManageAlarmSituationPresenter(_unitOfWorkFactory, _rtaRuleRepository, _rtaStateGroupRepository,
			                                            _activityRepository, _stateGroupActvityAlarmRepository,
			                                            _messageBroker, _manageAlarmSituationView);
			_target.Load();

			Assert.AreEqual(2, _target.RowCount);
			_target.OnActivityEvent(null, new EventMessageArgs(eventMessage));
			Assert.AreEqual(2, _target.RowCount);
		}

		[Test]
		public void VerifyDeleteActivity()
		{
			Guid activityId = Guid.NewGuid();
			var activity = MockRepository.GenerateMock<IActivity>();
			var eventMessage = MockRepository.GenerateMock<IEventMessage>();
			eventMessage.Stub(x => x.DomainUpdateType).Return(DomainUpdateType.Delete).Repeat.AtLeastOnce();
			eventMessage.Stub(x => x.DomainObjectId).Return(activityId).Repeat.AtLeastOnce();
			activity.Stub(x => x.Id).Return(activityId).Repeat.AtLeastOnce();

			IList<IActivity> activities = new List<IActivity> {activity};
			prepareMockForLoad(new List<IRtaStateGroup>(), new List<IRtaRule>(), activities,
			                   new List<IRtaMap>());
			_manageAlarmSituationView.RefreshGrid();

			_target = new ManageAlarmSituationPresenter(_unitOfWorkFactory, _rtaRuleRepository, _rtaStateGroupRepository,
			                                            _activityRepository, _stateGroupActvityAlarmRepository,
			                                            _messageBroker, _manageAlarmSituationView);
			_target.Load();

			Assert.AreEqual(2, _target.RowCount);
			_target.OnActivityEvent(null, new EventMessageArgs(eventMessage));
			Assert.AreEqual(1, _target.RowCount);
		}

		private void prepareMockForLoad(IList<IRtaStateGroup> rtaStateGroups, IList<IRtaRule> rules,
		                                IList<IActivity> activities, IList<IRtaMap> rtaMap)
		{
#pragma warning disable 618
			_rtaStateGroupRepository.Stub(x => x.LoadAllCompleteGraph()).Return(rtaStateGroups);
			_rtaRuleRepository.Stub(x => x.LoadAll()).Return(rules);
			_activityRepository.Stub(x => x.LoadAll()).Return(activities);
			_stateGroupActvityAlarmRepository.Stub(x => x.LoadAllCompleteGraph()).Return(rtaMap);
#pragma warning restore 618
		}

		[Test]
		public void QueryRowCountShouldReturnOne()
		{
			prepareMockForLoad(new List<IRtaStateGroup>(), new List<IRtaRule>(), new List<IActivity>(),
			                   new List<IRtaMap>());

			_target = new ManageAlarmSituationPresenter(_unitOfWorkFactory, _rtaRuleRepository, _rtaStateGroupRepository,
			                                            _activityRepository, _stateGroupActvityAlarmRepository, _messageBroker,
			                                            _manageAlarmSituationView);
			_target.Load();

			var e = new GridRowColCountEventArgs();
			_target.QueryRowCount(null, e);

			Assert.AreEqual(1, e.Count);
				//with no activities added the constructor adds the default no activity present null object
			Assert.AreEqual(1, _target.RowCount);
		}

		[Test]
		public void QueryColumnCountShouldReturnOne()
		{
			prepareMockForLoad(new List<IRtaStateGroup>(), new List<IRtaRule>(), new List<IActivity>(),
			                   new List<IRtaMap>());

			_target = new ManageAlarmSituationPresenter(_unitOfWorkFactory, _rtaRuleRepository, _rtaStateGroupRepository,
			                                            _activityRepository, _stateGroupActvityAlarmRepository, _messageBroker,
			                                            _manageAlarmSituationView);
			_target.Load();

			var e = new GridRowColCountEventArgs();
			_target.QueryColCount(null, e);

			Assert.AreEqual(1, e.Count);
				//with no state groups added the constructor adds the default no state group present null object

		}


		[Test]
		public void QueryCountReturnCount()
		{
			var activities = new List<IActivity>();
			var rtagroups = new List<IRtaStateGroup>();

			activities.Add(new Activity("Test"));
			rtagroups.Add(new RtaStateGroup("in", false, true));
			rtagroups.Add(new RtaStateGroup("out", false, false));

			prepareMockForLoad(rtagroups, new List<IRtaRule>(), activities, new List<IRtaMap>());

			_target = new ManageAlarmSituationPresenter(_unitOfWorkFactory, _rtaRuleRepository, _rtaStateGroupRepository,
			                                            _activityRepository, _stateGroupActvityAlarmRepository, _messageBroker,
			                                            _manageAlarmSituationView);
			_target.Load();
			var e = new GridRowColCountEventArgs();

			_target.QueryColCount(null, e);
			var e2 = new GridRowColCountEventArgs();
			_target.QueryRowCount(null, e2);

			Assert.AreEqual(3, e.Count);
			Assert.AreEqual(2, e2.Count);
			Assert.AreEqual(2, _target.RowCount);
			Assert.AreEqual(3, _target.ColCount);
		}

		[Test]
		public void QueryCellInfoWithBadIndex()
		{
			prepareMockForLoad(new List<IRtaStateGroup>(), new List<IRtaRule>(), new List<IActivity>(),
			                   new List<IRtaMap>());

			_target = new ManageAlarmSituationPresenter(_unitOfWorkFactory, _rtaRuleRepository, _rtaStateGroupRepository,
			                                            _activityRepository, _stateGroupActvityAlarmRepository, _messageBroker,
			                                            _manageAlarmSituationView);
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

			var alarms = new List<IRtaRule>
				{
					new RtaRule(new Description("userALARM"), Color.Blue, 1, 0.8),
					new RtaRule(new Description("ok"), Color.Blue, 1, 0.8),
					new RtaRule(new Description("unknown"), Color.Blue, 1, 0.8)
				};

			var rtaMap1 = new RtaMap(rtaitem1, activity) {RtaRule = alarms[1]};

			var rtaMap2 = new RtaMap(null, null) {RtaRule = alarms[1]};

			prepareMockForLoad(new List<IRtaStateGroup> {rtaitem1, rtaitem2}, alarms, new List<IActivity> {activity},
			                   new List<IRtaMap> {rtaMap1, rtaMap2});

			var list = new StringCollection();
			foreach (IRtaRule type in alarms)
			{
				list.Add(type.Description.Name);
			}

			_target = new ManageAlarmSituationPresenter(_unitOfWorkFactory, _rtaRuleRepository, _rtaStateGroupRepository,
			                                            _activityRepository, _stateGroupActvityAlarmRepository, _messageBroker,
			                                            _manageAlarmSituationView);
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
			Assert.AreEqual(UserTexts.Resources.NoScheduledActivity, style.Text);
		}


		[Test]
		public void SaveCellInfoInsert()
		{
			var alarms = new List<IRtaRule>
				{
					new RtaRule(new Description("userALARM"), Color.Blue, 1, 0.8),
					new RtaRule(new Description("ok"), Color.Blue, 1, 0.8),
					new RtaRule(new Description("unknown"), Color.Blue, 1, 0.8)
				};

			var activity = ActivityFactory.CreateActivity("activity");
			var activities = new List<IActivity> {activity};
			var rtaitem = new RtaStateGroup("stategroup", false, true);
			var rtagroups = new List<IRtaStateGroup> {rtaitem};

			prepareMockForLoad(rtagroups, alarms, activities, new List<IRtaMap>());

			_target = new ManageAlarmSituationPresenter(_unitOfWorkFactory, _rtaRuleRepository, _rtaStateGroupRepository,
			                                            _activityRepository, _stateGroupActvityAlarmRepository,
			                                            _messageBroker, _manageAlarmSituationView);
			_target.Load();
			var style = new GridStyleInfo {Text = alarms[1].Description.Name};

			var e = new GridSaveCellInfoEventArgs(1, 1, style, StyleModifyType.Changes);

			_target.SaveCellInfo(null, e);
		}

		[Test]
		public void SaveCellInfoRemove()
		{
			var alarms = new List<IRtaRule>
				{
					new RtaRule(new Description("userALARM"), Color.Blue, 1, 0.8),
					new RtaRule(new Description("ok"), Color.Blue, 1, 0.8),
					new RtaRule(new Description("unknown"), Color.Blue, 1, 0.8)
				};

			var activity = ActivityFactory.CreateActivity("activity");
			var activities = new List<IActivity> {activity};

			var rtaitem = new RtaStateGroup("stategroup", true, false);
			var rtagroups = new List<IRtaStateGroup> {rtaitem};

			var rtaMap = new RtaMap(rtaitem, activity) {RtaRule = alarms[1]};

			prepareMockForLoad(rtagroups, alarms, activities, new List<IRtaMap> {rtaMap});

			_target = new ManageAlarmSituationPresenter(_unitOfWorkFactory, _rtaRuleRepository, _rtaStateGroupRepository,
			                                            _activityRepository, _stateGroupActvityAlarmRepository, _messageBroker,
			                                            _manageAlarmSituationView);
			_target.Load();
			var style = new GridStyleInfo {Text = ""};

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

			var alarms = new List<IRtaRule>
				{
					new RtaRule(new Description("userALARM"), Color.Blue, 1, 0.8),
					new RtaRule(new Description("ok"), Color.Blue, 1, 0.8),
					new RtaRule(new Description("unknown"), Color.Blue, 1, 0.8)
				};

			var activities = new List<IActivity>();
			var activity = ActivityFactory.CreateActivity("activity");
			activities.Add(activity);

			var rtagroups = new List<IRtaStateGroup>();
			var rtaitem = new RtaStateGroup("stategroup", true, false);
			rtagroups.Add(rtaitem);

			var rtaMap = new RtaMap(rtaitem, activity) {RtaRule = alarms[1]};

			prepareMockForLoad(rtagroups, alarms, activities, new List<IRtaMap> {rtaMap});

			_target = new ManageAlarmSituationPresenter(_unitOfWorkFactory, _rtaRuleRepository, _rtaStateGroupRepository,
			                                            _activityRepository, _stateGroupActvityAlarmRepository, _messageBroker,
			                                            _manageAlarmSituationView);
			_target.Load();
			var style = new GridStyleInfo {Text = "userALARM"};

			var e = new GridSaveCellInfoEventArgs(1, 1, style, StyleModifyType.Changes);

			_target.SaveCellInfo(null, e);
			_target.QueryCellInfo(null, new GridQueryCellInfoEventArgs(1, 1, style));
			style.CellValue.ToString().Should().Be("userALARM");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"),
		 Test]
		public void SaveCellInfoUpdateWithNullRtaStateGroup()
		{
#pragma warning disable 618

			var alarms = new List<IRtaRule>
				{
					new RtaRule(new Description("userALARM"), Color.Blue, 1, 0.8),
					new RtaRule(new Description("ok"), Color.Blue, 1, 0.8),
					new RtaRule(new Description("unknown"), Color.Blue, 1, 0.8)
				};

			var activities = new List<IActivity>();
			var activity = ActivityFactory.CreateActivity("activity");
			activities.Add(activity);

			var rtagroups = new List<IRtaStateGroup>();

			var rtaMap = new RtaMap(null, activity) {RtaRule = alarms[1]};

			prepareMockForLoad(rtagroups, alarms, activities, new List<IRtaMap> {rtaMap});

			_target = new ManageAlarmSituationPresenter(_unitOfWorkFactory, _rtaRuleRepository, _rtaStateGroupRepository,
			                                            _activityRepository, _stateGroupActvityAlarmRepository, _messageBroker,
			                                            _manageAlarmSituationView);
			_target.Load();
			var style = new GridStyleInfo {Text = "userALARM"};

			var e = new GridSaveCellInfoEventArgs(1, 1, style, StyleModifyType.Changes);

			_target.SaveCellInfo(null, e); 
			_target.QueryCellInfo(null, new GridQueryCellInfoEventArgs(1, 1, style));
			style.CellValue.ToString().Should().Be("userALARM");
		}

		[Test]
		public void SaveCellInfoUpdateWithNullActivity()
		{
#pragma warning disable 618

			var alarms = new List<IRtaRule>
				{
					new RtaRule(new Description("userALARM"), Color.Blue, 1, 0.8),
					new RtaRule(new Description("ok"), Color.Blue, 1, 0.8),
					new RtaRule(new Description("unknown"), Color.Blue, 1, 0.8)
				};

			var activities = new List<IActivity>();

			var rtagroups = new List<IRtaStateGroup>();
			var rtaitem = new RtaStateGroup("stategroup", true, false);
			rtagroups.Add(rtaitem);

			var rtaMap = new RtaMap(rtaitem, null) {RtaRule = alarms[1]};

			prepareMockForLoad(rtagroups, alarms, activities, new List<IRtaMap> {rtaMap});

			_target = new ManageAlarmSituationPresenter(_unitOfWorkFactory, _rtaRuleRepository, _rtaStateGroupRepository,
			                                            _activityRepository, _stateGroupActvityAlarmRepository, _messageBroker,
			                                            _manageAlarmSituationView);
			_target.Load();
			var style = new GridStyleInfo {Text = "userALARM"};

			var e = new GridSaveCellInfoEventArgs(1, 1, style, StyleModifyType.Changes);

			_target.SaveCellInfo(null, e);
			_target.QueryCellInfo(null, new GridQueryCellInfoEventArgs(1, 1, style));
			style.CellValue.ToString().Should().Be("userALARM");
		}

		[Test]
		public void ShouldSaveChangedRtaMap()
		{
			var alarms = new List<IRtaRule>
				{
					new RtaRule(new Description("userALARM"), Color.Blue, 1, 0.8),
					new RtaRule(new Description("ok"), Color.Blue, 1, 0.8),
					new RtaRule(new Description("unknown"), Color.Blue, 1, 0.8)
				};

			var unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();
			var activities = new List<IActivity>();
			var rtagroups = new List<IRtaStateGroup>();
			var rtaitem = new RtaStateGroup("stategroup", true, false);
			rtagroups.Add(rtaitem);

			IRtaMap rtaMap = new RtaMap(rtaitem, null) { RtaRule = alarms[1] };
			rtaMap.SetId(Guid.NewGuid());
			prepareMockForLoad(rtagroups, alarms, activities, new List<IRtaMap> { rtaMap });

			_unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(unitOfWork);
			unitOfWork.Stub(x => x.Merge(rtaMap)).IgnoreArguments().Return(rtaMap);

			_target = new ManageAlarmSituationPresenter(_unitOfWorkFactory, _rtaRuleRepository, _rtaStateGroupRepository,
														_activityRepository, _stateGroupActvityAlarmRepository, _messageBroker,
														_manageAlarmSituationView);
			_target.Load();
			var style = new GridStyleInfo { Text = "userALARM" };

			var e = new GridSaveCellInfoEventArgs(1, 1, style, StyleModifyType.Changes);

			_target.SaveCellInfo(null, e);
			_target.OnSave();

			unitOfWork.AssertWasCalled(x => x.Merge(rtaMap),
			                           o =>
			                           o.Constraints(Rhino.Mocks.Constraints.Is.Equal(rtaMap)));
			unitOfWork.AssertWasCalled(x => x.PersistAll());
			_rtaRuleRepository.AssertWasCalled(x => x.LoadAll());
			_activityRepository.AssertWasCalled(x => x.LoadAll());
			_rtaStateGroupRepository.AssertWasCalled(x => x.LoadAll());
		}

		[TearDown]
		public void Teardown()
		{
			_target = new ManageAlarmSituationPresenter(_unitOfWorkFactory, _rtaRuleRepository, _rtaStateGroupRepository,
			                                            _activityRepository, _stateGroupActvityAlarmRepository, null,
			                                            _manageAlarmSituationView);
			_target.Dispose();
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (disposing)
			{
				ReleaseManagedResources();
			}
			ReleaseUnmanagedResources();
		}

		protected virtual void ReleaseUnmanagedResources()
		{
		}

		protected virtual void ReleaseManagedResources()
		{
			_target.Dispose();
		}
	}
}
