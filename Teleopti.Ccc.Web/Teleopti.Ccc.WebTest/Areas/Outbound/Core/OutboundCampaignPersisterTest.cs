using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
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
				new OutboundSkillPersister(_fakeSkillRepository, new FakeWorkloadRepository()), _createOrUpdateSkillDays);
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
			campaigns[0].TargetRate.Should().Be.EqualTo(_campaignInput.CallListLen * _campaignInput.ConnectRate / 100);
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

		[SetUp]
		public void Setup()
		{
			_outboundCampaignRepository = MockRepository.GenerateMock<IOutboundCampaignRepository>();
			_outboundCampaignMapper = MockRepository.GenerateMock<IOutboundCampaignMapper>();
			_outboundCampaignViewModelMapper = MockRepository.GenerateMock<IOutboundCampaignViewModelMapper>();
			_outboundSkillCreator = MockRepository.GenerateMock<IOutboundSkillCreator>();
			_activityRepository = MockRepository.GenerateMock<IActivityRepository>();
			_outboundSkillPersister = MockRepository.GenerateMock<IOutboundSkillPersister>();
			var activityList = new List<IActivity>() {ActivityFactory.CreateActivity("asfdsa")};
			_activityRepository.Stub(x => x.LoadAll()).Return(activityList);
		}

		[Test]
		public void ShouldStoreNewCampaign()
		{
			var expectedVM = new CampaignViewModel();
			var createOrUpdateSkillDays = MockRepository.GenerateMock<ICreateOrUpdateSkillDays>();
			createOrUpdateSkillDays.Stub(x => x.Create(null, new DateOnlyPeriod(), 0, new TimeSpan(), null)).IgnoreArguments();
			var target = new OutboundCampaignPersister(_outboundCampaignRepository, null, _outboundCampaignViewModelMapper, _outboundSkillCreator, _activityRepository, _outboundSkillPersister, createOrUpdateSkillDays);
			_outboundCampaignViewModelMapper.Stub(x => x.Map(new Domain.Outbound.Campaign())).IgnoreArguments().Return(expectedVM);

			var result = target.Persist(new CampaignForm()
			{
				Name = "myCampaign",
				CallListLen = 1000,
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
			var campaignVM = new CampaignViewModel { Id = new Guid(), ActivityId = new Guid()};
			var target = new OutboundCampaignPersister(_outboundCampaignRepository, _outboundCampaignMapper, null, null, null, null, null);
			var expectedCampaign = new Domain.Outbound.Campaign();
			_outboundCampaignMapper.Stub(x => x.Map(campaignVM)).Return(expectedCampaign);

			var result = target.Persist(campaignVM);

			result.Should().Be.SameInstanceAs(expectedCampaign);
		}


	}
}
