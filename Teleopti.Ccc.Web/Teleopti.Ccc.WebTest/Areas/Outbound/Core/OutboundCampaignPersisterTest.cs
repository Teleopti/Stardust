﻿using System;
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
		private ISkill _skill;
		
		[SetUp]
		public void Setup()
		{
			_outboundCampaignRepository = MockRepository.GenerateMock<IOutboundCampaignRepository>();
			_skillRepository = MockRepository.GenerateMock<ISkillRepository>();
			_outboundCampaignMapper = MockRepository.GenerateMock<IOutboundCampaignMapper>();
			_outboundCampaignViewModelMapper = MockRepository.GenerateMock<IOutboundCampaignViewModelMapper>();

			var skillType = SkillTypeFactory.CreateSkillType();
			_skill = SkillFactory.CreateSkill("sdfsdf", skillType, 15);
			var skillList = new List<ISkill> { _skill };
			_skillRepository.Stub(x => x.LoadAll()).Return(skillList);
		}

		[Test]
		public void ShouldStoreNewCampaign()
		{
			var expectedVM = new CampaignViewModel();
			var target = new OutboundCampaignPersister(_outboundCampaignRepository, _skillRepository, null, _outboundCampaignViewModelMapper);
			_outboundCampaignViewModelMapper.Stub(x => x.Map(new Domain.Outbound.Campaign())).IgnoreArguments().Return(expectedVM);

			var result = target.Persist("test");

			_outboundCampaignRepository.AssertWasCalled(x => x.Add(null), o => o.IgnoreArguments());
			result.Should().Be.SameInstanceAs(expectedVM);
		}

		[Test]
		public void ShouldUpdateCampaign()
		{
			var campaignVM = new CampaignViewModel { Id = new Guid(), Skills = new List<SkillViewModel> { new SkillViewModel() { Id = _skill.Id, IsSelected = true} } };
			var target = new OutboundCampaignPersister(_outboundCampaignRepository, _skillRepository, _outboundCampaignMapper, null);
			var expectedCampaign = new Domain.Outbound.Campaign();
			_outboundCampaignMapper.Stub(x => x.Map(campaignVM)).Return(expectedCampaign);

			var result = target.Persist(campaignVM);

			result.Should().Be.SameInstanceAs(expectedCampaign);
		}
	}
}
