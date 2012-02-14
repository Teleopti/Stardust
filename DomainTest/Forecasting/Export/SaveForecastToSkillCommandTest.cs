using System;
using System.Collections.Generic;
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
	public class SaveForecastToSkillCommandTest
	{
		private ISkill targetSkill;
		private ISaveForecastToSkillCommand target;
		private MockRepository mocks;
		private ISkillDayLoadHelper skillDayLoadHelper;
		private IScenarioProvider scenarioProvider;
		private ISkillDayRepository skillDayRepository;

		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			targetSkill = SkillFactory.CreateSkillWithWorkloadAndSources();

			skillDayRepository = mocks.DynamicMock<ISkillDayRepository>();
			scenarioProvider = mocks.DynamicMock<IScenarioProvider>();
			skillDayLoadHelper = mocks.DynamicMock<ISkillDayLoadHelper>();

			target = new SaveForecastToSkillCommand(skillDayLoadHelper, skillDayRepository, scenarioProvider);
		}

		[Test]
		public void ShouldCopyForecastFromMultisiteSubSkillToSkill()
		{
			var period = new DateOnlyPeriod(2011, 1, 1, 2011, 6, 30);
			var targetScenario = ScenarioFactory.CreateScenarioAggregate();
			var skillDay = SkillDayFactory.CreateSkillDay(targetSkill, period.StartDate, targetScenario);
			skillDay.SkillDayCalculator = new SkillDayCalculator(targetSkill, new[] { skillDay }, period);
			skillDay.SplitSkillDataPeriods(skillDay.SkillDataPeriodCollection.ToList());

			targetSkill.RemoveWorkload(targetSkill.WorkloadCollection.First());
			targetSkill.AddWorkload(skillDay.WorkloadDayCollection[0].Workload);

			var skillStaffPeriod = mocks.DynamicMock<ISkillStaffPeriod>();
			var skillStaff = mocks.DynamicMock<ISkillStaff>();
			var skillStaffPeriodDictionary = mocks.DynamicMock<ISkillStaffPeriodDictionary>();
			var secondBusinessUnit = mocks.DynamicMock<IBusinessUnit>();
			var intervalPeriod = new DateTimePeriod(skillDay.SkillDataPeriodCollection[0].Period.StartDateTime,
													skillDay.SkillDataPeriodCollection[0].Period.StartDateTime.AddMinutes(
														targetSkill.DefaultResolution));

			ReflectionHelper.SetBusinessUnit(targetSkill, secondBusinessUnit);
			ReflectionHelper.SetBusinessUnit(skillDay, secondBusinessUnit);

			using (mocks.Record())
			{
				Expect.Call(scenarioProvider.DefaultScenario(secondBusinessUnit)).Return(targetScenario);
				Expect.Call(skillDayLoadHelper.LoadSchedulerSkillDays(period, new[] { targetSkill }, targetScenario)).Return(
					new Dictionary<ISkill, IList<ISkillDay>> { { targetSkill, new[] { skillDay } } });
				Expect.Call(skillDayRepository.GetAllSkillDays(period, new[] { skillDay }, targetSkill, targetScenario, false)).
					Return(new[] { skillDay });
				ISkillStaffPeriod dummyStaffPeriod;
				Expect.Call(skillStaffPeriodDictionary.TryGetValue(intervalPeriod,
																   out dummyStaffPeriod)).OutRef(skillStaffPeriod).Return(true);
				Expect.Call(skillStaffPeriod.Period).Return(intervalPeriod);
				Expect.Call(skillStaffPeriod.Payload).Return(skillStaff).Repeat.AtLeastOnce();
				Expect.Call(skillStaff.TaskData).Return(new Task(2, TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(4)));
				Expect.Call(skillStaff.ForecastedIncomingDemand).Return(3d);
				Expect.Call(skillStaff.Shrinkage).Return(new Percent(0.1));
			}
			using (mocks.Playback())
			{
				target.Execute(period, targetSkill, skillStaffPeriodDictionary);
				skillDay.SkillDataPeriodCollection[0].ManualAgents.Should().Be.EqualTo(3d);
				skillDay.SkillDataPeriodCollection[0].Shrinkage.Value.Should().Be.EqualTo(0.1d);

				var taskPeriod = skillDay.WorkloadDayCollection[0].TaskPeriodList.FirstOrDefault(p => p.Period.StartDateTime == intervalPeriod.StartDateTime);
				taskPeriod.Tasks.Should().Be.EqualTo(2d);
				taskPeriod.AverageTaskTime.Should().Be.EqualTo(TimeSpan.FromSeconds(3));
				taskPeriod.AverageAfterTaskTime.Should().Be.EqualTo(TimeSpan.FromSeconds(4));

				((IBelongsToBusinessUnit)skillDay).BusinessUnit.Should().Be.EqualTo(secondBusinessUnit);
			}
		}
	}
}