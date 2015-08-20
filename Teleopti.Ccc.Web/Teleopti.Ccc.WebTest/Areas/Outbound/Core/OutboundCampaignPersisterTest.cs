using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Backlog;
using Teleopti.Ccc.Domain.Outbound;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Persisters.Outbound;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.Mapping;
using Teleopti.Ccc.Web.Areas.Outbound.Models;
using Teleopti.Interfaces.Domain;

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
			
			_outboundCampaignPersister = new OutboundCampaignPersister(_fakeCampaignRepository,
				new OutboundCampaignMapper(_fakeCampaignRepository), new OutboundCampaignViewModelMapper(),
				skillCreator, _fakeActivityRepository,
				new OutboundSkillPersister(_fakeSkillRepository, new FakeWorkloadRepository()), _createOrUpdateSkillDays, null, null, null);
		}

		[Test]
		public void ShouldPersistSkillWhenPersistingCampaign()
		{
			_outboundCampaignPersister.Persist(_campaignInput);

			var campaigns = _fakeCampaignRepository.LoadAll();

			campaigns.Count.Should().Be.EqualTo(1);
			campaigns[0].Skill.Should().Be.SameInstanceAs(_fakeSkillRepository.LoadAll().First());
		}		
		
		[Test]
		public void ShouldSetCampaignName()
		{
			_outboundCampaignPersister.Persist(_campaignInput);

			var campaigns = _fakeCampaignRepository.LoadAll();
			campaigns[0].Name.Should().Be.EqualTo(_campaignInput.Name);
		}

		[Test]
		public void ShouldSetCallListLen()
		{
			_outboundCampaignPersister.Persist(_campaignInput);

			var campaigns = _fakeCampaignRepository.LoadAll();
			campaigns[0].CallListLen.Should().Be.EqualTo(_campaignInput.CallListLen);
		}

		[Test]
		public void ShouldSetTargetRate()
		{
			_outboundCampaignPersister.Persist(_campaignInput);

			var campaigns = _fakeCampaignRepository.LoadAll();
			campaigns[0].TargetRate.Should().Be.EqualTo(_campaignInput.TargetRate);
		}

		[Test]
		public void ShouldSetConnectRate()
		{
			_outboundCampaignPersister.Persist(_campaignInput);

			var campaigns = _fakeCampaignRepository.LoadAll();
			campaigns[0].ConnectRate.Should().Be.EqualTo(_campaignInput.ConnectRate);
		}

		[Test]
		public void ShouldSetRightPartyConnectRate()
		{
			_outboundCampaignPersister.Persist(_campaignInput);

			var campaigns = _fakeCampaignRepository.LoadAll();
			campaigns[0].RightPartyConnectRate.Should().Be.EqualTo(_campaignInput.RightPartyConnectRate);
		}

		[Test]
		public void ShouldSetConnectAverageHandlingTime()
		{
			_outboundCampaignPersister.Persist(_campaignInput);

			var campaigns = _fakeCampaignRepository.LoadAll();
			campaigns[0].ConnectAverageHandlingTime.Should().Be.EqualTo(_campaignInput.ConnectAverageHandlingTime);
		}

		[Test]
		public void ShouldSetRightPartyAverageHandlingTime()
		{
			_outboundCampaignPersister.Persist(_campaignInput);

			var campaigns = _fakeCampaignRepository.LoadAll();
			campaigns[0].RightPartyAverageHandlingTime.Should().Be.EqualTo(_campaignInput.RightPartyAverageHandlingTime);
		}

		[Test]
		public void ShouldSetUnproductiveTime()
		{
			_outboundCampaignPersister.Persist(_campaignInput);

			var campaigns = _fakeCampaignRepository.LoadAll();
			campaigns[0].UnproductiveTime.Should().Be.EqualTo(_campaignInput.UnproductiveTime);
		}

		[Test]
		public void ShouldSetSpanningPeriod()
		{
			_outboundCampaignPersister.Persist(_campaignInput);

			var campaigns = _fakeCampaignRepository.LoadAll();
			campaigns[0].SpanningPeriod.Should().Be.EqualTo(new DateOnlyPeriod(_campaignInput.StartDate, _campaignInput.EndDate));
		}

		[Test]
		public void ShouldSetWorkingHours()
		{
			_outboundCampaignPersister.Persist(_campaignInput);
			
			var campaigns = _fakeCampaignRepository.LoadAll();
			for (var i = 0; i < 7; ++i)
			{
				campaigns[0].WorkingHours[_campaignInput.WorkingHours[i].WeekDay].Should().Be.EqualTo(new TimePeriod(_campaignInput.WorkingHours[i].StartTime, (_campaignInput.WorkingHours[i].EndTime)));
			}
		}

		[Test]
		public void ShouldCreateActivityWhenUserDidNotChooseAnyExisted()
		{
			_outboundCampaignPersister.Persist(_campaignInput);

			_fakeActivityRepository.LoadAll().First().Name.Should().Be.EqualTo(_campaignInput.Activity.Name);
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

			_createOrUpdateSkillDays.AssertWasCalled(x => x.Create(campaign.Skill, campaign.SpanningPeriod, campaign.CampaignTasks(), campaign.AverageTaskHandlingTime(), campaign.WorkingHours));
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
		private IList<IActivity> _activityList;
		private IOutboundPeriodMover _outboundPeriodMover;

		private Guid _campaignId;
		private IOutboundCampaign _campaign;

		[SetUp]
		public void Setup()
		{
			_outboundCampaignRepository = MockRepository.GenerateMock<IOutboundCampaignRepository>();
			_outboundCampaignMapper = MockRepository.GenerateMock<IOutboundCampaignMapper>();
			_outboundCampaignViewModelMapper = MockRepository.GenerateMock<IOutboundCampaignViewModelMapper>();
			_outboundSkillCreator = MockRepository.GenerateMock<IOutboundSkillCreator>();
			_activityRepository = MockRepository.GenerateMock<IActivityRepository>();
			_outboundSkillPersister = MockRepository.GenerateMock<IOutboundSkillPersister>();
			_productionReplanHelper = MockRepository.GenerateMock<IProductionReplanHelper>();
			_outboundPeriodMover = MockRepository.GenerateMock<IOutboundPeriodMover>();
			_activityList = new List<IActivity>() { ActivityFactory.CreateActivity("aa") };
			_activityRepository.Stub(x => x.LoadAll()).Return(_activityList);
			_productionReplanHelper.Stub(x => x.Replan(null)).IgnoreArguments();
			_outboundPeriodMover.Stub(x => x.Move(null, new DateOnlyPeriod())).IgnoreArguments();

			_campaignId = Guid.NewGuid();
			_campaign = new Domain.Outbound.Campaign();

			_outboundCampaignRepository.Stub(x => x.Get(_campaignId)).Return(_campaign);
			_target = new OutboundCampaignPersister(_outboundCampaignRepository, _outboundCampaignMapper, null, _outboundSkillCreator, _activityRepository, null, null, _productionReplanHelper, _outboundPeriodMover, null);
		}


		[Test]
		public void ManualReplanShouldDelegateToReplanHelper()
		{
			_target.ManualReplanCampaign(_campaignId);
			_productionReplanHelper.AssertWasCalled( x => x.Replan(_campaign));
		}

		[Test]
		public void ShouldStoreNewCampaign()
		{
			var expectedVM = new CampaignViewModel();
			var createOrUpdateSkillDays = MockRepository.GenerateMock<ICreateOrUpdateSkillDays>();
			createOrUpdateSkillDays.Stub(x => x.Create(null, new DateOnlyPeriod(), 0, new TimeSpan(), null)).IgnoreArguments();
			var target = new OutboundCampaignPersister(_outboundCampaignRepository, null, _outboundCampaignViewModelMapper, _outboundSkillCreator, _activityRepository, 
				_outboundSkillPersister, createOrUpdateSkillDays, null, null, null);
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
			var campaignVM = new CampaignViewModel { Id = new Guid(), Activity = new ActivityViewModel()};
			var target = new OutboundCampaignPersister(_outboundCampaignRepository, _outboundCampaignMapper, null, null, null, null, null, _productionReplanHelper, _outboundPeriodMover, null);
			var expectedCampaign = new Domain.Outbound.Campaign();
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
			var newCampaign = new Domain.Outbound.Campaign() { CallListLen = 2000 };
			newCampaign.SetId(campaignId);

			var campaignVM = new CampaignViewModel { Id = oldCampaign.Id, Activity = new ActivityViewModel() };
			_outboundCampaignRepository.Stub(x => x.Load((Guid)campaignVM.Id).Clone()).Return(oldCampaign);
			_outboundCampaignMapper.Stub(x => x.Map(campaignVM)).Return(newCampaign);

			_target.Persist(campaignVM);

			_productionReplanHelper.AssertWasCalled(x => x.Replan(newCampaign));
		}		
		
		[Test]
		public void ShouldReplanWhenCampaignTargetRateUpdated()
		{
			var campaignId = new Guid();
			var oldCampaign = new Domain.Outbound.Campaign(){TargetRate = 50};
			oldCampaign.SetId(campaignId);
			var newCampaign = new Domain.Outbound.Campaign() { TargetRate = 60 };
			newCampaign.SetId(campaignId);

			var campaignVM = new CampaignViewModel { Id = oldCampaign.Id, Activity = new ActivityViewModel()};
			_outboundCampaignRepository.Stub(x => x.Load((Guid)campaignVM.Id).Clone()).Return(oldCampaign);
			_outboundCampaignMapper.Stub(x => x.Map(campaignVM)).Return(newCampaign);

			_target.Persist(campaignVM);

			_productionReplanHelper.AssertWasCalled(x => x.Replan(newCampaign));
		}		
		
		[Test]
		public void ShouldReplanWhenCampaignConnectRateUpdated()
		{
			var campaignId = new Guid();
			var oldCampaign = new Domain.Outbound.Campaign(){ConnectRate = 100};
			oldCampaign.SetId(campaignId);
			var newCampaign = new Domain.Outbound.Campaign() { ConnectRate = 60 };
			newCampaign.SetId(campaignId);

			var campaignVM = new CampaignViewModel { Id = oldCampaign.Id, Activity = new ActivityViewModel()};
			_outboundCampaignRepository.Stub(x => x.Load((Guid)campaignVM.Id).Clone()).Return(oldCampaign);
			_outboundCampaignMapper.Stub(x => x.Map(campaignVM)).Return(newCampaign);

			_target.Persist(campaignVM);

			_productionReplanHelper.AssertWasCalled(x => x.Replan(newCampaign));
		}		
		
		[Test]
		public void ShouldReplanWhenCampaignRightPartyConnectRateUpdated()
		{
			var campaignId = new Guid();
			var oldCampaign = new Domain.Outbound.Campaign(){RightPartyConnectRate = 100};
			oldCampaign.SetId(campaignId);
			var newCampaign = new Domain.Outbound.Campaign() { RightPartyConnectRate = 60 };
			newCampaign.SetId(campaignId);

			var campaignVM = new CampaignViewModel { Id = oldCampaign.Id, Activity = new ActivityViewModel() };
			_outboundCampaignRepository.Stub(x => x.Load((Guid)campaignVM.Id).Clone()).Return(oldCampaign);
			_outboundCampaignMapper.Stub(x => x.Map(campaignVM)).Return(newCampaign);

			_target.Persist(campaignVM);

			_productionReplanHelper.AssertWasCalled(x => x.Replan(newCampaign));
		}		
		
		[Test]
		public void ShouldReplanWhenCampaignConnectAverageHandlingTimeUpdated()
		{
			var campaignId = new Guid();
			var oldCampaign = new Domain.Outbound.Campaign(){ConnectAverageHandlingTime = 100};
			oldCampaign.SetId(campaignId);
			var newCampaign = new Domain.Outbound.Campaign() { ConnectAverageHandlingTime = 60 };
			newCampaign.SetId(campaignId);

			var campaignVM = new CampaignViewModel { Id = oldCampaign.Id, Activity = new ActivityViewModel() };
			_outboundCampaignRepository.Stub(x => x.Load((Guid)campaignVM.Id).Clone()).Return(oldCampaign);
			_outboundCampaignMapper.Stub(x => x.Map(campaignVM)).Return(newCampaign);

			_target.Persist(campaignVM);

			_productionReplanHelper.AssertWasCalled(x => x.Replan(newCampaign));
		}		
		
		[Test]
		public void ShouldReplanWhenCampaignRightPartyAverageHandlingTimeUpdated()
		{
			var campaignId = new Guid();
			var oldCampaign = new Domain.Outbound.Campaign(){RightPartyAverageHandlingTime = 100};
			oldCampaign.SetId(campaignId);
			var newCampaign = new Domain.Outbound.Campaign() { RightPartyAverageHandlingTime = 60 };
			newCampaign.SetId(campaignId);

			var campaignVM = new CampaignViewModel { Id = oldCampaign.Id, Activity = new ActivityViewModel() };
			_outboundCampaignRepository.Stub(x => x.Load((Guid)campaignVM.Id).Clone()).Return(oldCampaign);
			_outboundCampaignMapper.Stub(x => x.Map(campaignVM)).Return(newCampaign);

			_target.Persist(campaignVM);

			_productionReplanHelper.AssertWasCalled(x => x.Replan(newCampaign));
		}		
		
		[Test]
		public void ShouldReplanWhenCampaignUnproductiveTimeUpdated()
		{
			var campaignId = new Guid();
			var oldCampaign = new Domain.Outbound.Campaign(){UnproductiveTime = 100};
			oldCampaign.SetId(campaignId);
			var newCampaign = new Domain.Outbound.Campaign() { UnproductiveTime = 60 };
			newCampaign.SetId(campaignId);

			var campaignVM = new CampaignViewModel { Id = oldCampaign.Id, Activity = new ActivityViewModel() };
			_outboundCampaignRepository.Stub(x => x.Load((Guid)campaignVM.Id).Clone()).Return(oldCampaign);
			_outboundCampaignMapper.Stub(x => x.Map(campaignVM)).Return(newCampaign);

			_target.Persist(campaignVM);

			_productionReplanHelper.AssertWasCalled(x => x.Replan(newCampaign));
		}		
		
		[Test]
		public void ShouldUpdateConnectedSkillNameWhenCampaignNameUpdated()
		{
			var campaignId = new Guid();
			var oldCampaign = new Domain.Outbound.Campaign(){Name = "oldName"};
			oldCampaign.SetId(campaignId);
			var newCampaign = new Domain.Outbound.Campaign() { Name = "newName", Skill = SkillFactory.CreateSkill("oldName") };
			newCampaign.SetId(campaignId);

			var campaignVM = new CampaignViewModel { Id = oldCampaign.Id, Activity = new ActivityViewModel() };
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

			var campaignVM = new CampaignViewModel { Id = oldCampaign.Id, Activity = new ActivityViewModel() { Id = _activityList.First().Id, Name = _activityList.First().Name} };
			_outboundCampaignRepository.Stub(x => x.Load((Guid)campaignVM.Id).Clone()).Return(oldCampaign);
			_outboundCampaignMapper.Stub(x => x.Map(campaignVM)).Return(newCampaign);

			var result = _target.Persist(campaignVM);

			result.Skill.Activity.Name.Should().Be.EqualTo(_activityList.First().Name);
			_productionReplanHelper.AssertWasNotCalled(x => x.Replan(newCampaign));
		}		
		
		[Test]
		public void ShouldMovePeroidWhenSpanningPeriodUpdated()
		{
			var oldPeriod = new DateOnlyPeriod(new DateOnly(2015,7,3), new DateOnly(2015,8,3));
			var campaignId = new Guid();
			var oldCampaign = new Domain.Outbound.Campaign();
			oldCampaign.SetId(campaignId);
			oldCampaign.SpanningPeriod = oldPeriod;
			var newCampaign = new Domain.Outbound.Campaign();
			newCampaign.SetId(campaignId);
			newCampaign.SpanningPeriod = new DateOnlyPeriod(new DateOnly(2015,7,4), new DateOnly(2015,8,3));
			var skill = SkillFactory.CreateSkill("mySkill");
			skill.AddWorkload(WorkloadFactory.CreateWorkload(skill));
			newCampaign.Skill = skill;

			var campaignVM = new CampaignViewModel { Id = oldCampaign.Id, Activity = new ActivityViewModel()};
			_outboundCampaignRepository.Stub(x => x.Load((Guid)campaignVM.Id).Clone()).Return(oldCampaign);
			_outboundCampaignMapper.Stub(x => x.Map(campaignVM)).Return(newCampaign);

			_target.Persist(campaignVM);

			_outboundPeriodMover.AssertWasCalled(x => x.Move(newCampaign, oldPeriod));
		}		
		
		[Test]
		public void ShouldNotMovePeroidWhenAnyPeriodNotUpdated()
		{
			var oldPeriod = new DateOnlyPeriod(new DateOnly(2015,7,3), new DateOnly(2015,8,3));
			var campaignId = new Guid();
			var oldCampaign = new Domain.Outbound.Campaign();
			oldCampaign.SetId(campaignId);
			oldCampaign.SpanningPeriod = oldPeriod;
			var newCampaign = new Domain.Outbound.Campaign();
			newCampaign.SetId(campaignId);
			newCampaign.SpanningPeriod = oldPeriod;

			var campaignVM = new CampaignViewModel { Id = oldCampaign.Id, Activity = new ActivityViewModel()};
			_outboundCampaignRepository.Stub(x => x.Load((Guid)campaignVM.Id).Clone()).Return(oldCampaign);
			_outboundCampaignMapper.Stub(x => x.Map(campaignVM)).Return(newCampaign);

			_target.Persist(campaignVM);

			_outboundPeriodMover.AssertWasNotCalled(x => x.Move(newCampaign, oldPeriod));
		}

		[Test]
		public void ShouldMovePeroidWhenWorkHoursUpdated()
		{
			var oldPeriod = new TimePeriod(new TimeSpan(9,0,0), new TimeSpan(15,0,0));
			var campaignId = new Guid();
			var oldCampaign = new Domain.Outbound.Campaign() { SpanningPeriod = new DateOnlyPeriod(new DateOnly(2015, 7, 4), new DateOnly(2015, 8, 3)) };
			oldCampaign.SetId(campaignId);
			oldCampaign.WorkingHours.Add(DayOfWeek.Monday, oldPeriod);
			var newCampaign = new Domain.Outbound.Campaign() { SpanningPeriod = new DateOnlyPeriod(new DateOnly(2015, 7, 4), new DateOnly(2015, 8, 3)) };
			newCampaign.SetId(campaignId);
			newCampaign.WorkingHours.Add(DayOfWeek.Monday, new TimePeriod());
			var skill = SkillFactory.CreateSkill("mySkill");
			skill.AddWorkload(WorkloadFactory.CreateWorkload(skill));
			newCampaign.Skill = skill;

			var campaignVM = new CampaignViewModel { Id = oldCampaign.Id, Activity = new ActivityViewModel() };
			_outboundCampaignRepository.Stub(x => x.Load((Guid)campaignVM.Id).Clone()).Return(oldCampaign);
			_outboundCampaignMapper.Stub(x => x.Map(campaignVM)).Return(newCampaign);

			_target.Persist(campaignVM);

			_outboundPeriodMover.AssertWasCalled(x => x.Move(newCampaign, oldCampaign.SpanningPeriod));
		}		
		
		[Test]
		public void ShouldMovePeroidWhenWeekDayAdded()
		{
			var oldPeriod = new TimePeriod(new TimeSpan(9,0,0), new TimeSpan(15,0,0));
			var campaignId = new Guid();
			var oldCampaign = new Domain.Outbound.Campaign() { SpanningPeriod = new DateOnlyPeriod(new DateOnly(2015, 7, 4), new DateOnly(2015, 8, 3)) };
			oldCampaign.SetId(campaignId);
			oldCampaign.WorkingHours.Add(DayOfWeek.Monday, oldPeriod);
			var newCampaign = new Domain.Outbound.Campaign() { SpanningPeriod = new DateOnlyPeriod(new DateOnly(2015, 7, 4), new DateOnly(2015, 8, 3)) };
			newCampaign.SetId(campaignId);
			newCampaign.WorkingHours.Add(DayOfWeek.Thursday, oldPeriod);
			var skill = SkillFactory.CreateSkill("mySkill");
			skill.AddWorkload(WorkloadFactory.CreateWorkload(skill));
			newCampaign.Skill = skill;

			var campaignVM = new CampaignViewModel { Id = oldCampaign.Id, Activity = new ActivityViewModel() };
			_outboundCampaignRepository.Stub(x => x.Load((Guid)campaignVM.Id).Clone()).Return(oldCampaign);
			_outboundCampaignMapper.Stub(x => x.Map(campaignVM)).Return(newCampaign);

			_target.Persist(campaignVM);

			_outboundPeriodMover.AssertWasCalled(x => x.Move(newCampaign, oldCampaign.SpanningPeriod));
		}		
		
		[Test]
		public void ShouldMovePeroidWhenWeekDayRemoved()
		{
			var oldPeriod = new TimePeriod(new TimeSpan(9,0,0), new TimeSpan(15,0,0));
			var campaignId = new Guid();
			var oldCampaign = new Domain.Outbound.Campaign() { SpanningPeriod = new DateOnlyPeriod(new DateOnly(2015, 7, 4), new DateOnly(2015, 8, 3)) };
			oldCampaign.SetId(campaignId);
			oldCampaign.WorkingHours.Add(DayOfWeek.Monday, oldPeriod);
			var newCampaign = new Domain.Outbound.Campaign() { SpanningPeriod = new DateOnlyPeriod(new DateOnly(2015, 7, 4), new DateOnly(2015, 8, 3)) };
			newCampaign.SetId(campaignId);
			newCampaign.WorkingHours.Remove(DayOfWeek.Monday);
			var skill = SkillFactory.CreateSkill("mySkill");
			skill.AddWorkload(WorkloadFactory.CreateWorkload(skill));
			newCampaign.Skill = skill;

			var campaignVM = new CampaignViewModel { Id = oldCampaign.Id, Activity = new ActivityViewModel() };
			_outboundCampaignRepository.Stub(x => x.Load((Guid)campaignVM.Id).Clone()).Return(oldCampaign);
			_outboundCampaignMapper.Stub(x => x.Map(campaignVM)).Return(newCampaign);

			_target.Persist(campaignVM);

			_outboundPeriodMover.AssertWasCalled(x => x.Move(newCampaign, oldCampaign.SpanningPeriod));
		}

		[Test]
		public void ShouldPersistFromDoubleToTimeSpan()
		{
			var id = new Guid();
			var date = new DateOnly(2015, 7, 20);
			var campaign = new Domain.Outbound.Campaign() { SpanningPeriod = new DateOnlyPeriod(new DateOnly(2015, 7, 4), new DateOnly(2015, 8, 3)) };

			var productionPlanFactory = new OutboundProductionPlanFactory(new IncomingTaskFactory(new FlatDistributionSetter()));
			var campaignTaskManager = MockRepository.GenerateMock<IOutboundCampaignTaskManager>();
			var incommingTask = productionPlanFactory.CreateAndMakeInitialPlan(campaign.SpanningPeriod, 1000, TimeSpan.FromHours(4), campaign.WorkingHours);
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

			_target = new OutboundCampaignPersister(_outboundCampaignRepository, null, null, null, null, null, createOrUpdateSkillDays, null, null, campaignTaskManager);
			_target.PersistManualProductionPlan(manualProductionPlan);

			campaign.GetManualProductionPlan(date).Should().Be.EqualTo(new TimeSpan(1, 2, 33, 35));
			createOrUpdateSkillDays.AssertWasCalled(x => x.UpdateSkillDays(campaign.Skill, incommingTask));
		}

		[Test]
		public void ShouldNotPersistWhenDateIsOutOfSpanningPeriod()
		{
			var id = new Guid();
			var date = new DateOnly(2015, 7, 3);
			var campaign = new Domain.Outbound.Campaign() { SpanningPeriod = new DateOnlyPeriod(new DateOnly(2015, 7, 4), new DateOnly(2015, 8, 3)) };

			var productionPlanFactory = new OutboundProductionPlanFactory(new IncomingTaskFactory(new FlatDistributionSetter()));
			var campaignTaskManager = MockRepository.GenerateMock<IOutboundCampaignTaskManager>();
			var incommingTask = productionPlanFactory.CreateAndMakeInitialPlan(campaign.SpanningPeriod, 1000, TimeSpan.FromHours(4), campaign.WorkingHours);
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

			_target = new OutboundCampaignPersister(_outboundCampaignRepository, null, null, null, null, null, createOrUpdateSkillDays, null, null, campaignTaskManager);
			_target.PersistManualProductionPlan(manualProductionPlan);

			campaign.GetManualProductionPlan(date).Should().Be.EqualTo(null);
			createOrUpdateSkillDays.AssertWasNotCalled(x=>x.UpdateSkillDays(campaign.Skill, incommingTask));
		}
	}
}
