using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Backlog;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Outbound;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Persisters.Outbound;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.Mapping;
using Teleopti.Ccc.Web.Areas.Outbound.Models;
using Teleopti.Ccc.Web.Core.Data;


namespace Teleopti.Ccc.WebTest.Areas.Outbound.Core
{
	[TestFixture]
	class OutboundCampaignPersisterIntegrationTest
	{
		private OutboundCampaignPersister _outboundCampaignPersister;
		private FakeCampaignRepository _fakeCampaignRepository;
		private FakeSkillRepository _fakeSkillRepository;
		private FakeActivityRepository _fakeActivityRepository;
		private CampaignForm _campaignInput;	
		private ICreateOrUpdateSkillDays _createOrUpdateSkillDays;
		private FakeOutboundScheduledResourcesCacher _outboundScheduledResourcesCacher;

		[SetUp]
		public void Init()
		{
			_campaignInput =  new CampaignForm()
			{
				Name = "myCampaign",
				CallListLen = 1000,
				TargetRate = 50,
				ConnectRate = 50,
				RightPartyConnectRate = 30,
				ConnectAverageHandlingTime = 60,
				RightPartyAverageHandlingTime = 100,
				UnproductiveTime = 150,
				Activity = new ActivityViewModel(){Id = null, Name = "myActivity"},
				StartDate = new DateOnly(2015, 6, 23),
				EndDate = new DateOnly(2100, 1, 1),
				WorkingHours = new List<CampaignWorkingHour>()
				{
					new CampaignWorkingHour(){WeekDay = DayOfWeek.Monday, StartTime = new TimeSpan(9,0,0), EndTime = new TimeSpan(17,0,0)},
					new CampaignWorkingHour(){WeekDay = DayOfWeek.Tuesday, StartTime =new TimeSpan(9,0,0), EndTime = new TimeSpan(17,0,0)},
					new CampaignWorkingHour(){WeekDay = DayOfWeek.Wednesday, StartTime = new TimeSpan(9,0,0), EndTime = new TimeSpan(17,0,0)},
					new CampaignWorkingHour(){WeekDay = DayOfWeek.Thursday, StartTime = new TimeSpan(9,0,0), EndTime = new TimeSpan(17,0,0)},
					new CampaignWorkingHour(){WeekDay = DayOfWeek.Friday, StartTime = new TimeSpan(9,0,0), EndTime = new TimeSpan(17,0,0)},
					new CampaignWorkingHour(){WeekDay = DayOfWeek.Saturday, StartTime = new TimeSpan(10,0,0), EndTime = new TimeSpan(15,0,0)},
					new CampaignWorkingHour(){WeekDay = DayOfWeek.Sunday, StartTime = new TimeSpan(10,0,0), EndTime = new TimeSpan(15,0,0)}
				}
			};

			var skillCreator = MockRepository.GenerateMock<IOutboundSkillCreator>();
			skillCreator.Stub(x => x.CreateSkill(null, null)).IgnoreArguments().Return(SkillFactory.CreateSkillWithWorkloadAndSources());
			_createOrUpdateSkillDays = MockRepository.GenerateMock<ICreateOrUpdateSkillDays>();
			_createOrUpdateSkillDays.Stub(x => x.Create(null, new DateOnlyPeriod(), 0, new TimeSpan(), null)).IgnoreArguments();

			_fakeCampaignRepository = new FakeCampaignRepository();
			_fakeSkillRepository = new FakeSkillRepository();
			_fakeActivityRepository = new FakeActivityRepository();
			_fakeActivityRepository.Add(ActivityFactory.CreateActivity("Phone"));
			_outboundScheduledResourcesCacher = new FakeOutboundScheduledResourcesCacher();
			
			_outboundCampaignPersister = new OutboundCampaignPersister(_fakeCampaignRepository,
				new OutboundCampaignMapper(_fakeCampaignRepository), new OutboundCampaignViewModelMapper(),
				skillCreator, _fakeActivityRepository,
				new OutboundSkillPersister(_fakeSkillRepository, new FakeWorkloadRepository()), _createOrUpdateSkillDays, null, null, null, null, _outboundScheduledResourcesCacher);
		}

		[Test]
		public void ShouldPersistSkillWhenPersistingCampaign()
		{
			_outboundCampaignPersister.Persist(_campaignInput);

			var campaigns = _fakeCampaignRepository.LoadAll().ToList();

			campaigns.Count.Should().Be.EqualTo(1);
			campaigns[0].Skill.Should().Be.SameInstanceAs(_fakeSkillRepository.LoadAll().First());
		}		
		
		[Test]
		public void ShouldSetCampaignName()
		{
			_outboundCampaignPersister.Persist(_campaignInput);

			var campaigns = _fakeCampaignRepository.LoadAll().ToList();
			campaigns[0].Name.Should().Be.EqualTo(_campaignInput.Name);
		}

		[Test]
		public void ShouldSetCallListLen()
		{
			_outboundCampaignPersister.Persist(_campaignInput);

			var campaigns = _fakeCampaignRepository.LoadAll().ToList();
			campaigns[0].CallListLen.Should().Be.EqualTo(_campaignInput.CallListLen);
		}

		[Test]
		public void ShouldSetTargetRate()
		{
			_outboundCampaignPersister.Persist(_campaignInput);

			var campaigns = _fakeCampaignRepository.LoadAll().ToList();
			campaigns[0].TargetRate.Should().Be.EqualTo(_campaignInput.TargetRate);
		}

		[Test]
		public void ShouldSetConnectRate()
		{
			_outboundCampaignPersister.Persist(_campaignInput);

			var campaigns = _fakeCampaignRepository.LoadAll().ToList();
			campaigns[0].ConnectRate.Should().Be.EqualTo(_campaignInput.ConnectRate);
		}

		[Test]
		public void ShouldSetRightPartyConnectRate()
		{
			_outboundCampaignPersister.Persist(_campaignInput);

			var campaigns = _fakeCampaignRepository.LoadAll().ToList();
			campaigns[0].RightPartyConnectRate.Should().Be.EqualTo(_campaignInput.RightPartyConnectRate);
		}

		[Test]
		public void ShouldSetConnectAverageHandlingTime()
		{
			_outboundCampaignPersister.Persist(_campaignInput);

			var campaigns = _fakeCampaignRepository.LoadAll().ToList();
			campaigns[0].ConnectAverageHandlingTime.Should().Be.EqualTo(_campaignInput.ConnectAverageHandlingTime);
		}

		[Test]
		public void ShouldSetRightPartyAverageHandlingTime()
		{
			_outboundCampaignPersister.Persist(_campaignInput);

			var campaigns = _fakeCampaignRepository.LoadAll().ToList();
			campaigns[0].RightPartyAverageHandlingTime.Should().Be.EqualTo(_campaignInput.RightPartyAverageHandlingTime);
		}

		[Test]
		public void ShouldSetUnproductiveTime()
		{
			_outboundCampaignPersister.Persist(_campaignInput);

			var campaigns = _fakeCampaignRepository.LoadAll().ToList();
			campaigns[0].UnproductiveTime.Should().Be.EqualTo(_campaignInput.UnproductiveTime);
		}

		[Test]
		public void ShouldSetSpanningPeriod()
		{
			_outboundCampaignPersister.Persist(_campaignInput);

			var campaigns = _fakeCampaignRepository.LoadAll().ToList();

			var startDateTime = new DateTime(_campaignInput.StartDate.Year, _campaignInput.StartDate.Month, _campaignInput.StartDate.Day, 0,0,0, DateTimeKind.Utc);
			var endDateTime = new DateTime(_campaignInput.EndDate.Year, _campaignInput.EndDate.Month, _campaignInput.EndDate.Day, 23,59,59, DateTimeKind.Utc);
			campaigns[0].SpanningPeriod.Should().Be.EqualTo(new DateTimePeriod(startDateTime, endDateTime));
		}		
		
		[Test]
		public void ShouldSetBelongsToPeriod()
		{
			_outboundCampaignPersister.Persist(_campaignInput);

			var campaigns = _fakeCampaignRepository.LoadAll().ToList();

			campaigns[0].BelongsToPeriod.Should().Be.EqualTo(new DateOnlyPeriod(_campaignInput.StartDate, _campaignInput.EndDate));
		}

		[Test]
		public void ShouldSetWorkingHours()
		{
			_outboundCampaignPersister.Persist(_campaignInput);
			
			var campaigns = _fakeCampaignRepository.LoadAll().ToList();
			for (var i = 0; i < 7; ++i)
			{
				campaigns[0].WorkingHours[_campaignInput.WorkingHours[i].WeekDay].Should().Be.EqualTo(new TimePeriod(_campaignInput.WorkingHours[i].StartTime, (_campaignInput.WorkingHours[i].EndTime)));
			}
		}

		[Test]
		public void ShouldCreateActivityWhenUserDidNotChooseAnyExisted()
		{
			_outboundCampaignPersister.Persist(_campaignInput);

			_fakeActivityRepository.LoadAll().Last().Name.Should().Be.EqualTo(_campaignInput.Activity.Name);
		}		
		
		[Test]
		public void ShouldLoadActivityForUserChosen()
		{
			_campaignInput.Activity.Id = _fakeActivityRepository.LoadAll().First().Id;
			_outboundCampaignPersister.Persist(_campaignInput);

			_fakeActivityRepository.LoadAll().First().Id.Should().Be.EqualTo(_campaignInput.Activity.Id);
		}

		[Test]
		public void ShouldTriggerProductionPlanCalculation()
		{
			_outboundCampaignPersister.Persist(_campaignInput);
			var campaign = _fakeCampaignRepository.LoadAll().First();

			_createOrUpdateSkillDays.AssertWasCalled(x => x.Create(campaign.Skill, campaign.SpanningPeriod.ToDateOnlyPeriod(campaign.Skill.TimeZone), campaign.CampaignTasks(), campaign.AverageTaskHandlingTime(), campaign.WorkingHours));
		}
	}

	[TestFixture]
	class OutboundCampaignPersisterTest
	{
		private IOutboundCampaignRepository _outboundCampaignRepository;
		private IOutboundCampaignMapper _outboundCampaignMapper;
		private IOutboundCampaignViewModelMapper _outboundCampaignViewModelMapper;
		private IOutboundSkillCreator _outboundSkillCreator;
		private IActivityRepository _activityRepository;
		private IOutboundSkillPersister _outboundSkillPersister;
		private IProductionReplanHelper _productionReplanHelper;
		private OutboundCampaignPersister _target;
		private IActivity _activity = ActivityFactory.CreateActivity("aa");
		private IOutboundPeriodMover _outboundPeriodMover;
		private ISkillRepository _skillRepository;
		private IOutboundScheduledResourcesCacher _outboundScheduledResourcesCacher;

		[SetUp]
		public void Setup()
		{
			_outboundCampaignRepository = MockRepository.GenerateMock<IOutboundCampaignRepository>();
			_outboundCampaignMapper = MockRepository.GenerateMock<IOutboundCampaignMapper>();
			_outboundCampaignViewModelMapper = MockRepository.GenerateMock<IOutboundCampaignViewModelMapper>();
			_outboundSkillCreator = MockRepository.GenerateMock<IOutboundSkillCreator>();
			_activityRepository = new FakeActivityRepository();
			_activityRepository.Add(_activity);
			_outboundSkillPersister = MockRepository.GenerateMock<IOutboundSkillPersister>();
			_productionReplanHelper = MockRepository.GenerateMock<IProductionReplanHelper>();
			_outboundPeriodMover = MockRepository.GenerateMock<IOutboundPeriodMover>();
			_skillRepository = MockRepository.GenerateMock<ISkillRepository>();
			_outboundScheduledResourcesCacher = MockRepository.GenerateMock<IOutboundScheduledResourcesCacher>();
			_productionReplanHelper.Stub(x => x.Replan(null)).IgnoreArguments();
			_outboundPeriodMover.Stub(x => x.Move(null, new DateOnlyPeriod())).IgnoreArguments();
			_skillRepository.Stub(x => x.Get(new Guid()));
			_target = new OutboundCampaignPersister(_outboundCampaignRepository, _outboundCampaignMapper, null, _outboundSkillCreator,
				_activityRepository, null, null, _productionReplanHelper, _outboundPeriodMover, null, _skillRepository, _outboundScheduledResourcesCacher);

		}

		[Test]
		public void ManualReplanShouldDelegateToReplanHelper()
		{
			
			var campaignId = Guid.NewGuid();
			var campaign = new Domain.Outbound.Campaign();
			_outboundCampaignRepository.Stub(x => x.Get(campaignId)).Return(campaign);
			var planForm = new PlanWithScheduleForm(){CampaignId = campaignId,SkipDates = new List<DateOnly>()};

			_target = new OutboundCampaignPersister(_outboundCampaignRepository, _outboundCampaignMapper, null, _outboundSkillCreator, _activityRepository, null, null, _productionReplanHelper, _outboundPeriodMover, null, null, _outboundScheduledResourcesCacher);
			_target.ManualReplanCampaign(planForm);

			_productionReplanHelper.AssertWasCalled(x => x.Replan(campaign));
		}

		[Test]
		public void ShouldStoreNewCampaign()
		{
			var expectedVM = new CampaignViewModel();
			var createOrUpdateSkillDays = MockRepository.GenerateMock<ICreateOrUpdateSkillDays>();
			createOrUpdateSkillDays.Stub(x => x.Create(null, new DateOnlyPeriod(), 0, new TimeSpan(), null)).IgnoreArguments();
			var skill = SkillFactory.CreateSkill("mySkill");
			skill.TimeZone = TimeZoneInfo.Utc;
			_outboundSkillCreator.Stub(x=>x.CreateSkill(ActivityFactory.CreateActivity("myActivity"), new Domain.Outbound.Campaign())).IgnoreArguments().Return(skill);
			var target = new OutboundCampaignPersister(_outboundCampaignRepository, null, _outboundCampaignViewModelMapper, _outboundSkillCreator, _activityRepository,
				_outboundSkillPersister, createOrUpdateSkillDays, null, null, null, null, _outboundScheduledResourcesCacher);
			_outboundCampaignViewModelMapper.Stub(x => x.Map(new Domain.Outbound.Campaign())).IgnoreArguments().Return(expectedVM);

			var result = target.Persist(new CampaignForm()
			{
				Name = "myCampaign",
				CallListLen = 1000,
				TargetRate = 50,
				ConnectRate = 50,
				RightPartyConnectRate = 30,
				ConnectAverageHandlingTime = 60,
				RightPartyAverageHandlingTime = 100,
				UnproductiveTime = 150,
				Activity = new ActivityViewModel() { Id = null, Name = "myActivity" },
				StartDate = new DateOnly(2015, 6, 23),
				EndDate = new DateOnly(2100, 1, 1),
				WorkingHours = new List<CampaignWorkingHour>()
			});

			_outboundCampaignRepository.AssertWasCalled(x => x.Add(null), o => o.IgnoreArguments());
			result.Should().Be.SameInstanceAs(expectedVM);
		}

		[Test]
		public void ShouldUpdateCampaign()
		{
			var activityId = new Guid();
			var campaignVM = new CampaignViewModel { Id = new Guid(), Activity = new ActivityViewModel(){Id = activityId } };
			var target = new OutboundCampaignPersister(_outboundCampaignRepository, _outboundCampaignMapper, null, null, null, null, null, _productionReplanHelper, _outboundPeriodMover, null, null, _outboundScheduledResourcesCacher);
			var expectedCampaign = new Domain.Outbound.Campaign();
			expectedCampaign.Skill = new Skill();
			var activity = new Activity();
			activity.SetId(activityId);
			expectedCampaign.Skill.Activity = activity;
			_outboundCampaignRepository.Stub(x => x.Load((Guid)campaignVM.Id).Clone()).Return(expectedCampaign);
			_outboundCampaignMapper.Stub(x => x.Map(campaignVM)).Return(expectedCampaign);

			var result = target.Persist(campaignVM);

			result.Should().Be.SameInstanceAs(expectedCampaign);
		}

		[Test]
		public void ShouldReplanWhenCampaignCallListUpdated()
		{
			var campaignId = new Guid();
			var oldCampaign = new Domain.Outbound.Campaign() { CallListLen = 1000 };
			oldCampaign.SetId(campaignId);
			var activityId = new Guid();
			oldCampaign.Skill = new Skill();
			var activity = new Activity();
			activity.SetId(activityId);
			oldCampaign.Skill.Activity = activity;
			var newCampaign = new Domain.Outbound.Campaign() { CallListLen = 2000 };
			newCampaign.SetId(campaignId);
			

			var campaignVM = new CampaignViewModel { Id = oldCampaign.Id, Activity = new ActivityViewModel() { Id = activityId } };
			_outboundCampaignRepository.Stub(x => x.Load((Guid)campaignVM.Id).Clone()).Return(oldCampaign);
			_outboundCampaignMapper.Stub(x => x.Map(campaignVM)).Return(newCampaign);

			_target.Persist(campaignVM);

			_productionReplanHelper.AssertWasCalled(x => x.Replan(newCampaign));
		}

		[Test]
		public void ShouldReplanWhenCampaignTargetRateUpdated()
		{
			var campaignId = new Guid();
			var oldCampaign = new Domain.Outbound.Campaign() { TargetRate = 50 };
			oldCampaign.SetId(campaignId);
			var activityId = new Guid();
			oldCampaign.Skill = new Skill();
			var activity = new Activity();
			activity.SetId(activityId);
			oldCampaign.Skill.Activity = activity;
			var newCampaign = new Domain.Outbound.Campaign() { TargetRate = 60 };
			newCampaign.SetId(campaignId);

			var campaignVM = new CampaignViewModel { Id = oldCampaign.Id, Activity = new ActivityViewModel() { Id = activityId } };
			_outboundCampaignRepository.Stub(x => x.Load((Guid)campaignVM.Id).Clone()).Return(oldCampaign);
			_outboundCampaignMapper.Stub(x => x.Map(campaignVM)).Return(newCampaign);

			_target.Persist(campaignVM);

			_productionReplanHelper.AssertWasCalled(x => x.Replan(newCampaign));
		}

		[Test]
		public void ShouldReplanWhenCampaignConnectRateUpdated()
		{
			var campaignId = new Guid();
			var oldCampaign = new Domain.Outbound.Campaign() { ConnectRate = 100 };
			oldCampaign.SetId(campaignId);
			var activityId = new Guid();
			oldCampaign.Skill = new Skill();
			var activity = new Activity();
			activity.SetId(activityId);
			oldCampaign.Skill.Activity = activity;
			var newCampaign = new Domain.Outbound.Campaign() { ConnectRate = 60 };
			newCampaign.SetId(campaignId);

			var campaignVM = new CampaignViewModel { Id = oldCampaign.Id, Activity = new ActivityViewModel() { Id = activityId } };
			_outboundCampaignRepository.Stub(x => x.Load((Guid)campaignVM.Id).Clone()).Return(oldCampaign);
			_outboundCampaignMapper.Stub(x => x.Map(campaignVM)).Return(newCampaign);

			_target.Persist(campaignVM);

			_productionReplanHelper.AssertWasCalled(x => x.Replan(newCampaign));
		}

		[Test]
		public void ShouldReplanWhenCampaignRightPartyConnectRateUpdated()
		{
			var campaignId = new Guid();
			var oldCampaign = new Domain.Outbound.Campaign() { RightPartyConnectRate = 100 };
			oldCampaign.SetId(campaignId);
			var activityId = new Guid();
			oldCampaign.Skill = new Skill();
			var activity = new Activity();
			activity.SetId(activityId);
			oldCampaign.Skill.Activity = activity;
			var newCampaign = new Domain.Outbound.Campaign() { RightPartyConnectRate = 60 };
			newCampaign.SetId(campaignId);

			var campaignVM = new CampaignViewModel { Id = oldCampaign.Id, Activity = new ActivityViewModel() { Id = activityId } };
			_outboundCampaignRepository.Stub(x => x.Load((Guid)campaignVM.Id).Clone()).Return(oldCampaign);
			_outboundCampaignMapper.Stub(x => x.Map(campaignVM)).Return(newCampaign);

			_target.Persist(campaignVM);

			_productionReplanHelper.AssertWasCalled(x => x.Replan(newCampaign));
		}

		[Test]
		public void ShouldReplanWhenCampaignConnectAverageHandlingTimeUpdated()
		{
			var campaignId = new Guid();
			var oldCampaign = new Domain.Outbound.Campaign() { ConnectAverageHandlingTime = 100 };
			oldCampaign.SetId(campaignId);
			var activityId = new Guid();
			oldCampaign.Skill = new Skill();
			var activity = new Activity();
			activity.SetId(activityId);
			oldCampaign.Skill.Activity = activity;
			var newCampaign = new Domain.Outbound.Campaign() { ConnectAverageHandlingTime = 60 };
			newCampaign.SetId(campaignId);

			var campaignVM = new CampaignViewModel { Id = oldCampaign.Id, Activity = new ActivityViewModel() { Id = activityId } };
			_outboundCampaignRepository.Stub(x => x.Load((Guid)campaignVM.Id).Clone()).Return(oldCampaign);
			_outboundCampaignMapper.Stub(x => x.Map(campaignVM)).Return(newCampaign);

			_target.Persist(campaignVM);

			_productionReplanHelper.AssertWasCalled(x => x.Replan(newCampaign));
		}

		[Test]
		public void ShouldReplanWhenCampaignRightPartyAverageHandlingTimeUpdated()
		{
			var campaignId = new Guid();
			var oldCampaign = new Domain.Outbound.Campaign() { RightPartyAverageHandlingTime = 100 };
			oldCampaign.SetId(campaignId);
			var activityId = new Guid();
			oldCampaign.Skill = new Skill();
			var activity = new Activity();
			activity.SetId(activityId);
			oldCampaign.Skill.Activity = activity;
			var newCampaign = new Domain.Outbound.Campaign() { RightPartyAverageHandlingTime = 60 };
			newCampaign.SetId(campaignId);

			var campaignVM = new CampaignViewModel { Id = oldCampaign.Id, Activity = new ActivityViewModel() { Id = activityId } };
			_outboundCampaignRepository.Stub(x => x.Load((Guid)campaignVM.Id).Clone()).Return(oldCampaign);
			_outboundCampaignMapper.Stub(x => x.Map(campaignVM)).Return(newCampaign);

			_target.Persist(campaignVM);

			_productionReplanHelper.AssertWasCalled(x => x.Replan(newCampaign));
		}

		[Test]
		public void ShouldReplanWhenCampaignUnproductiveTimeUpdated()
		{
			var campaignId = new Guid();
			var oldCampaign = new Domain.Outbound.Campaign() { UnproductiveTime = 100 };
			oldCampaign.SetId(campaignId);
			var activityId = new Guid();
			oldCampaign.Skill = new Skill();
			var activity = new Activity();
			activity.SetId(activityId);
			oldCampaign.Skill.Activity = activity;
			var newCampaign = new Domain.Outbound.Campaign() { UnproductiveTime = 60 };
			newCampaign.SetId(campaignId);

			var campaignVM = new CampaignViewModel { Id = oldCampaign.Id, Activity = new ActivityViewModel() { Id = activityId } };
			_outboundCampaignRepository.Stub(x => x.Load((Guid)campaignVM.Id).Clone()).Return(oldCampaign);
			_outboundCampaignMapper.Stub(x => x.Map(campaignVM)).Return(newCampaign);

			_target.Persist(campaignVM);

			_productionReplanHelper.AssertWasCalled(x => x.Replan(newCampaign));
		}

		[Test]
		public void ShouldUpdateConnectedSkillNameWhenCampaignNameUpdated()
		{
			var campaignId = new Guid();
			var oldCampaign = new Domain.Outbound.Campaign() { Name = "oldName" };
			oldCampaign.SetId(campaignId);
			var newCampaign = new Domain.Outbound.Campaign() { Name = "newName", Skill = SkillFactory.CreateSkill("oldName") };
			newCampaign.SetId(campaignId);

			var campaignVM = new CampaignViewModel { Id = oldCampaign.Id, Activity = new ActivityViewModel() { Name = "myActivity" } };
			_outboundCampaignRepository.Stub(x => x.Load((Guid)campaignVM.Id).Clone()).Return(oldCampaign);
			_outboundCampaignMapper.Stub(x => x.Map(campaignVM)).Return(newCampaign);

			var result = _target.Persist(campaignVM);

			result.Skill.Name.Should().Be.EqualTo("newName");
			_productionReplanHelper.AssertWasNotCalled(x => x.Replan(newCampaign));
		}

		[Test]
		public void ShouldUpdateConnectedSkillActivityWhenActivityUpdated()
		{
			var skill = SkillFactory.CreateSkill("old");
			skill.Activity = ActivityFactory.CreateActivity("myActivity");
			var campaignId = new Guid();
			var oldCampaign = new Domain.Outbound.Campaign() { Name = "old", Skill = skill };
			oldCampaign.SetId(campaignId);
			var newCampaign = new Domain.Outbound.Campaign() { Name = "old", Skill = skill };
			newCampaign.SetId(campaignId);

			var campaignVM = new CampaignViewModel { Id = oldCampaign.Id, Activity = new ActivityViewModel() { Id = _activity.Id, Name = _activity.Name } };
			_outboundCampaignRepository.Stub(x => x.Load((Guid)campaignVM.Id).Clone()).Return(oldCampaign);
			_outboundCampaignMapper.Stub(x => x.Map(campaignVM)).Return(newCampaign);

			var result = _target.Persist(campaignVM);

			result.Skill.Activity.Name.Should().Be.EqualTo(_activity.Name);
			_productionReplanHelper.AssertWasNotCalled(x => x.Replan(newCampaign));
		}

		[Test]
		public void ShouldMovePeroidWhenSpanningPeriodUpdated()
		{
			var oldPeriod = new DateTimePeriod(new DateTime(2015, 7, 3, 0,0,0,DateTimeKind.Utc), new DateTime(2015, 8, 3, 23, 59,59, DateTimeKind.Utc));
			var campaignId = new Guid();
			var oldCampaign = new Domain.Outbound.Campaign();
			oldCampaign.SetId(campaignId);
			oldCampaign.SpanningPeriod = oldPeriod;
			var newCampaign = new Domain.Outbound.Campaign();
			newCampaign.SetId(campaignId);
			newCampaign.SpanningPeriod = new DateTimePeriod(new DateTime(2015, 7, 4, 0, 0, 0, DateTimeKind.Utc), new DateTime(2015, 8, 3, 23, 59, 59, DateTimeKind.Utc));
			var skill = SkillFactory.CreateSkill("mySkill");
			skill.TimeZone = TimeZoneInfo.Utc;
			skill.AddWorkload(WorkloadFactory.CreateWorkload(skill));
			newCampaign.Skill = skill;

			var campaignVM = new CampaignViewModel { Id = oldCampaign.Id, Activity = new ActivityViewModel(){Name = "myActivity"} };
			_outboundCampaignRepository.Stub(x => x.Load((Guid)campaignVM.Id).Clone()).Return(oldCampaign);
			_outboundCampaignMapper.Stub(x => x.Map(campaignVM)).Return(newCampaign);

			_target.Persist(campaignVM);

			_outboundPeriodMover.AssertWasCalled(x => x.Move(newCampaign, oldPeriod.ToDateOnlyPeriod(newCampaign.Skill.TimeZone)));
		}

		[Test]
		public void ShouldNotMovePeroidWhenAnyPeriodNotUpdated()
		{
			var oldPeriod = new DateTimePeriod(new DateTime(2015, 7, 3, 0, 0, 0, DateTimeKind.Utc), new DateTime(2015, 8, 3, 23, 59, 59, DateTimeKind.Utc));
			var campaignId = new Guid();
			var oldCampaign = new Domain.Outbound.Campaign();
			oldCampaign.SetId(campaignId);
			oldCampaign.SpanningPeriod = oldPeriod;
			var newCampaign = new Domain.Outbound.Campaign();
			newCampaign.SetId(campaignId);
			newCampaign.SpanningPeriod = oldPeriod;
			var skill = SkillFactory.CreateSkill("mySkill");
			skill.TimeZone = TimeZoneInfo.Utc;
			newCampaign.Skill = skill;

			var campaignVM = new CampaignViewModel { Id = oldCampaign.Id, Activity = new ActivityViewModel() { Name = "myActivity" } };
			_outboundCampaignRepository.Stub(x => x.Load((Guid)campaignVM.Id).Clone()).Return(oldCampaign);
			_outboundCampaignMapper.Stub(x => x.Map(campaignVM)).Return(newCampaign);

			_target.Persist(campaignVM);

			_outboundPeriodMover.AssertWasNotCalled(x => x.Move(newCampaign, oldPeriod.ToDateOnlyPeriod(newCampaign.Skill.TimeZone)));
		}

		[Test]
		public void ShouldMovePeroidWhenWorkHoursUpdated()
		{
			var oldPeriod = new TimePeriod(new TimeSpan(9, 0, 0), new TimeSpan(15, 0, 0));
			var campaignId = new Guid();
			var oldCampaign = new Domain.Outbound.Campaign() { SpanningPeriod = new DateTimePeriod(new DateTime(2015, 7, 4, 0, 0, 0, DateTimeKind.Utc), new DateTime(2015, 8, 3, 23, 59, 59, DateTimeKind.Utc)) };
			oldCampaign.SetId(campaignId);
			oldCampaign.WorkingHours.Add(DayOfWeek.Monday, oldPeriod);
			var newCampaign = new Domain.Outbound.Campaign() { SpanningPeriod = new DateTimePeriod(new DateTime(2015, 7, 4, 0, 0, 0, DateTimeKind.Utc), new DateTime(2015, 8, 3, 23, 59, 59, DateTimeKind.Utc)) };
			newCampaign.SetId(campaignId);
			newCampaign.WorkingHours.Add(DayOfWeek.Monday, new TimePeriod());
			var skill = SkillFactory.CreateSkill("mySkill");
			skill.AddWorkload(WorkloadFactory.CreateWorkload(skill));
			skill.TimeZone = TimeZoneInfo.Utc;
			newCampaign.Skill = skill;

			var campaignVM = new CampaignViewModel { Id = oldCampaign.Id, Activity = new ActivityViewModel() { Name = "myActivity" } };
			_outboundCampaignRepository.Stub(x => x.Load((Guid)campaignVM.Id).Clone()).Return(oldCampaign);
			_outboundCampaignMapper.Stub(x => x.Map(campaignVM)).Return(newCampaign);

			_target.Persist(campaignVM);

			_outboundPeriodMover.AssertWasCalled(x => x.Move(newCampaign, oldCampaign.SpanningPeriod.ToDateOnlyPeriod(newCampaign.Skill.TimeZone)));
		}

		[Test]
		public void ShouldMovePeroidWhenWeekDayAdded()
		{
			var oldPeriod = new TimePeriod(new TimeSpan(9, 0, 0), new TimeSpan(15, 0, 0));
			var campaignId = new Guid();
			var oldCampaign = new Domain.Outbound.Campaign { SpanningPeriod = new DateTimePeriod(new DateTime(2015, 7, 4, 0, 0, 0, DateTimeKind.Utc), new DateTime(2015, 8, 3, 23, 59, 59, DateTimeKind.Utc)) };
			oldCampaign.SetId(campaignId);
			oldCampaign.WorkingHours.Add(DayOfWeek.Monday, oldPeriod);
			var newCampaign = new Domain.Outbound.Campaign { SpanningPeriod = new DateTimePeriod(new DateTime(2015, 7, 4, 0, 0, 0, DateTimeKind.Utc), new DateTime(2015, 8, 3, 23, 59, 59, DateTimeKind.Utc)) };
			newCampaign.SetId(campaignId);
			newCampaign.WorkingHours.Add(DayOfWeek.Thursday, oldPeriod);
			var skill = SkillFactory.CreateSkill("mySkill");
			skill.AddWorkload(WorkloadFactory.CreateWorkload(skill));
			skill.TimeZone = TimeZoneInfo.Utc;
			newCampaign.Skill = skill;

			var campaignVM = new CampaignViewModel { Id = oldCampaign.Id, Activity = new ActivityViewModel() { Name = "myActivity" } };
			_outboundCampaignRepository.Stub(x => x.Load(campaignVM.Id.GetValueOrDefault()).Clone()).Return(oldCampaign);
			_outboundCampaignMapper.Stub(x => x.Map(campaignVM)).Return(newCampaign);

			_target.Persist(campaignVM);

			_outboundPeriodMover.AssertWasCalled(x => x.Move(newCampaign, oldCampaign.SpanningPeriod.ToDateOnlyPeriod(newCampaign.Skill.TimeZone)));
		}

		[Test]
		public void ShouldMovePeroidWhenWeekDayRemoved()
		{
			var oldPeriod = new TimePeriod(9, 0, 15, 0);
			var campaignId = Guid.NewGuid();
			var oldCampaign = new Domain.Outbound.Campaign { SpanningPeriod = new DateTimePeriod(new DateTime(2015, 7, 4, 0, 0, 0, DateTimeKind.Utc), new DateTime(2015, 8, 3, 23, 59, 59, DateTimeKind.Utc)) };
			oldCampaign.SetId(campaignId);
			oldCampaign.WorkingHours.Add(DayOfWeek.Monday, oldPeriod);
			var newCampaign = new Domain.Outbound.Campaign { SpanningPeriod = new DateTimePeriod(new DateTime(2015, 7, 4, 0, 0, 0, DateTimeKind.Utc), new DateTime(2015, 8, 3, 23, 59, 59, DateTimeKind.Utc)) };
			newCampaign.SetId(campaignId);
			newCampaign.WorkingHours.Remove(DayOfWeek.Monday);
			var skill = SkillFactory.CreateSkill("mySkill");
			skill.AddWorkload(WorkloadFactory.CreateWorkload(skill));
			skill.TimeZone = TimeZoneInfo.Utc;
			newCampaign.Skill = skill;

			var campaignVM = new CampaignViewModel { Id = oldCampaign.Id, Activity = new ActivityViewModel() { Name = "myActivity" } };
			_outboundCampaignRepository.Stub(x => x.Load(campaignVM.Id.GetValueOrDefault()).Clone()).Return(oldCampaign);
			_outboundCampaignMapper.Stub(x => x.Map(campaignVM)).Return(newCampaign);

			_target.Persist(campaignVM);

			_outboundPeriodMover.AssertWasCalled(x => x.Move(newCampaign, oldCampaign.SpanningPeriod.ToDateOnlyPeriod(newCampaign.Skill.TimeZone)));
		}

		[Test]
		public void ShouldPersistFromDoubleToTimeSpan()
		{
			var id = new Guid();
			var date = new DateOnly(2015, 7, 20);
			var campaign = new Domain.Outbound.Campaign { SpanningPeriod = new DateTimePeriod(new DateTime(2015, 7, 4, 0, 0, 0, DateTimeKind.Utc), new DateTime(2015, 8, 3, 23, 59, 59, DateTimeKind.Utc)) };
			var skill = SkillFactory.CreateSkill("mySkill");
			skill.TimeZone = TimeZoneInfo.Utc;
			campaign.Skill = skill;

			var productionPlanFactory = new OutboundProductionPlanFactory(new IncomingTaskFactory(new FlatDistributionSetter()));
			var campaignTaskManager = MockRepository.GenerateMock<IOutboundCampaignTaskManager>();
			var incommingTask = productionPlanFactory.CreateAndMakeInitialPlan(campaign.SpanningPeriod.ToDateOnlyPeriod(campaign.Skill.TimeZone), 1000, TimeSpan.FromHours(4), campaign.WorkingHours);
			campaignTaskManager.Stub(x => x.GetIncomingTaskFromCampaign(new Domain.Outbound.Campaign()))
				.IgnoreArguments()
				.Return(incommingTask);
			var createOrUpdateSkillDays = MockRepository.GenerateMock<ICreateOrUpdateSkillDays>();
			createOrUpdateSkillDays.Stub(x => x.Create(null, new DateOnlyPeriod(), 0, new TimeSpan(), null)).IgnoreArguments();

			var manualProductionPlan = new ManualPlanForm
			{
				CampaignId = id,
				ManualProductionPlan = new List<ManualViewModel>()
				{
					new ManualViewModel() {Date = date, Time = 26.56}
				}
			};
			_outboundCampaignRepository.Stub(x => x.Get(id)).Return(campaign);

			_target = new OutboundCampaignPersister(_outboundCampaignRepository, null, null, null, null, null, createOrUpdateSkillDays, null, null, campaignTaskManager, null, _outboundScheduledResourcesCacher);
			_target.PersistManualProductionPlan(manualProductionPlan);

			campaign.GetManualProductionPlan(date).Should().Be.EqualTo(new TimeSpan(1, 2, 33, 35));
			createOrUpdateSkillDays.AssertWasCalled(x => x.UpdateSkillDays(campaign.Skill, incommingTask));
		}

		[Test]
		public void ShouldWriteToCacheForecastWhenPersistingManualProductionPlan()
		{
			var id = new Guid();
			var date = new DateOnly(2015, 7, 20);
			var campaign = new Domain.Outbound.Campaign() { SpanningPeriod = new DateTimePeriod(new DateTime(2015, 7, 4, 0, 0, 0, DateTimeKind.Utc), new DateTime(2015, 8, 3, 23, 59, 59, DateTimeKind.Utc)) };
			var skill = SkillFactory.CreateSkill("mySkill");
			skill.TimeZone = TimeZoneInfo.Utc;
			campaign.Skill = skill;

			var productionPlanFactory = new OutboundProductionPlanFactory(new IncomingTaskFactory(new FlatDistributionSetter()));
			var campaignTaskManager = MockRepository.GenerateMock<IOutboundCampaignTaskManager>();
			var incommingTask = productionPlanFactory.CreateAndMakeInitialPlan(campaign.SpanningPeriod.ToDateOnlyPeriod(campaign.Skill.TimeZone), 1000, TimeSpan.FromHours(4), campaign.WorkingHours);
			campaignTaskManager.Stub(x => x.GetIncomingTaskFromCampaign(new Domain.Outbound.Campaign()))
				.IgnoreArguments()
				.Return(incommingTask);
			var createOrUpdateSkillDays = MockRepository.GenerateMock<ICreateOrUpdateSkillDays>();
			createOrUpdateSkillDays.Stub(x => x.Create(null, new DateOnlyPeriod(), 0, new TimeSpan(), null)).IgnoreArguments();

			var outboundScheduledResourcesCacher = MockRepository.GenerateMock<IOutboundScheduledResourcesCacher>();

			var manualProductionPlan = new ManualPlanForm()
			{
				CampaignId = id,
				ManualProductionPlan = new List<ManualViewModel>()
				{
					new ManualViewModel() {Date = date, Time = 26.56}
				}
			};
			_outboundCampaignRepository.Stub(x => x.Get(id)).Return(campaign);

			_target = new OutboundCampaignPersister(_outboundCampaignRepository, null, null, null, null, null, createOrUpdateSkillDays, null, null, campaignTaskManager, null, outboundScheduledResourcesCacher);
			_target.PersistManualProductionPlan(manualProductionPlan);

			campaign.GetManualProductionPlan(date).Should().Be.EqualTo(new TimeSpan(1, 2, 33, 35));
			createOrUpdateSkillDays.AssertWasCalled(x => x.UpdateSkillDays(campaign.Skill, incommingTask));

			outboundScheduledResourcesCacher.AssertWasCalled(x => x.SetForecastedTime(
				Arg<IOutboundCampaign>.Is.Equal(campaign),
				Arg<Dictionary<DateOnly, TimeSpan>>.Is.Anything));

		}

		[Test]
		public void ShouldNotPersistWhenDateIsOutOfSpanningPeriod()
		{
			var id = new Guid();
			var date = new DateOnly(2015, 7, 3);
			var campaign = new Domain.Outbound.Campaign() { SpanningPeriod = new DateTimePeriod(new DateTime(2015, 7, 4, 0, 0, 0, DateTimeKind.Utc), new DateTime(2015, 8, 3, 23, 59, 59, DateTimeKind.Utc)) };
			var skill = SkillFactory.CreateSkill("mySkill");
			skill.TimeZone = TimeZoneInfo.Utc;
			campaign.Skill = skill;

			var productionPlanFactory = new OutboundProductionPlanFactory(new IncomingTaskFactory(new FlatDistributionSetter()));
			var campaignTaskManager = MockRepository.GenerateMock<IOutboundCampaignTaskManager>();
			var incommingTask = productionPlanFactory.CreateAndMakeInitialPlan(campaign.SpanningPeriod.ToDateOnlyPeriod(campaign.Skill.TimeZone), 1000, TimeSpan.FromHours(4), campaign.WorkingHours);
			campaignTaskManager.Stub(x => x.GetIncomingTaskFromCampaign(new Domain.Outbound.Campaign()))
				.IgnoreArguments()
				.Return(incommingTask);
			var createOrUpdateSkillDays = MockRepository.GenerateMock<ICreateOrUpdateSkillDays>();
			createOrUpdateSkillDays.Stub(x => x.Create(null, new DateOnlyPeriod(), 0, new TimeSpan(), null)).IgnoreArguments();

			var manualProductionPlan = new ManualPlanForm()
			{
				CampaignId = id,
				ManualProductionPlan = new List<ManualViewModel>()
				{
					new ManualViewModel() {Date = date, Time = 26.56}
				}
			};
			_outboundCampaignRepository.Stub(x => x.Get(id)).Return(campaign);

			_target = new OutboundCampaignPersister(_outboundCampaignRepository, null, null, null, null, null, createOrUpdateSkillDays, null, null, campaignTaskManager, null, _outboundScheduledResourcesCacher);
			_target.PersistManualProductionPlan(manualProductionPlan);

			campaign.GetManualProductionPlan(date).Should().Be.EqualTo(null);
			createOrUpdateSkillDays.AssertWasNotCalled(x => x.UpdateSkillDays(campaign.Skill, incommingTask));
		}

		[Test]
		public void ShouldPassThroughSkipedDatesForManualPlan()
		{
			var campaignId = new Guid();
			var date = new DateOnly(2015, 8, 21);
			var campaign = new Domain.Outbound.Campaign() { SpanningPeriod = new DateTimePeriod(new DateTime(2015, 8, 21, 0, 0, 0, DateTimeKind.Utc), new DateTime(2015, 8, 28, 23, 59, 59, DateTimeKind.Utc)) };
			var skill = SkillFactory.CreateSkill("mySkill");
			skill.TimeZone = TimeZoneInfo.Utc;
			campaign.Skill = skill;
			var skipDates = new List<DateOnly>() {date};
			_outboundCampaignRepository.Stub(x => x.Get(campaignId)).Return(campaign);

			var manualPlanForm = new ManualPlanForm(){
				CampaignId = campaignId,
				ManualProductionPlan = new List<ManualViewModel>()
				{
					new ManualViewModel(){Date = date, Time = 8}
				},
				SkipDates = skipDates
			};

			var createOrUpdateSkillDays = MockRepository.GenerateMock<ICreateOrUpdateSkillDays>();
			var taskManager = MockRepository.GenerateMock<IOutboundCampaignTaskManager>();
			_target = new OutboundCampaignPersister(_outboundCampaignRepository, null, null, null, null, null, createOrUpdateSkillDays, _productionReplanHelper, null, taskManager, null, _outboundScheduledResourcesCacher);
			_target.PersistManualProductionPlan(manualPlanForm);

			taskManager.AssertWasCalled((x=>x.GetIncomingTaskFromCampaign(campaign, skipDates)));
		}

		[Test]
		public void ShouldRemoveManualPlan()
		{
			var campaignId = new Guid();
			var date = new DateOnly(2015, 8, 21);
			var campaign = new Domain.Outbound.Campaign() { SpanningPeriod = new DateTimePeriod(new DateTime(2015, 8, 21, 0, 0, 0, DateTimeKind.Utc), new DateTime(2015, 8, 21, 23, 59, 59, DateTimeKind.Utc)) };
			campaign.SetManualProductionPlan(date, TimeSpan.FromHours(1));
			var skipedDates = new List<DateOnly> {date};

			var removeManualForm = new RemoveManualPlanForm() { CampaignId = campaignId, Dates = new List<DateOnly>() { date }, SkipDates = skipedDates};
			_outboundCampaignRepository.Stub(x => x.Get(campaignId)).Return(campaign);

			_target = new OutboundCampaignPersister(_outboundCampaignRepository, null, null, null, null, null, null, _productionReplanHelper, null, null, null, _outboundScheduledResourcesCacher);
			_target.RemoveManualProductionPlan(removeManualForm);

			(campaign.GetManualProductionPlan(date)==null).Should().Be.True();
			_productionReplanHelper.AssertWasCalled(x=>x.Replan(campaign, skipedDates.ToArray()));
		}

		[Test]
		public void ShouldRemoveActualBacklog()
		{
			var campaignId = new Guid();
			var date = new DateOnly(2015, 8, 21);
			var campaign = new Domain.Outbound.Campaign() { SpanningPeriod = new DateTimePeriod(new DateTime(date.Date.Ticks, DateTimeKind.Utc), new DateTime(date.Date.Ticks, DateTimeKind.Utc)) };
			campaign.SetActualBacklog(date, TimeSpan.FromHours(1));

			var removeForm = new RemoveActualBacklogForm() { CampaignId = campaignId, Dates = new List<DateOnly>() { date } };
			_outboundCampaignRepository.Stub(x => x.Get(campaignId)).Return(campaign);

			_target = new OutboundCampaignPersister(_outboundCampaignRepository, null, null, null, null, null, null, _productionReplanHelper, null, null, null, _outboundScheduledResourcesCacher);
			_target.RemoveActualBacklog(removeForm);

			(campaign.GetActualBacklog(date) == null).Should().Be.True();			
		}

		[Test]
		public void ShouldRemoveActivityWhenItIsOutboundActivity()
		{
			var activity = ActivityFactory.CreateActivity("myActivity");
			activity.SetId(new Guid());
			activity.IsOutboundActivity = true;
			var activityRepository = MockRepository.GenerateMock<IActivityRepository>();
			activityRepository.Stub(x => x.Get(activity.Id.Value)).Return(activity);
			var skill = SkillFactory.CreateSkill("skill1");
			skill.Activity = activity;
			_skillRepository.Stub(x => x.LoadAll()).Return(new List<ISkill>()
			{
				skill,
				SkillFactory.CreateSkill("skill2")
			});
			var campaign = new Domain.Outbound.Campaign();
			campaign.Skill = skill;

			_target = new OutboundCampaignPersister(_outboundCampaignRepository, null, null, null, activityRepository, _outboundSkillPersister, null, null, null, null, _skillRepository, _outboundScheduledResourcesCacher);
			_target.RemoveCampaign(campaign);

			activityRepository.AssertWasCalled(x=>x.Remove(activity));
		}

		[Test]
		public void ShouldNotRemoveActivityWhenItIsNotOutboundActivity()
		{
			var activity = ActivityFactory.CreateActivity("myActivity");
			activity.SetId(new Guid());
			activity.IsOutboundActivity = false;
			var activityRepository = MockRepository.GenerateMock<IActivityRepository>();
			activityRepository.Stub(x => x.Get(activity.Id.Value)).Return(activity);
			var skill = SkillFactory.CreateSkill("skill1");
			skill.Activity = activity;
			var campaign = new Domain.Outbound.Campaign();
			campaign.Skill = skill;

			_target = new OutboundCampaignPersister(_outboundCampaignRepository, null, null, null, activityRepository, _outboundSkillPersister, null, null, null, null, _skillRepository, _outboundScheduledResourcesCacher);
			_target.RemoveCampaign(campaign);

			activityRepository.AssertWasNotCalled(x => x.Remove(activity));
		}

		[Test]
		public void ShouldNotRemoveActivityWhenItReferencedByOthers()
		{
			var activity = ActivityFactory.CreateActivity("myActivity");
			activity.SetId(new Guid());
			activity.IsOutboundActivity = true;
			var activityRepository = MockRepository.GenerateMock<IActivityRepository>();
			activityRepository.Stub(x => x.Get(activity.Id.Value)).Return(activity);
			var skill1 = SkillFactory.CreateSkill("skill1");
			skill1.Activity = activity;
			var skill2 = SkillFactory.CreateSkill("skill2");
			skill2.Activity = activity;
			_skillRepository.Stub(x => x.LoadAll()).Return(new List<ISkill>()
			{
				skill1,
				skill2
			});
			var campaign = new Domain.Outbound.Campaign();
			campaign.Skill = skill1;

			_target = new OutboundCampaignPersister(_outboundCampaignRepository, null, null, null, activityRepository, _outboundSkillPersister, null, null, null, null, _skillRepository, _outboundScheduledResourcesCacher);
			_target.RemoveCampaign(campaign);

			activityRepository.AssertWasNotCalled(x => x.Remove(activity));
		}

		[Test]
		public void ShouldRemoveSkillAndCampaign()
		{
			var activity = ActivityFactory.CreateActivity("myActivity");
			activity.SetId(new Guid());
			activity.IsOutboundActivity = false;
			var activityRepository = MockRepository.GenerateMock<IActivityRepository>();
			activityRepository.Stub(x => x.Get(activity.Id.Value)).Return(activity);
			var skill = SkillFactory.CreateSkill("skill1");
			skill.Activity = activity;
			var campaign = new Domain.Outbound.Campaign();
			campaign.Skill = skill;

			_target = new OutboundCampaignPersister(_outboundCampaignRepository, null, null, null, activityRepository, _outboundSkillPersister, null, null, null, null, _skillRepository, _outboundScheduledResourcesCacher);
			_target.RemoveCampaign(campaign);

			_outboundSkillPersister.AssertWasCalled(x => x.RemoveSkill(skill));
			_outboundCampaignRepository.AssertWasCalled(x => x.Remove(campaign));
		}
	}
}
