using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
	[TestFixture]
	public class GroupPersonSkillAggregatorTest
	{
		private IGroupPersonSkillAggregator _target;
		private IPerson _person1;
		private ISkill _skill1;
		private ISkill _skill2;
		private ISkill _skill3;
		private IPerson _person2;

		[SetUp]
		public void Setup()
		{
			_target = new GroupPersonSkillAggregator(new PersonalSkillsProvider());
			_skill1 = SkillFactory.CreateSkill("1");
			_skill2 = SkillFactory.CreateSkill("2");
			_skill3 = SkillFactory.CreateSkill("3");
			_person1 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue,
			                                                      new List<ISkill> {_skill1, _skill2});
			_person2 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue,
																  new List<ISkill> { _skill2, _skill3 });
		}

		[Test]
		public void ShouldReturnUnionOfAllSkills()
		{
			IList<ISkill> result = new List<ISkill>(_target.AggregatedSkills(new List<IPerson> { _person1, _person2 }, new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue)));
			Assert.IsTrue(result.Contains(_skill1));
			Assert.IsTrue(result.Contains(_skill2));
			Assert.IsTrue(result.Contains(_skill3));
			Assert.AreEqual(3, result.Count);
		}

		[Test]
		public void ShouldDisregardInactiveSkills()
		{
			var personalSkill = (IPersonSkillModify)_person1.Period(DateOnly.MinValue).PersonSkillCollection.First();
			personalSkill.Active = false;
			IList<ISkill> result = new List<ISkill>(_target.AggregatedSkills(new List<IPerson> { _person1 }, new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue)));
			Assert.IsTrue(result.Contains(_skill2));
			Assert.AreEqual(1, result.Count);
		}

		[Test]
		public void ShouldDisregardDeletedSkills()
		{
			var personalSkill = (IPersonSkillModify)_person1.Period(DateOnly.MinValue).PersonSkillCollection.First();
			personalSkill.Active = true;
			var skill = _person1.Period(DateOnly.MinValue).PersonSkillCollection.First().Skill;
			((IDeleteTag)skill).SetDeleted();
			IList<ISkill> result = new List<ISkill>(_target.AggregatedSkills(new List<IPerson> { _person1 }, new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue)));
			Assert.IsTrue(result.Contains(_skill2));
			Assert.AreEqual(1, result.Count);
		}
	}
}