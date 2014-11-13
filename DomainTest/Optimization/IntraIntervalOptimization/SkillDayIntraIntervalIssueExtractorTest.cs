using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization.IntraIntervalOptimization;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.IntraIntervalOptimization
{
	[TestFixture]
	public class SkillDayIntraIntervalIssueExtractorTest
	{
		private MockRepository _mock;
		private SkillDayIntraIntervalIssueExtractor _target;
		private ISkillDay _skillDay1;
		private ISkillDay _skillDay2;
		private ISkill _skill1;
		private ISkill _skill2;
		private ISkillStaffPeriod _skillStaffPeriod1;
		private ISkillStaffPeriod _skillStaffPeriod2;
		private IList<ISkillDay> _skillDays;
		
		[SetUp]
		public void SetUp()
		{
			_mock = new MockRepository();
			_skillDay1 = _mock.StrictMock<ISkillDay>();
			_skillDay2 = _mock.StrictMock<ISkillDay>();
			_skillDays = new List<ISkillDay>{_skillDay1, _skillDay2};
			_skill1 = SkillFactory.CreateSkill("skill1");
			_skill2 = SkillFactory.CreateSkill("skill2");
			_skillStaffPeriod1 = _mock.StrictMock<ISkillStaffPeriod>();
			_skillStaffPeriod2 = _mock.StrictMock<ISkillStaffPeriod>();
			_target = new SkillDayIntraIntervalIssueExtractor();
		}

		[Test]
		public void ShouldExtractOnIssues()
		{
			using (_mock.Record())
			{
				Expect.Call(_skillDay1.Skill).Return(_skill1);
				Expect.Call(_skillDay2.Skill).Return(_skill2);
				Expect.Call(_skillDay2.SkillStaffPeriodCollection).Return(new ReadOnlyCollection<ISkillStaffPeriod>(new List<ISkillStaffPeriod> {_skillStaffPeriod1, _skillStaffPeriod2}));
				Expect.Call(_skillStaffPeriod1.HasIntraIntervalIssue).Return(false);
				Expect.Call(_skillStaffPeriod2.HasIntraIntervalIssue).Return(true);
				Expect.Call(_skillStaffPeriod2.NoneEntityClone()).Return(_skillStaffPeriod2);
			}

			using (_mock.Playback())
			{
				var result = _target.ExtractOnIssues(_skillDays, _skill2);
				Assert.AreEqual(1, result.Count);
				Assert.AreEqual(_skillStaffPeriod2, result[0]);
			}
		}

		[Test]
		public void ShouldExtractAll()
		{
			using (_mock.Record())
			{
				Expect.Call(_skillDay1.Skill).Return(_skill1);
				Expect.Call(_skillDay2.Skill).Return(_skill2);
				Expect.Call(_skillDay2.SkillStaffPeriodCollection).Return(new ReadOnlyCollection<ISkillStaffPeriod>(new List<ISkillStaffPeriod> { _skillStaffPeriod1, _skillStaffPeriod2 }));
				Expect.Call(_skillStaffPeriod1.NoneEntityClone()).Return(_skillStaffPeriod1);
				Expect.Call(_skillStaffPeriod2.NoneEntityClone()).Return(_skillStaffPeriod2);
			}

			using (_mock.Playback())
			{
				var result = _target.ExtractAll(_skillDays, _skill2);
				Assert.AreEqual(2, result.Count);
			}	
		}
	}
}
