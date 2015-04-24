using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Outbound;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Authentication;
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
		private IUserTimeZone _userTimeZone;

		[SetUp]
		public void Setup()
		{
			_outboundCampaignRepository = MockRepository.GenerateMock<IOutboundCampaignRepository>();
			_skillRepository = MockRepository.GenerateMock<ISkillRepository>();
			_outboundCampaignMapper = MockRepository.GenerateMock<IOutboundCampaignMapper>();
			_outboundCampaignViewModelMapper = MockRepository.GenerateMock<IOutboundCampaignViewModelMapper>();
			_userTimeZone = MockRepository.GenerateMock<IUserTimeZone>();			
			var skillType = SkillTypeFactory.CreateSkillType();
			_skill = SkillFactory.CreateSkill("sdfsdf", skillType, 15);
			var skillList = new List<ISkill> { _skill };
			_skillRepository.Stub(x => x.LoadAll()).Return(skillList);
			
		}

		[Test]
		public void ShouldStoreNewCampaign()
		{
			var expectedVM = new CampaignViewModel();
			var target = new OutboundCampaignPersister(_outboundCampaignRepository, _skillRepository, null, _outboundCampaignViewModelMapper, null);
			_outboundCampaignViewModelMapper.Stub(x => x.Map(new Domain.Outbound.Campaign())).IgnoreArguments().Return(expectedVM);

			var result = target.Persist("test");

			_outboundCampaignRepository.AssertWasCalled(x => x.Add(null), o => o.IgnoreArguments());
			result.Should().Be.SameInstanceAs(expectedVM);
		}

		[Test]
		public void ShouldUpdateCampaign()
		{
			var campaignVM = new CampaignViewModel { Id = new Guid(), Skills = new List<SkillViewModel> { new SkillViewModel() { Id = _skill.Id, IsSelected = true} } };
			var target = new OutboundCampaignPersister(_outboundCampaignRepository, _skillRepository, _outboundCampaignMapper, null, null);
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

			var campaignWorkingPeriod = new CampaignWorkingPeriod
			{
				TimePeriod = new TimePeriod(new TimeSpan(8, 0, 0), new TimeSpan(9, 0, 0)),
				CampaignWorkingPeriodAssignments = new HashSet<CampaignWorkingPeriodAssignment>(),
			};
			var target = new OutboundCampaignPersister(_outboundCampaignRepository, _skillRepository, null, null, _userTimeZone);

			_outboundCampaignRepository.Stub(x => x.Get(form.CampaignId.Value)).IgnoreArguments().Return(_campaign);
			_userTimeZone.Stub(x => x.TimeZone()).Return(TimeZoneInfo.Utc);
			_campaign.Stub(x => x.AddWorkingPeriod(campaignWorkingPeriod)).IgnoreArguments();
			var result  = target.Persist(form);
			result.TimePeriod.Should().Be.EqualTo(campaignWorkingPeriod.TimePeriod);			
		}
	}
}
