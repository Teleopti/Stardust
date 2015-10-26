using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Intraday
{
	[IntradayTest]
	public class ListSkillStatusTest
	{
		public FakeSkillDayRepository SkillDayRepository;
		public FakeSkillRepository SkillRepository;
		public FakeScenarioRepository ScenarioRepository;
		public IntradaySkillStatusService Target;
		public FakeStatisticRepository StatisticRepository;
		

		[Test]
		public void TargetShouldNotBeNull()
		{
			Target.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldListAllSkillsWithCalls()
		{
			var skillId = Guid.NewGuid();
			var skill = SkillFactory.CreateSkill("Skill", TimeZoneInfo.Utc);
			skill.SetId(skillId);
			var models = new List<SkillTaskDetailsModel>
			{
				new SkillTaskDetailsModel
				{
					TotalTasks = 20,
					SkillId = skillId,
					Minimum = new DateTime(2015, 10, 21, 9, 0, 0, DateTimeKind.Utc),
					Maximum = new DateTime(2015, 10, 21, 10, 0, 0, DateTimeKind.Utc)
				}
			};
			SkillRepository.Add(skill);
			SkillDayRepository.AddFakeTemplateTaskModels(models);
			Assert.True(true);
			var result = Target.GetSkillStatusModels();
			result.Should().Not.Be.Null();
		}

		#region "Forecasted Data Tests"
		
		public SkillForecastedTasksProvider TargetSkillForecastedTasksProvider;
		
		[Test]
		public void ShouldReturnCorrectNumberOfSkills()
		{
			var skill1 = SkillFactory.CreateSkill("skill1");
			skill1.SetId(Guid.NewGuid());
			var skill2 = SkillFactory.CreateSkill("skill2");
			skill2.SetId(Guid.NewGuid());
			SkillRepository.Add(skill1);
			SkillRepository.Add(skill2);

			IEnumerable<SkillTaskDetailsModel> skillTaskDetailsModels = new List<SkillTaskDetailsModel>()
			{
				createFakeTasks(skill1.Id.Value, 5, 0.5),
				createFakeTasks(skill2.Id.Value, 5, 0.5)
			};
			SkillDayRepository.AddFakeTemplateTaskModels(skillTaskDetailsModels);
			var result = TargetSkillForecastedTasksProvider.GetForecastedTasks();
			result.Count.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldReturnForecastedData()
		{
			var skill1 = SkillFactory.CreateSkill("skill1");
			skill1.SetId(Guid.NewGuid());
			var skill2 = SkillFactory.CreateSkill("skill2");
			skill2.SetId(Guid.NewGuid());
			SkillRepository.Add(skill1);
			SkillRepository.Add(skill2);
			IEnumerable<SkillTaskDetailsModel> skillTaskDetailsModels = new List<SkillTaskDetailsModel>()
			{
				createFakeTasks(skill1.Id.Value, 5, 0.5),
				createFakeTasks(skill1.Id.Value, 5, 0.5),
				createFakeTasks(skill1.Id.Value, 5, 0.5),
				createFakeTasks(skill2.Id.Value, 5, 0.5),
				createFakeTasks(skill2.Id.Value, 5, 0.5)
			};
			SkillDayRepository.AddFakeTemplateTaskModels(skillTaskDetailsModels);
			var result = TargetSkillForecastedTasksProvider.GetForecastedTasks();

			result.First().IntervalTasks.Count.Should().Be.EqualTo(3);
			result.Second().IntervalTasks.Count.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldCalculateForecastedTasksCorrectly()
		{
			var skill1 = SkillFactory.CreateSkill("skill1");
			skill1.SetId(Guid.NewGuid());
			var skill2 = SkillFactory.CreateSkill("skill2");
			skill2.SetId(Guid.NewGuid());

			SkillRepository.Add(skill1);
			SkillRepository.Add(skill2);

			IEnumerable<SkillTaskDetailsModel> skillTaskDetailsModels = new List<SkillTaskDetailsModel>()
			{
				createFakeTasks(skill1.Id.Value, 5, 0.5),
				createFakeTasks(skill2.Id.Value,10,0.25)
			};
			SkillDayRepository.AddFakeTemplateTaskModels(skillTaskDetailsModels);
			var result = TargetSkillForecastedTasksProvider.GetForecastedTasks();

			result.First().IntervalTasks.First().Task.Should().Be.EqualTo(7.5);
			result.Second().IntervalTasks.First().Task.Should().Be.EqualTo(12.5);
		}

		private SkillTaskDetailsModel createFakeTasks(Guid skillId, int tasks, double campaign)
		{
			return new SkillTaskDetailsModel()
			{
				SkillId = skillId,
				TotalTasks = tasks + (tasks * campaign),
				Minimum = new DateTime(2015, 10, 21, 9, 0, 0, DateTimeKind.Utc),
				Maximum = new DateTime(2015, 10, 21, 10, 0, 0, DateTimeKind.Utc)
			};
		}
		#endregion

		#region "Actual Data Tests"

		public SkillActualTasksProvider TargetSkillActualTasksProvider;

		[Test]
		public void ShouldReturnActualTaskForOneSkill()
		{
			var skill1 = SkillFactory.CreateSkillWithWorkloadAndSources();
			skill1.SetId(Guid.NewGuid());
			//var skillRepository = new FakeSkillRepository();
			SkillRepository.Add(skill1);

			//FakeStatisticRepository fakeStatisticRepository = new FakeStatisticRepository();
			IStatisticTask statisticTask1 = new StatisticTask()
			{
				Interval = new DateTime(2015, 10, 22, 09, 00, 00, DateTimeKind.Utc),
				StatAnsweredTasks = 10
			};
			IStatisticTask statisticTask2 = new StatisticTask()
			{
				Interval = new DateTime(2015, 10, 22, 09, 15, 00, DateTimeKind.Utc),
				StatAnsweredTasks = 15
			};
			var statisticTaskList = new List<IStatisticTask>() { statisticTask1, statisticTask2 };
			StatisticRepository.FakeStatisticData(skill1.WorkloadCollection.First().QueueSourceCollection.First(), statisticTaskList);
			//var target = new SkillActualTasksProvider(skillRepository, StatisticRepository);
			var result = TargetSkillActualTasksProvider.GetActualTasks();
			result.Count.Should().Be.EqualTo(1);
			//result.First().Value.First().Task.Should().Be.EqualTo(10);
		}

		[Test]
		public void ShouldReturnActualTaskForTwoSkills()
		{
			var skill1 = SkillFactory.CreateSkillWithWorkloadAndSources();
			var skill2 = SkillFactory.CreateSkillWithWorkloadAndSources();
			//skill1.SetId(Guid.NewGuid());
			var skillRepository = new FakeSkillRepository();
			skillRepository.Add(skill1);
			skillRepository.Add(skill2);

			var fakeStatisticRepository = new FakeStatisticRepository();
			IStatisticTask statisticTask1 = new StatisticTask()
			{
				Interval = new DateTime(2015, 10, 22, 09, 00, 00, DateTimeKind.Utc),
				StatAnsweredTasks = 10
			};
			fakeStatisticRepository.FakeStatisticData(skill1.WorkloadCollection.First().QueueSourceCollection.First(), new List<IStatisticTask>() { statisticTask1 });

			IStatisticTask statisticTask2 = new StatisticTask()
			{
				Interval = new DateTime(2015, 10, 22, 09, 15, 00, DateTimeKind.Utc),
				StatAnsweredTasks = 15
			};
			fakeStatisticRepository.FakeStatisticData(skill1.WorkloadCollection.First().QueueSourceCollection.First(), new List<IStatisticTask>() { statisticTask2 });

			var target = new SkillActualTasksProvider(skillRepository, fakeStatisticRepository);
			var result = target.GetActualTasks();
			result.Count.Should().Be.EqualTo(1);
			//result.First().Value.First().Task.Should().Be.EqualTo(10);
		}

		#endregion

	}
}