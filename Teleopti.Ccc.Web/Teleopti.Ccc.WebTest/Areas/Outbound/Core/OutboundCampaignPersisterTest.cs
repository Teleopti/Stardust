using System;
using System.Collections.Generic;
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
	class OutboundCampaignPersisterTest
	{
		private IOutboundCampaignRepository _outboundCampaignRepository;
		private ISkillRepository _skillRepository;
		private IOutboundCampaignMapper _outboundCampaignMapper;
		private IOutboundCampaignViewModelMapper _outboundCampaignViewModelMapper;
		private ISkill _skill;
		private IOutboundSkillCreator _outboundSkillCreator;
		private IActivityRepository _activityRepository;
		private IOutboundSkillPersister _outboundSkillPersister;

		[SetUp]
		public void Setup()
		{
			_outboundCampaignRepository = MockRepository.GenerateMock<IOutboundCampaignRepository>();
			_skillRepository = MockRepository.GenerateMock<ISkillRepository>();
			_outboundCampaignMapper = MockRepository.GenerateMock<IOutboundCampaignMapper>();
			_outboundCampaignViewModelMapper = MockRepository.GenerateMock<IOutboundCampaignViewModelMapper>();
			_outboundSkillCreator = MockRepository.GenerateMock<IOutboundSkillCreator>();
			_activityRepository = MockRepository.GenerateMock<IActivityRepository>();
			_outboundSkillPersister = MockRepository.GenerateMock<IOutboundSkillPersister>();
			var skillType = SkillTypeFactory.CreateSkillType();
			_skill = SkillFactory.CreateSkill("sdfsdf", skillType, 15);
			var skillList = new List<ISkill> { _skill };
			_skillRepository.Stub(x => x.LoadAll()).Return(skillList);
			var activityList = new List<IActivity>() {ActivityFactory.CreateActivity("asfdsa")};
			_activityRepository.Stub(x => x.LoadAll()).Return(activityList);

		}

		[Test]
		public void ShouldStoreNewCampaign()
		{
			var expectedVM = new CampaignViewModel();
			var target = new OutboundCampaignPersister(_outboundCampaignRepository, null, null, _outboundCampaignViewModelMapper, _outboundSkillCreator, _activityRepository, _outboundSkillPersister);
			_outboundCampaignViewModelMapper.Stub(x => x.Map(new Domain.Outbound.Campaign())).IgnoreArguments().Return(expectedVM);

			var result = target.Persist("test");

			_outboundCampaignRepository.AssertWasCalled(x => x.Add(null), o => o.IgnoreArguments());
			result.Should().Be.SameInstanceAs(expectedVM);
		}

		[Test]
		public void ShouldUpdateCampaign()
		{
			var campaignVM = new CampaignViewModel { Id = new Guid(), Skills = new List<SkillViewModel> { new SkillViewModel() { Id = _skill.Id, IsSelected = true} } };
			var target = new OutboundCampaignPersister(_outboundCampaignRepository, _skillRepository, _outboundCampaignMapper, null, null, null, null);
			var expectedCampaign = new Domain.Outbound.Campaign();
			_outboundCampaignMapper.Stub(x => x.Map(campaignVM)).Return(expectedCampaign);

			var result = target.Persist(campaignVM);

			result.Should().Be.SameInstanceAs(expectedCampaign);
		}

		[Test]
		public void ShouldAddWorkingPeriod()
		{
			var _campaign = MockRepository.GenerateMock<Domain.Outbound.Campaign>();
			var form = new CampaignWorkingPeriodForm
			{
				Id = new Guid(),
				StartTime = new TimeSpan(8, 0, 0),
				EndTime = new TimeSpan(9, 0, 0),
				CampaignId = new Guid()
			};

			var target = new OutboundCampaignPersister(_outboundCampaignRepository, _skillRepository, null, null, null, null, null);
			_outboundCampaignRepository.Stub(x => x.Get(form.CampaignId.Value)).IgnoreArguments().Return(_campaign);

			var result  = target.Persist(form);

			result.TimePeriod.StartTime.Should().Be.EqualTo(form.StartTime);
			result.TimePeriod.EndTime.Should().Be.EqualTo(form.EndTime);			
		}

		[Test]
		public void ShouldAddSpeicifiedWokingPeriodAssignment()
		{
			var campaignId = new Guid();
			var campaignWorkingPeriodId = new Guid();

			
			var workingPeriod = MockRepository.GenerateMock<CampaignWorkingPeriod>();
			workingPeriod.Stub(x=> x.Id).Return(campaignWorkingPeriodId);
			workingPeriod.Stub(x => x.CampaignWorkingPeriodAssignments).Return(new HashSet<CampaignWorkingPeriodAssignment>());

			var campaign = MockRepository.GenerateMock<Domain.Outbound.Campaign>();
			_outboundCampaignRepository.Stub(x => x.Get(campaignId)).Return(campaign);

			campaign.Stub(x => x.CampaignWorkingPeriods).Return(new HashSet<CampaignWorkingPeriod>(){workingPeriod});

			var form = new CampaignWorkingPeriodAssignmentForm()
			{
				CampaignId = campaignId,
				CampaignWorkingPeriods = new List<Guid>() {campaignWorkingPeriodId},
				WeekDay = DayOfWeek.Monday
			};

			var target = new OutboundCampaignPersister(_outboundCampaignRepository, null, null, null, null, null, null);
			target.Persist(form);
			workingPeriod.AssertWasCalled(x=> x.AddAssignment(Arg<CampaignWorkingPeriodAssignment>.Matches(a => a.WeekdayIndex.Equals( DayOfWeek.Monday))));
		}

		[Test]
		public void ShouldRemoveSpeicifiedWokingPeriodAssignment()
		{
			var campaignId = new Guid();
			var campaignWorkingPeriodId = new Guid();

			var workingPeriod = MockRepository.GenerateMock<CampaignWorkingPeriod>();
			workingPeriod.Stub(x => x.Id).Return(campaignWorkingPeriodId);
			workingPeriod.Stub(x => x.CampaignWorkingPeriodAssignments)
					.Return(new HashSet<CampaignWorkingPeriodAssignment>() { new CampaignWorkingPeriodAssignment() { WeekdayIndex = DayOfWeek.Monday} });

			var campaign = MockRepository.GenerateMock<Domain.Outbound.Campaign>();
			_outboundCampaignRepository.Stub(x => x.Get(campaignId)).Return(campaign);

			campaign.Stub(x => x.CampaignWorkingPeriods).Return(new HashSet<CampaignWorkingPeriod>() { workingPeriod });

			var form = new CampaignWorkingPeriodAssignmentForm()
			{
				CampaignId = campaignId,
				CampaignWorkingPeriods = new List<Guid>(),
				WeekDay = DayOfWeek.Monday
			};

			var target = new OutboundCampaignPersister(_outboundCampaignRepository, null, null, null, null, null, null);
			target.Persist(form);
			workingPeriod.AssertWasCalled(x => x.RemoveAssignment(Arg<CampaignWorkingPeriodAssignment>.Matches(a => a.WeekdayIndex.Equals(DayOfWeek.Monday))));
		}
	}
}
