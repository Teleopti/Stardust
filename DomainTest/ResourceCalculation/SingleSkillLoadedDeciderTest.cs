using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
	[TestFixture]
	public class SingleSkillLoadedDeciderTest
	{
		private ISingleSkillLoadedDecider _target;

		[SetUp]
		public void Setup()
		{
			_target = new SingleSkillLoadedDecider();
		}

		[Test]
		public void ShouldCheckThatOnlyOneSkillIsLoaded()
		{
			IList<ISkill> loadedSkills = new List<ISkill>();

			bool result = _target.IsSingleSkill(loadedSkills);
			Assert.IsFalse(result);

			ISkill skill = SkillFactory.CreateSkill("hej");
			loadedSkills.Add(skill);
			Assert.IsTrue(_target.IsSingleSkill(loadedSkills));

			skill = SkillFactory.CreateSkill("hopp");
			loadedSkills.Add(skill);
			Assert.IsFalse(_target.IsSingleSkill(loadedSkills));
		}

		[Test]
		public void OnlySkillLoadedMustBePhoneSkill()
		{
			IList<ISkill> loadedSkills = new List<ISkill>();
			ISkillTypePhone skillTypePhone = new SkillTypePhone(new Description(), new ForecastSource());
			ISkill skill = SkillFactory.CreateSkill("hej", skillTypePhone, 15);
			loadedSkills.Add(skill);
			Assert.IsTrue(_target.IsSingleSkill(loadedSkills));

			loadedSkills.Clear();
			ISkillTypeEmail skillTypeEmail = new SkillTypeEmail(new Description(), new ForecastSource());
			skill = SkillFactory.CreateSkill("hopp", skillTypeEmail, 15);

			loadedSkills.Add(skill);
			Assert.IsFalse(_target.IsSingleSkill(loadedSkills));
		}
	}
}