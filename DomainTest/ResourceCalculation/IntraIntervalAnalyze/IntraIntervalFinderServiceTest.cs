using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation.IntraIntervalAnalyze;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation.IntraIntervalAnalyze
{
	[TestFixture]
	public class IntraIntervalFinderServiceTest
	{
		private MockRepository _mock;
		private IntraIntervalFinderService _target;
		private ISkillDayIntraIntervalFinder _skillDayIntraIntervalFinder;
		private ISchedulingResultStateHolder _schedulingResultStateHolder;
		private IResourceCalculationDataContainer _resourceCalculationDataContainer;
		private ISkillDay _skillDay;
		private ISkill _maxSeatSkill;
		private ISkill _normalSkill;

		[SetUp]
		public void SetUp()
		{
			_mock = new MockRepository();
			_schedulingResultStateHolder = _mock.StrictMock<ISchedulingResultStateHolder>();
			_skillDayIntraIntervalFinder = _mock.StrictMock<ISkillDayIntraIntervalFinder>();
			_resourceCalculationDataContainer = _mock.StrictMock<IResourceCalculationDataContainer>();
			_skillDay = _mock.StrictMock<ISkillDay>();
			_target = new IntraIntervalFinderService(_skillDayIntraIntervalFinder);
			var skillType = SkillTypeFactory.CreateSkillType();
			skillType.ForecastSource = ForecastSource.MaxSeatSkill;
			_maxSeatSkill = SkillFactory.CreateSkill("skill",skillType,15);
			_normalSkill = SkillFactory.CreateSkill("skill");
		}

		[Test]
		public void ShouldNotWorkWithMaxSeatSkills()
		{
			using (_mock.Record())
			{
				Expect.Call(_schedulingResultStateHolder.SkillDaysOnDateOnly(new List<DateOnly> {new DateOnly()})).Return(new List<ISkillDay>{_skillDay});
				Expect.Call(_skillDay.Skill).Return(_maxSeatSkill);
			}

			using (_mock.Record())
			{
				_target.Execute(_schedulingResultStateHolder, new DateOnly(), _resourceCalculationDataContainer);
			}
		}

		[Test]
		public void ShouldWorkWithNoneMaxSeatSkills()
		{
			using (_mock.Record())
			{
				Expect.Call(_schedulingResultStateHolder.SkillDaysOnDateOnly(new List<DateOnly> { new DateOnly() })).Return(new List<ISkillDay> { _skillDay });
				Expect.Call(_skillDay.Skill).Return(_normalSkill);
				Expect.Call(() =>_skillDayIntraIntervalFinder.SetIntraIntervalIssues(_skillDay, _resourceCalculationDataContainer));
			}

			using (_mock.Record())
			{
				_target.Execute(_schedulingResultStateHolder, new DateOnly(), _resourceCalculationDataContainer);
			}
		}
	}
}
