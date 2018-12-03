using System;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
	[TestFixture]
	public class PersonSkillPeriodAssemblerTest
	{
		[Test]
		public void VerifyDomainEntityToDto()
		{
			var skill = SkillFactory.CreateSkill("test").WithId();

			var person = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2001, 1, 1), new[] { skill }).WithId();
			var period = person.Period(new DateOnly(2001, 1, 1)).WithId();
			period.Team = TeamFactory.CreateSimpleTeam();

			var target = new PersonSkillPeriodAssembler();
			var personDto = target.DomainEntityToDto(period);
			Assert.AreEqual(period.Id, personDto.Id);
			Assert.AreEqual(skill.Id, personDto.SkillCollection[0]);
			Assert.AreEqual(1, personDto.PersonSkillCollection.First().Proficiency);
			Assert.AreEqual(true, personDto.PersonSkillCollection.First().Active);
		}

		[Test]
		public void VerifyDtoToDomainEntity()
		{
			var target = new PersonSkillPeriodAssembler();
			Assert.Throws<NotSupportedException>(() => target.DtoToDomainEntity(new PersonSkillPeriodDto()));
		}

		[Test]
		public void VerifyDomainEntityToDtoWithProficencyAndActive()
		{
			var skill = SkillFactory.CreateSkill("test").WithId();

			var person = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2001, 1, 1), new[] { skill }).WithId();
			var period = person.Period(new DateOnly(2001, 1, 1)).WithId();
			period.Team = TeamFactory.CreateSimpleTeam();

			person.DeactivateSkill(skill, period);
			person.ChangeSkillProficiency(skill, new Percent(0.9), period);

			var target = new PersonSkillPeriodAssembler();
			var personDto = target.DomainEntityToDto(period);
			Assert.AreEqual(period.PersonSkillCollection.First().SkillPercentage.Value, personDto.PersonSkillCollection.First().Proficiency);
			Assert.AreEqual(period.PersonSkillCollection.First().Active, personDto.PersonSkillCollection.First().Active);
		}
	}
}
