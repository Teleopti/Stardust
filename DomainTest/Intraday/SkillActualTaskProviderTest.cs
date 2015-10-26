using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Intraday
{
	[Ignore]
	public class SkillActualTaskProviderTest
	{
		[Test]
		public void ShouldReturnCorrectNumberOfSkills()
		{
			var skill1 = SkillFactory.CreateSkill("skill1");
			skill1.SetId(Guid.NewGuid());
			var skill2 = SkillFactory.CreateSkill("skill2");
			skill2.SetId(Guid.NewGuid());
			var skillRepository = new FakeSkillRepository();
			skillRepository.Add(skill1);
			skillRepository.Add(skill2);

			IStatisticRepository fakeStatisticRepository = new FakeStatisticRepository();
			var target = new SkillActualTasksProvider(skillRepository, fakeStatisticRepository);
			var result = target.GetActualTasks();
			result.Count.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldReturnActualTaskForOneSkill()
		{
			var skill1 = SkillFactory.CreateSkillWithWorkloadAndSources();
			skill1.SetId(Guid.NewGuid());
			var skillRepository = new FakeSkillRepository();
			skillRepository.Add(skill1);

			FakeStatisticRepository fakeStatisticRepository = new FakeStatisticRepository();
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
			var statisticTaskList = new List<IStatisticTask>() {statisticTask1, statisticTask2};
			fakeStatisticRepository.FakeStatisticData(skill1.WorkloadCollection.First().QueueSourceCollection.First(), statisticTaskList);
			var target = new SkillActualTasksProvider(skillRepository, fakeStatisticRepository);
			var result = target.GetActualTasks();
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
	}
}
