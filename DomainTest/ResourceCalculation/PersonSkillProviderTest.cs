using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;


namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
	[TestFixture]
	public class PersonSkillProviderTest
	{
		private IPersonSkillProvider _target;

		[SetUp]
		public void Setup()
		{
			_target = new PersonSkillProvider();
		}

		[Test]
		public void ShouldNotIncludePersonSkillThatIsBasedOnDeletedSkill()
		{
			var skill = SkillFactory.CreateSkill("OKSkill");
			var deletedSkill = SkillFactory.CreateSkill("Deleted");
			((IDeleteTag)deletedSkill).SetDeleted();
			var personWithPersonalSkills = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue,
			                                                                          new[] {skill, deletedSkill});
			var result = _target.SkillsOnPersonDate(personWithPersonalSkills, DateOnly.MinValue);
			Assert.IsFalse(result.Skills.ToList().Contains(deletedSkill));
		}

	}
}