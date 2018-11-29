using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Outbound;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;


namespace Teleopti.Ccc.DomainTest.Outbound
{
	[TestFixture]
	public class OutboundAssignedStaffProviderTest
	{
		private OutboundAssignedStaffProvider _target;
		private IPersonRepository _personRepository;
		private ISkill _campaignSkill;
		private ISkill _skill;
        private IOutboundCampaign _campaign;

		[SetUp]
		public void Setup()
		{
			_personRepository = new FakePersonRepositoryLegacy();
			_target = new OutboundAssignedStaffProvider(_personRepository);
			_campaignSkill = SkillFactory.CreateSkill("skill1");
			_skill = SkillFactory.CreateSkill("skill2");
			_campaign = new Campaign {Skill = _campaignSkill};
		}

		[Test]
		public void ShouldSelectPersonThatHaveSkillConnectedToCampaign()
		{
			var person1 = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(), new List<ISkill> { _skill });
			var person2 = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(), new List<ISkill> { _campaignSkill });
			_personRepository.Add(person1);
			_personRepository.Add(person2);

            var result = _target.Load(new List<IOutboundCampaign> { _campaign }, new DateOnlyPeriod());

			result.AllPeople.Should().Equals(new List<IPerson> {person1, person2});
			result.FixedStaffPeople.Should().Equals(new List<IPerson> { person2 });
		}

		[Test]
		public void ShouldNotSelectPersonThatHaveSkillOnlyOutsideOfPeriod()
		{
			var person1 = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(), new List<ISkill> { _campaignSkill });
			var person2 = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2015,7,3), new List<ISkill> { _campaignSkill });
			
			_personRepository.Add(person1);
			_personRepository.Add(person2);

            var result = _target.Load(new List<IOutboundCampaign> { _campaign }, new DateOnlyPeriod());

			result.AllPeople.Should().Equals(new List<IPerson> { person1, person2 });
			result.FixedStaffPeople.Should().Equals(new List<IPerson> { person1 });
		}

	}
}