using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ViewModels
{
	[DomainTest]
	public class SkillViewModelBuilderTest : ISetup
	{
		public FakeSkillRepository SkillRepository;
		public SkillViewModelBuilder Target;
		public FakeUserUiCulture UiCulture;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble(new FakeUserUiCulture(CultureInfoFactory.CreateSwedishCulture())).For<IUserUiCulture>();
		}

		[Test]
		public void ShouldGetSkills()
		{
			var skill = SkillFactory.CreateSkill("Sales").WithId();
			SkillRepository.Has(skill);

			var result = Target.Build().Single();

			result.Id.Should().Be(skill.Id.ToString());
			result.Name.Should().Be("Sales");
		}

		[Test]
		public void ShouldSortByName()
		{
			var skill1 = SkillFactory.CreateSkill("C").WithId();
			var skill2 = SkillFactory.CreateSkill("A").WithId();
			var skill3 = SkillFactory.CreateSkill("B").WithId();
			SkillRepository.Has(skill1);
			SkillRepository.Has(skill2);
			SkillRepository.Has(skill3);

			var result = Target.Build();

			result.Select(x => x.Name).Should().Have.SameSequenceAs(new[] {"A", "B", "C"});
		}

		[Test]
		public void ShouldSortBySwedishName()
		{
			var skill1 = SkillFactory.CreateSkill("Ä").WithId();
			var skill2 = SkillFactory.CreateSkill("A").WithId();
			var skill3 = SkillFactory.CreateSkill("Å").WithId();
			SkillRepository.Has(skill1);
			SkillRepository.Has(skill2);
			SkillRepository.Has(skill3);
			UiCulture.IsSwedish();

			var result = Target.Build();

			result.Select(x => x.Name).Should().Have.SameSequenceAs(new[] {"A", "Å", "Ä"});
		}
	}
}
