using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.DayOffPlanning
{
	[TestFixture]
	public class SkillGroupIslandsAnalyzerTest
	{
		private SkillGroupIslandsAnalyzer _target;

		[SetUp]
		public void Setup()
		{
			_target = new SkillGroupIslandsAnalyzer();
		}

		[Test]
		public void ShouldFindIslands()
		{
			var s1 = SkillFactory.CreateSkill("s1");
			s1.SetId(Guid.Parse("00000080-4720-4490-0020-6b79c2e92474"));
			var s2 = SkillFactory.CreateSkill("s2");
			s2.SetId(Guid.Parse("10000080-4720-4490-0020-6b79c2e92474"));
			var s3 = SkillFactory.CreateSkill("s3");
			s3.SetId(Guid.Parse("20000080-4720-4490-0020-6b79c2e92474"));

			var p1 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> {s1, s2});
			var p2 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> { s2, s1 });
			var p3 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> { s3 });

			var personList = new List<IPerson>{p1, p2, p3};
			var vitualSkillGroupsCreator = new VirtualSkillGroupsCreator();
			var skillGroups = vitualSkillGroupsCreator.GroupOnDate(DateOnly.MinValue, personList);

			var result = _target.FindIslands(skillGroups);
			result.Count.Should().Be.EqualTo(2);

			result[0].SkillGuidStrings.Should().Contain(s1.Id.ToString());
			result[0].SkillGuidStrings.Should().Contain(s2.Id.ToString());
			result[0].SkillGuidStrings.Count.Should().Be.EqualTo(2);

			result[1].SkillGuidStrings.Should().Contain(s3.Id.ToString());
			result[1].SkillGuidStrings.Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldAddGroupsToIsland()
		{
			var s1 = SkillFactory.CreateSkill("s1");
			s1.SetId(Guid.Parse("00000080-4720-4490-0020-6b79c2e92474"));
			var s2 = SkillFactory.CreateSkill("s2");
			s2.SetId(Guid.Parse("10000080-4720-4490-0020-6b79c2e92474"));
			var s3 = SkillFactory.CreateSkill("s3");
			s3.SetId(Guid.Parse("20000080-4720-4490-0020-6b79c2e92474"));

			var p1 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> { s1, s2 });
			var p2 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> { s2 });
			var p3 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> { s3 });

			var personList = new List<IPerson> { p1, p2, p3 };
			var vitualSkillGroupsCreator = new VirtualSkillGroupsCreator();
			var skillGroups = vitualSkillGroupsCreator.GroupOnDate(DateOnly.MinValue, personList);

			var result = _target.FindIslands(skillGroups);

			result[1].SkillGuidStrings.Should().Contain(s1.Id.ToString());
			result[1].SkillGuidStrings.Should().Contain(s2.Id.ToString());
			result[1].SkillGuidStrings.Count.Should().Be.EqualTo(2);
			result[1].GroupKeys.Count.Should().Be.EqualTo(2);

			result[0].SkillGuidStrings.Should().Contain(s3.Id.ToString());
			result[0].GroupKeys.Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldJoinIslands()
		{
			var s1 = SkillFactory.CreateSkill("s1");
			s1.SetId(Guid.Parse("00000080-4720-4490-0020-6b79c2e92474"));
			var s2 = SkillFactory.CreateSkill("s2");
			s2.SetId(Guid.Parse("10000080-4720-4490-0020-6b79c2e92474"));
			var s3 = SkillFactory.CreateSkill("s3");
			s3.SetId(Guid.Parse("20000080-4720-4490-0020-6b79c2e92474"));

			var p1 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> { s1, s2 });
			var p2 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> { s3 });
			var p3 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> { s2, s3 });

			var personList = new List<IPerson> { p1, p2, p3 };
			var vitualSkillGroupsCreator = new VirtualSkillGroupsCreator();
			var skillGroups = vitualSkillGroupsCreator.GroupOnDate(DateOnly.MinValue, personList);

			var result = _target.FindIslands(skillGroups);

			result.Count.Should().Be.EqualTo(1);
			result[0].SkillGuidStrings.Count.Should().Be.EqualTo(3);
			result[0].GroupKeys.Count.Should().Be.EqualTo(3);
		}

		[Test]
		public void ShouldJoinIslands2()
		{
			var s1 = SkillFactory.CreateSkill("s1");
			s1.SetId(Guid.Parse("00000080-4720-4490-0020-6b79c2e92474"));
			var s2 = SkillFactory.CreateSkill("s2");
			s2.SetId(Guid.Parse("10000080-4720-4490-0020-6b79c2e92474"));
			var s3 = SkillFactory.CreateSkill("s3");
			s3.SetId(Guid.Parse("20000080-4720-4490-0020-6b79c2e92474"));
			var s4 = SkillFactory.CreateSkill("s4");
			s4.SetId(Guid.Parse("30000080-4720-4490-0020-6b79c2e92474"));

			var p1 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> { s1, s2 });
			var p2 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> { s3 });
			var p3 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> { s2, s4 });

			var personList = new List<IPerson> { p1, p2, p3 };
			var vitualSkillGroupsCreator = new VirtualSkillGroupsCreator();
			var skillGroups = vitualSkillGroupsCreator.GroupOnDate(DateOnly.MinValue, personList);

			var result = _target.FindIslands(skillGroups);

			result.Count.Should().Be.EqualTo(2);
			result[0].SkillGuidStrings.Count.Should().Be.EqualTo(1);
			result[0].GroupKeys.Count.Should().Be.EqualTo(1);

			result[1].SkillGuidStrings.Count.Should().Be.EqualTo(3);
			result[1].GroupKeys.Count.Should().Be.EqualTo(2);
		}
	}
}