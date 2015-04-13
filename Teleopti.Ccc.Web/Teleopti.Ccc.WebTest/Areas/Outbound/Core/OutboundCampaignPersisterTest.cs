using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
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
		private CampaignForm _campaignForm;
		
		[SetUp]
		public void Setup()
		{
			_outboundCampaignRepository = MockRepository.GenerateMock<IOutboundCampaignRepository>();
			_skillRepository = MockRepository.GenerateMock<ISkillRepository>();
			_outboundCampaignMapper = MockRepository.GenerateMock<IOutboundCampaignMapper>();
			_outboundCampaignViewModelMapper = MockRepository.GenerateMock<IOutboundCampaignViewModelMapper>();

			_campaignForm = new CampaignForm() {Name = "test"};
		}

		[Test]
		public void ShouldStoreNewCampaign()
		{
			var expectedVM = new CampaignViewModel();
			var skillType = SkillTypeFactory.CreateSkillType();
			var skillList = new List<ISkill> { SkillFactory.CreateSkill("sdfsdf", skillType, 15) };
			var target = new OutboundCampaignPersister(_outboundCampaignRepository, _skillRepository, null, _outboundCampaignViewModelMapper);
			_skillRepository.Stub(x => x.LoadAll()).Return(skillList);
			_outboundCampaignViewModelMapper.Stub(x => x.Map(new Domain.Outbound.Campaign())).IgnoreArguments().Return(expectedVM);
			_campaignForm.Id = null;

			var result = target.Persist(_campaignForm);

			_outboundCampaignRepository.AssertWasCalled(x => x.Add(null), o => o.IgnoreArguments());
			result.Should().Be.SameInstanceAs(expectedVM);
		}

		[Test]
		public void ShouldUpdateCampaign()
		{
			var expectedVM = new CampaignViewModel();
			var target = new OutboundCampaignPersister(_outboundCampaignRepository, null, _outboundCampaignMapper, _outboundCampaignViewModelMapper);
			_campaignForm.Id = new Guid();
			var campaign = new Domain.Outbound.Campaign();
			_outboundCampaignMapper.Stub(x => x.Map(_campaignForm)).Return(campaign);
			_outboundCampaignViewModelMapper.Stub(x => x.Map(campaign)).Return(expectedVM);

			var result = target.Persist(_campaignForm);

			result.Should().Be.SameInstanceAs(expectedVM);
		}
	}
}
