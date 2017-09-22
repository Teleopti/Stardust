using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
    [TestFixture]
    public class SkillAssemblerTest
    {
	    [Test]
	    public void VerifyDomainEntityToDto()
	    {
		    var skillDomain = SkillFactory.CreateSkill("test").WithId();

		    var skillRep = new FakeSkillRepository();
		    skillRep.Add(skillDomain);
			
		    var activityAssembler = new ActivityAssembler(new FakeActivityRepository());
		    var target = new SkillAssembler(skillRep, activityAssembler);

		    SkillDto skillDto = target.DomainEntityToDto(skillDomain);

		    Assert.AreEqual(skillDomain.Id, skillDto.Id);
		    Assert.AreEqual(skillDomain.Name, skillDto.Name);
		    Assert.AreEqual(skillDomain.Description, skillDto.Description);
		    Assert.AreEqual(skillDomain.DefaultResolution, skillDto.Resolution);
		    Assert.AreEqual(skillDomain.SkillType.Description.Name, skillDto.SkillType);
		    Assert.AreEqual(skillDomain.DisplayColor.ToArgb(), skillDto.DisplayColor.ToColor().ToArgb());
		    Assert.IsNotNull(skillDto.Activity);
	    }

		[Test]
		public void ShouldIncludeIdOfWorkloadInDto()
		{
			var skillDomain = SkillFactory.CreateSkillWithWorkloadAndSources().WithId();
			skillDomain.WorkloadCollection.ForEach(w => w.WithId());

			var skillRep = new FakeSkillRepository();
			skillRep.Add(skillDomain);
			
			var activityAssembler = new ActivityAssembler(new FakeActivityRepository());
			var target = new SkillAssembler(skillRep, activityAssembler);

			SkillDto skillDto = target.DomainEntityToDto(skillDomain);
			skillDto.WorkloadIdCollection.First().Should().Be.EqualTo(skillDomain.WorkloadCollection.First().Id.GetValueOrDefault());
		}

		[Test]
	    public void VerifyDtoToDomainEntity()
	    {
		    var skill = SkillFactory.CreateSkill("test").WithId();

		    var skillRep = new FakeSkillRepository();
		    skillRep.Add(skill);

		    var activityRepository = new FakeActivityRepository();
		    var activity = ActivityFactory.CreateActivity("Phone").WithId();
		    activityRepository.Add(activity);
		    var activityAssembler = new ActivityAssembler(activityRepository);
		    var target = new SkillAssembler(skillRep, activityAssembler);

		    ISkill skillDomain = target.DtoToDomainEntity(new SkillDto {Id = skill.Id.GetValueOrDefault()});
		    Assert.IsNotNull(skillDomain);
	    }
    }
}
