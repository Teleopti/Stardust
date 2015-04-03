using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Outbound.Core
{
	[TestFixture]
	class OutboundCampaignPersisterTest
	{
		private IOutboundCampaignRepository _outboundCampaignRepository;
		private ISkillRepository _skillRepository;
		private IList<ISkill> _skillList;
		
		[SetUp]
		public void Setup()
		{
			_outboundCampaignRepository = MockRepository.GenerateMock<IOutboundCampaignRepository>();
			_skillRepository = MockRepository.GenerateMock<ISkillRepository>();
			var skillType = SkillTypeFactory.CreateSkillType();
			_skillList = new List<ISkill> { SkillFactory.CreateSkill("sdfsdf", skillType, 15) };
		}

		[Test]
		public void ShouldStoreCampaign()
		{
			var target = new OutboundCampaignPersister(_outboundCampaignRepository, _skillRepository);
			_skillRepository.Stub(x => x.LoadAll()).Return(_skillList);
			
			target.Persist("test");

			_outboundCampaignRepository.AssertWasCalled(x => x.Add(null), o => o.IgnoreArguments());
		}

		[Test]
		public void ShouldGetCampaignViewModel()
		{
			const string expectedName = "myCampaign";
			var target = new OutboundCampaignPersister(_outboundCampaignRepository, _skillRepository);
			_skillRepository.Stub(x => x.LoadAll()).Return(_skillList);

			var campaignVM = target.Persist(expectedName);

			campaignVM.Name.Should().Be.EqualTo(expectedName);
		}
	}
}
