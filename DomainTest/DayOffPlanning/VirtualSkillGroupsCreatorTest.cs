using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DomainTest.DayOffPlanning
{
	[TestFixture]
	public class VirtualSkillGroupsCreatorTest
	{
		private VirtualSkillGroupsCreator _target;

		[SetUp]
		public void Seup()
		{
			_target = new VirtualSkillGroupsCreator();
		}

		[Test]
		public void ShouldGroupForSpecificDate()
		{
			var s1 = SkillFactory.CreateSkill("s1");
			s1.SetId(Guid.Parse("00000080-4720-4490-0020-6b79c2e92474"));
			var s2 = SkillFactory.CreateSkill("s2");
			s2.SetId(Guid.Parse("10000080-4720-4490-0020-6b79c2e92474"));
			var s3 = SkillFactory.CreateSkill("s3");
			s3.SetId(Guid.Parse("20000080-4720-4490-0020-6b79c2e92474"));

			var p1 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> {s1, s2});
			var p2 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> { s2, s1 });
			var p3 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> { s2, s3 });
			var p4 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MaxValue, new List<ISkill> { s2, s3 });

			var personList = new List<IPerson>{p1, p2, p3, p4};
			var result = _target.GroupOnDate(DateOnly.MinValue, personList);

			result.GetKeys().Should().Contain(s1.Id + "|" + s2.Id);
			result.GetKeys().Should().Contain(s2.Id + "|" + s3.Id);
			result.GetPersonsForKey(s1.Id + "|" + s2.Id).Should().Contain(p1).And.Contain(p2);
			result.GetPersonsForKey(s2.Id + "|" + s3.Id).Should().Contain(p3).And.Not.Contain(p4);
		}

		[Test]
		public void ShouldSkipInactivePersonSkills()
		{
			var s1 = SkillFactory.CreateSkill("s1");
			s1.SetId(Guid.Parse("00000080-4720-4490-0020-6b79c2e92474"));
			var p1 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> { s1 });
			((IPersonSkillModify)p1.Period(DateOnly.MinValue).PersonSkillCollection.First()).Active = false;

			var personList = new List<IPerson> { p1 };
			var result = _target.GroupOnDate(DateOnly.MinValue, personList);

			result.GetKeys().Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldSkipDeletedSkills()
		{
			var s1 = SkillFactory.CreateSkill("s1");
			s1.SetId(Guid.Parse("00000080-4720-4490-0020-6b79c2e92474"));
			var p1 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> { s1 });
			((IDeleteTag)p1.Period(DateOnly.MinValue).PersonSkillCollection.First().Skill).SetDeleted();

			var personList = new List<IPerson> { p1 };
			var result = _target.GroupOnDate(DateOnly.MinValue, personList);

			result.GetKeys().Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldReturnTotalNumberOfAgentsForGivenSkill()
		{
			var s1 = SkillFactory.CreateSkill("s1");
			s1.SetId(Guid.Parse("00000080-4720-4490-0020-6b79c2e92474"));
			var s2 = SkillFactory.CreateSkill("s2");
			s2.SetId(Guid.Parse("10000080-4720-4490-0020-6b79c2e92474"));
			var s3 = SkillFactory.CreateSkill("s3");
			s3.SetId(Guid.Parse("20000080-4720-4490-0020-6b79c2e92474"));

			var p1 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> { s1, s2 });
			var p2 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> { s2, s1 });
			var p3 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> { s2, s3 });

			var personList = new List<IPerson> { p1, p2, p3 };
			var result = _target.GroupOnDate(DateOnly.MinValue, personList);

			result.GetPersonsForSkillKey("10000080-4720-4490-0020-6b79c2e92474").Count().Should().Be.EqualTo(3);
			result.GetPersonsForSkillKey("20000080-4720-4490-0020-6b79c2e92474").Count().Should().Be.EqualTo(1);
		}
	}
}