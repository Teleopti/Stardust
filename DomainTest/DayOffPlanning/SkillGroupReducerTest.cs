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
	public class SkillGroupReducerTest
	{
		private SkillGroupReducer _target;
		private ISkill _s1;
		private ISkill _s2;
		private ISkill _s3;
		private ISkill _s4;

		[SetUp]
		public void Setup()
		{
			_target = new SkillGroupReducer();
			_s1 = SkillFactory.CreateSkill("s1");
			_s1.SetId(Guid.Parse("00000080-4720-4490-0020-6b79c2e92474"));
			_s2 = SkillFactory.CreateSkill("s2");
			_s2.SetId(Guid.Parse("10000080-4720-4490-0020-6b79c2e92474"));
			_s3 = SkillFactory.CreateSkill("s3");
			_s3.SetId(Guid.Parse("20000080-4720-4490-0020-6b79c2e92474"));
			_s4 = SkillFactory.CreateSkill("s4");
			_s4.SetId(Guid.Parse("30000080-4720-4490-0020-6b79c2e92474"));
		}

		[Test]
		public void ShouldReturnSuggestedIfFound()
		{
			var p1 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> { _s1, _s2 }); //G1
			var p2 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> { _s2, _s1 }); //G1
			var p3 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> { _s2, _s3 }); //G2
			var p4 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> { _s3 }); //G3

			var personList = new List<IPerson> { p1, p2, p3, p4 };
			var vitualSkillGroupsCreator = new VirtualSkillGroupsCreator();
			var skillGroups = vitualSkillGroupsCreator.GroupOnDate(DateOnly.MinValue, personList);
			var skillGroupIslandsAnalyzer = new SkillGroupIslandsAnalyzer();
			var islandList = skillGroupIslandsAnalyzer.FindIslands(skillGroups);

			var result = _target.SuggestAction(skillGroups, islandList);

			result[0].RemoveFromGroupKey.Should().Be.EqualTo(_s2.Id + "|" + _s3.Id);
			result[0].SkillGuidStringToRemove.Should().Be.EqualTo(_s2.Id.ToString());
		}

		[Test]
		public void ShouldReturnEmptyListIfNotFound()
		{
			var p1 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> { _s1, _s2 }); //G1
			var p2 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> { _s2, _s1 }); //G1
			var p3 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> { _s4, _s3 }); //G2
			var p4 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> { _s3 }); //G3

			var personList = new List<IPerson> { p1, p2, p3, p4 };
			var vitualSkillGroupsCreator = new VirtualSkillGroupsCreator();
			var skillGroups = vitualSkillGroupsCreator.GroupOnDate(DateOnly.MinValue, personList);

			var skillGroupIslandsAnalyzer = new SkillGroupIslandsAnalyzer();
			var islandList = skillGroupIslandsAnalyzer.FindIslands(skillGroups);

			var result = _target.SuggestAction(skillGroups, islandList);

			result.Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldSkipSingleSkillGroups()
		{
			var p1 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> { _s1, _s2 }); //G1
			var p2 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> { _s2, _s1 }); //G1
			var p3 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> { _s2 }); //G2
			var p4 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> { _s3 }); //G3

			var personList = new List<IPerson> { p1, p2, p3, p4 };
			var vitualSkillGroupsCreator = new VirtualSkillGroupsCreator();
			var skillGroups = vitualSkillGroupsCreator.GroupOnDate(DateOnly.MinValue, personList);

			var skillGroupIslandsAnalyzer = new SkillGroupIslandsAnalyzer();
			var islandList = skillGroupIslandsAnalyzer.FindIslands(skillGroups);

			var result = _target.SuggestAction(skillGroups, islandList);

			result.Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldReturnMultipleResults()
		{
			var p1 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> { _s1, _s2 }); //G1
			var p2 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> { _s2, _s1 }); //G1
			var p3 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> { _s2, _s3 }); //G2
			var p4 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> { _s2, _s4 }); //G3

			var personList = new List<IPerson> { p1, p2, p3, p4 };
			var vitualSkillGroupsCreator = new VirtualSkillGroupsCreator();
			var skillGroups = vitualSkillGroupsCreator.GroupOnDate(DateOnly.MinValue, personList);

			var skillGroupIslandsAnalyzer = new SkillGroupIslandsAnalyzer();
			var islandList = skillGroupIslandsAnalyzer.FindIslands(skillGroups);

			var result = _target.SuggestAction(skillGroups, islandList);

			result.Count.Should().Be.EqualTo(2);
			result[0].SkillGuidStringToRemove.Should().Be.EqualTo(_s2.Id.ToString());
			result[1].SkillGuidStringToRemove.Should().Be.EqualTo(_s2.Id.ToString());
		}
	}
}