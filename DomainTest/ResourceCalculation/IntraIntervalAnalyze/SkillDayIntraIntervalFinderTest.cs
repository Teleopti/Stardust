using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation.IntraIntervalAnalyze;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;


namespace Teleopti.Ccc.DomainTest.ResourceCalculation.IntraIntervalAnalyze
{
	[TestFixture]
	public class SkillDayIntraIntervalFinderTest
	{
		private SkillDayIntraIntervalFinder _target;
		private MockRepository _mock;
		private IIntraIntervalFinder _intraIntervalFinder;
		private ISkillDay _skillDay;
		private IResourceCalculationDataContainer _resourceCalculationDataContainer;
		private ISkillStaffPeriod _skillStaffPeriod1;
		private ISkillStaffPeriod _skillStaffPeriod2;
		private ISkill _skill;

		[SetUp]
		public void SetUp()
		{
			_mock = new MockRepository();
			_skillDay = _mock.StrictMock<ISkillDay>();
			_resourceCalculationDataContainer = _mock.StrictMock<IResourceCalculationDataContainer>();
			_intraIntervalFinder = _mock.StrictMock<IIntraIntervalFinder>();
			_target = new SkillDayIntraIntervalFinder(_intraIntervalFinder);
			_skillStaffPeriod1 = SkillStaffPeriodFactory.CreateSkillStaffPeriod();
			_skillStaffPeriod2 = SkillStaffPeriodFactory.CreateSkillStaffPeriod();
			_skill = SkillFactory.CreateSkill("skill");
		}

		[Test]
		public void ShouldSetIntraIntervalIssues()
		{
			using (_mock.Record())
			{
				Expect.Call(_skillDay.SkillStaffPeriodCollection)
					.Return(
						new ReadOnlyCollection<ISkillStaffPeriod>(new List<ISkillStaffPeriod> {_skillStaffPeriod1, _skillStaffPeriod2}));
				Expect.Call(_skillDay.Skill).Return(_skill);
				Expect.Call(_intraIntervalFinder.FindForInterval(_skillStaffPeriod1.Period, _resourceCalculationDataContainer, _skill)).Return(true);
				Expect.Call(_intraIntervalFinder.FindForInterval(_skillStaffPeriod2.Period, _resourceCalculationDataContainer, _skill)).Return(false);
			}

			using (_mock.Playback())
			{
				_target.SetIntraIntervalIssues(_skillDay,_resourceCalculationDataContainer);
				Assert.IsTrue(_skillStaffPeriod1.HasIntraIntervalIssue);
				Assert.IsFalse(_skillStaffPeriod2.HasIntraIntervalIssue);
			}
		}
	}	
}
