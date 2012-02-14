using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Export;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting.Export
{
	[TestFixture]
	public class OpenAndSplitSkillCommandTest
	{
		private ISkill targetSkill;
		private IOpenAndSplitSkillCommand target;
		private MockRepository mocks;
		private IScenarioProvider scenarioProvider;
		private ISkillDayRepository skillDayRepository;
		private WorkloadDayHelper workloadDayHelper;

		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			targetSkill = SkillFactory.CreateSkillWithWorkloadAndSources();

			skillDayRepository = mocks.StrictMock<ISkillDayRepository>();
			scenarioProvider = mocks.DynamicMock<IScenarioProvider>();
			workloadDayHelper = new WorkloadDayHelper();

			target = new OpenAndSplitSkillCommand(scenarioProvider, skillDayRepository, workloadDayHelper);
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

			ReflectionHelper.SetBusinessUnit(targetSkill, secondBusinessUnit);

			using (mocks.Record())
			{
				Expect.Call(scenarioProvider.DefaultScenario(secondBusinessUnit)).Return(targetScenario);
				Expect.Call(skillDayRepository.FindRange(period, targetSkill, targetScenario)).Return(new[] { skillDay });
				Expect.Call(skillDayRepository.GetAllSkillDays(period, new[] { skillDay }, targetSkill, targetScenario, true)).
					Return(new[] { skillDay });
			}
			using (mocks.Playback())
			{
				target.Execute(targetSkill, period);
				skillDay.SkillDataPeriodCollection.Count.Should().Be.EqualTo(60);
				skillDay.WorkloadDayCollection[0].OpenTaskPeriodList.Count.Should().Be.EqualTo(96);

				((IBelongsToBusinessUnit)skillDay).BusinessUnit.Should().Be.EqualTo(secondBusinessUnit);
			}
		}
	}
}