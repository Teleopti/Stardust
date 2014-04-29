using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
    [TestFixture]
    public class PersonSkillPeriodAssemblerTest
    {
        [Test]
        public void VerifyDomainEntityToDto()
        {
            var target = new PersonSkillPeriodAssembler();
            var person = PersonFactory.CreatePersonWithGuid("aaa", "bbb");

            var period = MockRepository.GenerateMock<IPersonPeriod>();

            var skill = SkillFactory.CreateSkill("test");
            skill.SetId(Guid.NewGuid());

            period.Stub(x => x.Id).Return(Guid.NewGuid());
            period.Stub(x => x.Parent).Return(person);
            period.Stub(x => x.Team).Return(TeamFactory.CreateSimpleTeam());

            period.Stub(x => x.PersonSkillCollection)
                .Return(new List<IPersonSkill> {PersonSkillFactory.CreatePersonSkill(skill, 1)});
            person.AddPersonPeriod(period);
            
            var personDto = target.DomainEntityToDto(period);
            Assert.AreEqual(period.Id, personDto.Id);
            Assert.AreEqual(skill.Id, personDto.SkillCollection[0]);
            Assert.AreEqual(1, personDto.PersonSkillCollection.First().Proficiency);
            Assert.AreEqual(true, personDto.PersonSkillCollection.First().Active);
        }

        [Test, ExpectedException(typeof(NotSupportedException))]
        public void VerifyDtoToDomainEntity()
        {
            var target = new PersonSkillPeriodAssembler();
            target.DtoToDomainEntity(new PersonSkillPeriodDto());
        }

        [Test]
        public void VerifyDomainEntityToDtoWithProficencyAndActive()
        {
            var target = new PersonSkillPeriodAssembler();
            var person = PersonFactory.CreatePersonWithGuid("aaa", "bbb");

            var period = MockRepository.GenerateMock<IPersonPeriod>();

            var skill = SkillFactory.CreateSkill("test");
            skill.SetId(Guid.NewGuid());

            period.Stub(x => x.Id).Return(Guid.NewGuid());
            period.Stub(x => x.Parent).Return(person);
            period.Stub(x => x.Team).Return(TeamFactory.CreateSimpleTeam());

            var personSkill = PersonSkillFactory.CreatePersonSkill(skill, 0.9);
            period.Stub(x => x.PersonSkillCollection)
                .Return(new List<IPersonSkill> { personSkill });
            
            person.AddPersonPeriod(period);
            person.DeactivateSkill(skill, period);

            var personDto = target.DomainEntityToDto(period);
            Assert.AreEqual(personSkill.SkillPercentage.Value, personDto.PersonSkillCollection.First().Proficiency);
            Assert.AreEqual(personSkill.Active, personDto.PersonSkillCollection.First().Active);
        }
    }
}
