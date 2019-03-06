using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Export;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData;


namespace Teleopti.Ccc.DomainTest.Forecasting.Export
{
	[TestFixture]
	public class OpenAndSplitSkillCommandTest
	{
		private ISkill targetSkill;
		private IOpenAndSplitSkillCommand target;
		private MockRepository mocks;
		private IScenarioRepository scenarioRepository;
		private ISkillDayRepository skillDayRepository;
		private WorkloadDayHelper workloadDayHelper;

		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			targetSkill = SkillFactory.CreateSkillWithWorkloadAndSources();

			skillDayRepository = mocks.StrictMock<ISkillDayRepository>();
			scenarioRepository = mocks.DynamicMock<IScenarioRepository>();
			workloadDayHelper = new WorkloadDayHelper();

			target = new OpenAndSplitSkillCommand(scenarioRepository, skillDayRepository, workloadDayHelper);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldOpenSkillDaysAndSplitIntoIntervals()
		{
			var period = new DateOnlyPeriod(2011, 1, 1, 2011, 6, 30);
			var targetScenario = ScenarioFactory.CreateScenarioAggregate();
			var skillDay = SkillDayFactory.CreateSkillDay(targetSkill, period.StartDate, targetScenario);
			skillDay.SkillDayCalculator = new SkillDayCalculator(targetSkill, new[] { skillDay }, period);
			skillDay.SplitSkillDataPeriods(skillDay.SkillDataPeriodCollection.ToList());

			targetSkill.RemoveWorkload(targetSkill.WorkloadCollection.First());
			targetSkill.AddWorkload(skillDay.WorkloadDayCollection[0].Workload);

			var secondBusinessUnit = mocks.DynamicMock<IBusinessUnit>();

			targetSkill.SetBusinessUnit(secondBusinessUnit);

			using (mocks.Record())
			{
				Expect.Call(scenarioRepository.LoadDefaultScenario(secondBusinessUnit)).Return(targetScenario);
				Expect.Call(skillDayRepository.FindRange(period, targetSkill, targetScenario)).Return(new[] { skillDay });
				Expect.Call(skillDayRepository.GetAllSkillDays(period, new[] { skillDay }, targetSkill, targetScenario, null)).
						  Return(new[] { skillDay }).IgnoreArguments();
				Expect.Call(() => skillDayRepository.AddRange(new[] {skillDay})).IgnoreArguments();
			}
			using (mocks.Playback())
			{
				target.Execute(targetSkill, period, new List<TimePeriod> { new TimePeriod(6, 0, 8, 0) });
				skillDay.SkillDataPeriodCollection.Count.Should().Be.EqualTo(60);
				skillDay.WorkloadDayCollection[0].OpenTaskPeriodList.Count.Should().Be.EqualTo(8);

				((IFilterOnBusinessUnit)skillDay).GetOrFillWithBusinessUnit_DONTUSE().Should().Be.EqualTo(secondBusinessUnit);
			}
		}
	}
}