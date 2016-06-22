using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ViewModels
{
	[DomainTest]
	public class SkillViewModelBuilderTest
	{
		public FakeSkillRepository SkillRepository;
		public SkillViewModelBuilder Target;

		[Test]
		public void ShouldGetSkills()
		{
			var skill = SkillFactory.CreateSkill("Sales").WithId();
			SkillRepository.Has(skill);

			var result = Target.Build().Single();

			result.Id.Should().Be(skill.Id.ToString());
			result.Name.Should().Be("Sales");
		}
	}
}
