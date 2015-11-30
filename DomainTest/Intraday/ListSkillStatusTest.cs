using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
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
		public FakeStatisticRepository StatisticRepository;

		#region "Intraday service"
		public IntradaySkillStatusService Target;

		[Test]
		public void TargetShouldNotBeNull()
		{
			Target.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldReturnStatusForOneSkill()
		{
			var skillId = Guid.NewGuid();
			var skill = SkillFactory.CreateSkill("Phone", TimeZoneInfo.Utc);
			skill.SetId(skillId);
			var forecastedData = new List<SkillTaskDetailsModel>
			{
				getSkillTaskDetailsModel(skillId, 7, "Phone", new DateTime(2015, 10, 21, 9, 0, 0, DateTimeKind.Utc),
					new DateTime(2015, 10, 21, 10, 0, 0, DateTimeKind.Utc)),
				getSkillTaskDetailsModel(skillId, 9, "Phone", new DateTime(2015, 10, 21, 10, 0, 0, DateTimeKind.Utc),
					new DateTime(2015, 11, 21, 11, 0, 0, DateTimeKind.Utc)),
				getSkillTaskDetailsModel(skillId, 7, "Phone", new DateTime(2015, 10, 21, 11, 0, 0, DateTimeKind.Utc),
					new DateTime(2015, 12, 21, 12, 0, 0, DateTimeKind.Utc))

			};
			SkillRepository.Add(skill);
			SkillDayRepository.AddFakeTemplateTaskModels(forecastedData);
			IList<IIntradayStatistics> actualData = new List<IIntradayStatistics>()
			{
				getIntradayStatistics(skillId,new DateTime(2015, 10, 21, 9, 0, 0, DateTimeKind.Utc),12,"Phone"),
				getIntradayStatistics(skillId,new DateTime(2015, 10, 21, 10, 0, 0, DateTimeKind.Utc),15,"Phone"),
				getIntradayStatistics(skillId,new DateTime(2015, 10, 21, 11, 0, 0, DateTimeKind.Utc),7,"Phone")
			};
			StatisticRepository.AddIntradayStatistics(actualData);

			var result = Target.GetSkillStatusModels(new DateTime(2015, 10, 21, 11, 0, 0, DateTimeKind.Utc));
			result.Count().Should().Be.EqualTo(1);
			result.First().SkillName.Should().Be.EqualTo("Phone");
			result.First().Measures.First().Value.Should().Be.EqualTo(-11);
		}

		[Test]
		public void ShouldReturnStatusIfNoActualDataFound()
		{
			var skillId = Guid.NewGuid();
			var skill = SkillFactory.CreateSkill("Phone", TimeZoneInfo.Utc);
			skill.SetId(skillId);
			var forecastedData = new List<SkillTaskDetailsModel>
			{
				getSkillTaskDetailsModel(skillId, 7, "Phone", new DateTime(2015, 10, 21, 9, 0, 0, DateTimeKind.Utc),
					new DateTime(2015, 10, 21, 10, 0, 0, DateTimeKind.Utc)),
				getSkillTaskDetailsModel(skillId, 9, "Phone", new DateTime(2015, 10, 21, 10, 0, 0, DateTimeKind.Utc),
					new DateTime(2015, 10, 21, 11, 0, 0, DateTimeKind.Utc)),
				getSkillTaskDetailsModel(skillId, 7, "Phone", new DateTime(2015, 10, 21, 11, 0, 0, DateTimeKind.Utc),
					new DateTime(2015, 10, 21, 12, 0, 0, DateTimeKind.Utc))

			};
			SkillRepository.Add(skill);
			SkillDayRepository.AddFakeTemplateTaskModels(forecastedData);
			StatisticRepository.AddIntradayStatistics(new List<IIntradayStatistics>());

			var result = Target.GetSkillStatusModels(new DateTime(2015, 10, 21, 12, 0, 0, DateTimeKind.Utc));
			result.Count().Should().Be.EqualTo(1);
			result.First().SkillName.Should().Be.EqualTo("Phone");
			result.First().Measures.First().Value.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldReturnSortedSkillsBasedOnSeverity()
		{
			var phoneSkillId = Guid.NewGuid();
			var phoneSkill = SkillFactory.CreateSkill("Phone", TimeZoneInfo.Utc);
			phoneSkill.SetId(phoneSkillId);
			SkillRepository.Add(phoneSkill);

			var chatSkillId = Guid.NewGuid();
			var chatSkill = SkillFactory.CreateSkill("Chat", TimeZoneInfo.Utc);
			chatSkill.SetId(chatSkillId);
			SkillRepository.Add(chatSkill);

			var forecastedData = new List<SkillTaskDetailsModel>
			{
				getSkillTaskDetailsModel(phoneSkillId, 7, "Phone", new DateTime(2015, 10, 21, 9, 0, 0, DateTimeKind.Utc),
					new DateTime(2015, 10, 21, 10, 0, 0, DateTimeKind.Utc)),
				getSkillTaskDetailsModel(phoneSkillId, 9, "Phone", new DateTime(2015, 10, 21, 10, 0, 0, DateTimeKind.Utc),
					new DateTime(2015, 11, 21, 11, 0, 0, DateTimeKind.Utc)),
				getSkillTaskDetailsModel(phoneSkillId, 7, "Phone", new DateTime(2015, 10, 21, 11, 0, 0, DateTimeKind.Utc),
					new DateTime(2015, 12, 21, 12, 0, 0, DateTimeKind.Utc)),
				
				getSkillTaskDetailsModel(chatSkillId, 7, "Chat", new DateTime(2015, 10, 21, 9, 0, 0, DateTimeKind.Utc),
					new DateTime(2015, 10, 21, 10, 0, 0, DateTimeKind.Utc)),
				getSkillTaskDetailsModel(chatSkillId, 12, "Chat", new DateTime(2015, 10, 21, 10, 0, 0, DateTimeKind.Utc),
					new DateTime(2015, 11, 21, 11, 0, 0, DateTimeKind.Utc)),
				getSkillTaskDetailsModel(chatSkillId, 7, "Chat", new DateTime(2015, 10, 21, 11, 0, 0, DateTimeKind.Utc),
					new DateTime(2015, 12, 21, 12, 0, 0, DateTimeKind.Utc))

			};
			SkillDayRepository.AddFakeTemplateTaskModels(forecastedData);
			IList<IIntradayStatistics> actualData = new List<IIntradayStatistics>()
			{
				getIntradayStatistics(phoneSkillId,new DateTime(2015, 10, 21, 9, 0, 0, DateTimeKind.Utc),12,"Phone"),
				getIntradayStatistics(phoneSkillId,new DateTime(2015, 10, 21, 10, 0, 0, DateTimeKind.Utc),15,"Phone"),
				getIntradayStatistics(phoneSkillId,new DateTime(2015, 10, 21, 11, 0, 0, DateTimeKind.Utc),7,"Phone"),
				
				getIntradayStatistics(chatSkillId,new DateTime(2015, 10, 21, 9, 0, 0, DateTimeKind.Utc),12,"Chat"),
				getIntradayStatistics(chatSkillId,new DateTime(2015, 10, 21, 10, 0, 0, DateTimeKind.Utc),7,"Chat"),
				getIntradayStatistics(chatSkillId,new DateTime(2015, 10, 21, 11, 0, 0, DateTimeKind.Utc),9,"Chat")
			};
			StatisticRepository.AddIntradayStatistics(actualData);

			var result = Target.GetSkillStatusModels(new DateTime(2015, 10, 21, 11, 0, 0, DateTimeKind.Utc));
			result.Count().Should().Be.EqualTo(2);
			result.First().SkillName.Should().Be.EqualTo("Chat");
			result.Second().SkillName.Should().Be.EqualTo("Phone");
			result.First().Measures.First().Value.Should().Be.EqualTo(-2);
			result.Second().Measures.First().Value.Should().Be.EqualTo(-11);
		}

		[Test]
		public void ShouldReturnDataThatIsUptillTheIntervalOfActualData()
		{
			var skillId = Guid.NewGuid();
			var skill = SkillFactory.CreateSkill("Phone", TimeZoneInfo.Utc);
			skill.SetId(skillId);
			var forecastedData = new List<SkillTaskDetailsModel>
			{
				getSkillTaskDetailsModel(skillId, 5, "Phone", new DateTime(2015, 10, 21, 9, 0, 0, DateTimeKind.Utc),
					new DateTime(2015, 10, 21, 10, 0, 0, DateTimeKind.Utc)),
				getSkillTaskDetailsModel(skillId, 12, "Phone", new DateTime(2015, 10, 21, 10, 0, 0, DateTimeKind.Utc),
					new DateTime(2015, 11, 21, 11, 0, 0, DateTimeKind.Utc)),
				getSkillTaskDetailsModel(skillId, 13, "Phone", new DateTime(2015, 10, 21, 11, 0, 0, DateTimeKind.Utc),
					new DateTime(2015, 12, 21, 12, 0, 0, DateTimeKind.Utc)),
				getSkillTaskDetailsModel(skillId, 10, "Phone", new DateTime(2015, 10, 21, 12, 0, 0, DateTimeKind.Utc),
					new DateTime(2015, 12, 21, 13, 0, 0, DateTimeKind.Utc))

			};
			SkillRepository.Add(skill);
			SkillDayRepository.AddFakeTemplateTaskModels(forecastedData);
			IList<IIntradayStatistics> actualData = new List<IIntradayStatistics>()
			{
				getIntradayStatistics(skillId,new DateTime(2015, 10, 21, 10, 0, 0, DateTimeKind.Utc),5,"Phone"),
				getIntradayStatistics(skillId,new DateTime(2015, 10, 21, 11, 0, 0, DateTimeKind.Utc),15,"Phone")
			};
			StatisticRepository.AddIntradayStatistics(actualData);

			var result = Target.GetSkillStatusModels(new DateTime(2015, 10, 21, 11, 0, 0, DateTimeKind.Utc));
			result.First().Measures.First().Value.Should().Be.EqualTo(10);
		}

		[Test]
		public void ShouldReturnTheLatestDateOfActualData()
		{
			var skillId = Guid.NewGuid();
			var skill = SkillFactory.CreateSkill("Phone", TimeZoneInfo.Utc);
			skill.SetId(skillId);
			var forecastedData = new List<SkillTaskDetailsModel>
			{
				getSkillTaskDetailsModel(skillId, 5, "Phone", new DateTime(2015, 10, 21, 9, 0, 0, DateTimeKind.Utc),
					new DateTime(2015, 10, 21, 10, 0, 0, DateTimeKind.Utc)),
				getSkillTaskDetailsModel(skillId, 12, "Phone", new DateTime(2015, 10, 21, 10, 0, 0, DateTimeKind.Utc),
					new DateTime(2015, 11, 21, 11, 0, 0, DateTimeKind.Utc)),
				getSkillTaskDetailsModel(skillId, 13, "Phone", new DateTime(2015, 10, 21, 11, 0, 0, DateTimeKind.Utc),
					new DateTime(2015, 12, 21, 12, 0, 0, DateTimeKind.Utc)),
				getSkillTaskDetailsModel(skillId, 10, "Phone", new DateTime(2015, 10, 21, 12, 0, 0, DateTimeKind.Utc),
					new DateTime(2015, 12, 21, 13, 0, 0, DateTimeKind.Utc))

			};
			SkillRepository.Add(skill);
			SkillDayRepository.AddFakeTemplateTaskModels(forecastedData);
			IList<IIntradayStatistics> actualData = new List<IIntradayStatistics>()
			{
				getIntradayStatistics(skillId,new DateTime(2015, 10, 21, 10, 0, 0, DateTimeKind.Utc),5,"Phone"),
				getIntradayStatistics(skillId,new DateTime(2015, 10, 21, 11, 0, 0, DateTimeKind.Utc),15,"Phone")
			};
			StatisticRepository.AddIntradayStatistics(actualData);

			var result = Target.GetSkillStatusModels(new DateTime(2015, 10, 21, 12, 0, 0, DateTimeKind.Utc));
			result.First().Measures.First().LatestDate.Should().Be.EqualTo(new DateTime(2015, 10, 21, 11, 0, 0, DateTimeKind.Utc));
		}

		[Test]
		public void ShouldReturnTheSumOfForecastedDataUntilActualData()
		{
			var skillId = Guid.NewGuid();
			var skill = SkillFactory.CreateSkill("Phone", TimeZoneInfo.Utc);
			skill.SetId(skillId);
			var forecastedData = new List<SkillTaskDetailsModel>
			{
				getSkillTaskDetailsModel(skillId, 5, "Phone", new DateTime(2015, 10, 21, 9, 0, 0, DateTimeKind.Utc),
					new DateTime(2015, 10, 21, 10, 0, 0, DateTimeKind.Utc)),
				getSkillTaskDetailsModel(skillId, 12, "Phone", new DateTime(2015, 10, 21, 10, 0, 0, DateTimeKind.Utc),
					new DateTime(2015, 11, 21, 11, 0, 0, DateTimeKind.Utc)),
				getSkillTaskDetailsModel(skillId, 13, "Phone", new DateTime(2015, 10, 21, 11, 0, 0, DateTimeKind.Utc),
					new DateTime(2015, 12, 21, 12, 0, 0, DateTimeKind.Utc)),
				getSkillTaskDetailsModel(skillId, 10, "Phone", new DateTime(2015, 10, 21, 12, 0, 0, DateTimeKind.Utc),
					new DateTime(2015, 12, 21, 13, 0, 0, DateTimeKind.Utc))

			};
			SkillRepository.Add(skill);
			SkillDayRepository.AddFakeTemplateTaskModels(forecastedData);
			IList<IIntradayStatistics> actualData = new List<IIntradayStatistics>()
			{
				getIntradayStatistics(skillId,new DateTime(2015, 10, 21, 10, 0, 0, DateTimeKind.Utc),5,"Phone"),
				getIntradayStatistics(skillId,new DateTime(2015, 10, 21, 11, 0, 0, DateTimeKind.Utc),15,"Phone")
			};
			StatisticRepository.AddIntradayStatistics(actualData);

			var result = Target.GetSkillStatusModels(new DateTime(2015, 10, 21, 12, 0, 0, DateTimeKind.Utc));
			result.First().Measures.First().ForecastedCalls.Should().Be.EqualTo(30);
			result.First().Measures.First().ActualCalls.Should().Be.EqualTo(20);
		}

		[Test]
		public void ShouldReturnDateAccordingToSkillTimeZone()
		{
			var skillId = Guid.NewGuid();
			var skill = SkillFactory.CreateSkill("Phone", TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time"));
			skill.SetId(skillId);
			var forecastedData = new List<SkillTaskDetailsModel>
			{
				getSkillTaskDetailsModel(skillId, 5, "Phone", new DateTime(2015, 10, 21, 9, 0, 0, DateTimeKind.Utc),
					new DateTime(2015, 10, 21, 10, 0, 0, DateTimeKind.Utc))

			};
			SkillRepository.Add(skill);
			SkillDayRepository.AddFakeTemplateTaskModels(forecastedData);
			IList<IIntradayStatistics> actualData = new List<IIntradayStatistics>()
			{
				getIntradayStatistics(skillId,new DateTime(2015, 10, 21, 14, 0, 0, DateTimeKind.Unspecified),5,"Phone")
			};
			StatisticRepository.AddIntradayStatistics(actualData);

			var result = Target.GetSkillStatusModels(new DateTime(2015, 10, 21, 9, 0, 0, DateTimeKind.Utc));
			result.First()
				.Measures.First()
				.LatestDate.Should()
				.Be.EqualTo(new DateTime(2015, 10, 21,14, 0, 0, DateTimeKind.Unspecified));
		}

		[Test]
		public void ShouldIgnoreTheSkillWithoutAnyQueueOrWorkload()
		{
			var phoneSkillId = Guid.NewGuid();
			var chatSkillId = Guid.NewGuid();
			var phoneSkill = SkillFactory.CreateSkill("Phone", TimeZoneInfo.Utc);
			phoneSkill.SetId(phoneSkillId);
			var forecastedData = new List<SkillTaskDetailsModel>
			{
				getSkillTaskDetailsModel(phoneSkillId, 7, "Phone", new DateTime(2015, 10, 21, 9, 0, 0, DateTimeKind.Utc),
					new DateTime(2015, 10, 21, 10, 0, 0, DateTimeKind.Utc))

			};
			SkillRepository.Add(phoneSkill);
			SkillDayRepository.AddFakeTemplateTaskModels(forecastedData);
			IList<IIntradayStatistics> actualData = new List<IIntradayStatistics>()
			{
				getIntradayStatistics(chatSkillId,new DateTime(2015, 10, 21, 9, 0, 0, DateTimeKind.Utc),12,"Chat"),
				getIntradayStatistics(phoneSkillId,new DateTime(2015, 10, 21, 9, 0, 0, DateTimeKind.Utc),12,"Phone")
			};
			StatisticRepository.AddIntradayStatistics(actualData);

			var result = Target.GetSkillStatusModels(new DateTime(2015, 10, 21, 11, 0, 0, DateTimeKind.Utc));
			result.Count().Should().Be.EqualTo(1);
			result.First().SkillName.Should().Be.EqualTo("Phone");
		}

		private static IntradayStatistics getIntradayStatistics(Guid skillId, DateTime start, int task, string skillName)
		{
			return new IntradayStatistics()
			{
				Interval = start,
				SkillId = skillId,
				SkillName = skillName,
				StatOfferedTasks = task
			};
		}

		private static SkillTaskDetailsModel getSkillTaskDetailsModel(Guid skillId, int task, String skillName, DateTime start, DateTime end)
		{
			return new SkillTaskDetailsModel
			{
				TotalTasks = task,
				Name = skillName,
				SkillId = skillId,
				Minimum = start,
				Maximum = end
			};
		}

		#endregion
		
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
			var result = TargetSkillForecastedTasksProvider.GetForecastedTasks(new DateTime(2015, 10, 21, 9, 0, 0, DateTimeKind.Utc));
			result.Count.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldHandleNonUTCDatesInForecastedTasks()
		{
			var skill1 = SkillFactory.CreateSkill("skill1");
			skill1.SetId(Guid.NewGuid());
			SkillRepository.Add(skill1);

			IEnumerable<SkillTaskDetailsModel> skillTaskDetailsModels = new List<SkillTaskDetailsModel>()
			{
				new SkillTaskDetailsModel()
				{
					SkillId = skill1.Id.Value,
					TotalTasks = 10,
					Minimum = new DateTime(2015, 10, 21, 9, 0, 0),
					Maximum = new DateTime(2015, 10, 21, 10, 0, 0)
				}
			};
			SkillDayRepository.AddFakeTemplateTaskModels(skillTaskDetailsModels);
			var result = TargetSkillForecastedTasksProvider.GetForecastedTasks(new DateTime(2015, 10, 21, 9, 0, 0,DateTimeKind.Utc));
			result.Count.Should().Be.EqualTo(1);
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
			var result = TargetSkillForecastedTasksProvider.GetForecastedTasks(new DateTime(2015, 10, 21, 9, 0, 0, DateTimeKind.Utc));

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
			var result = TargetSkillForecastedTasksProvider.GetForecastedTasks(new DateTime(2015, 10, 21, 10, 0, 0, DateTimeKind.Utc));

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
			var skill = SkillFactory.CreateSkill("phone", TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
			Guid skillId = Guid.NewGuid();
			skill.SetId(skillId);
			SkillRepository.Add(skill);

			IIntradayStatistics intradayStatistic1 = new IntradayStatistics()
			{
				Interval = TimeZoneHelper.ConvertFromUtc(new DateTime(2015, 10, 22, 09, 00, 00,  DateTimeKind.Utc),  TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time")) ,
				StatOfferedTasks = 10,
				SkillId = skillId
			};
			IIntradayStatistics intradayStatistic2 = new IntradayStatistics()
			{
				Interval =  TimeZoneHelper.ConvertFromUtc(new DateTime(2015, 10, 22, 09, 15, 00,  DateTimeKind.Utc),  TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time")), 
				StatOfferedTasks = 15,
				SkillId = skillId
			};
			StatisticRepository.AddIntradayStatistics(new List<IIntradayStatistics>(){intradayStatistic1,intradayStatistic2});
			var skillsTimeZone = new Dictionary<Guid,TimeZoneInfo>();
			skillsTimeZone.Add(skill.Id.Value, TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
         var result = TargetSkillActualTasksProvider.GetActualTasks(skillsTimeZone);
			result.Count.Should().Be.EqualTo(1);
			result.First().IntervalTasks.First().Task.Should().Be.EqualTo(10);
			result.First().IntervalTasks.Second().Task.Should().Be.EqualTo(15);
		}

		[Test]
		public void ShouldReturnActualTaskForTwoSkills()
		{
			var skill1 = SkillFactory.CreateSkill("phone", TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
			var skill2 = SkillFactory.CreateSkill("phone", TimeZoneInfo.Utc);
			Guid skill1Id = Guid.NewGuid();
			skill1.SetId(skill1Id);
			Guid skill2Id = Guid.NewGuid();
			skill2.SetId(skill2Id);
			SkillRepository.Add(skill1);
			SkillRepository.Add(skill2);

			IIntradayStatistics intradayStatistic1 = new IntradayStatistics()
			{
				Interval = TimeZoneHelper.ConvertFromUtc(new DateTime(2015, 10, 22, 09, 00, 00, DateTimeKind.Utc),TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time")),
				StatOfferedTasks = 10,
				SkillId = skill1Id
			};
			IIntradayStatistics intradayStatistic2 = new IntradayStatistics()
			{
				Interval = new DateTime(2015, 10, 22, 09, 15, 00, DateTimeKind.Utc),
				StatOfferedTasks = 15,
				SkillId = skill2Id
			};
			StatisticRepository.AddIntradayStatistics(new List<IIntradayStatistics>() { intradayStatistic1, intradayStatistic2 });
			var skillsTimeZone = new Dictionary<Guid, TimeZoneInfo>();
			skillsTimeZone.Add(skill1.Id.Value, TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
			skillsTimeZone.Add(skill2.Id.Value, TimeZoneInfo.Utc);
			var result = TargetSkillActualTasksProvider.GetActualTasks(skillsTimeZone);
			result.Count.Should().Be.EqualTo(2);
			result.First().IntervalTasks.First().Task.Should().Be.EqualTo(10);
			result.First()
				.IntervalTasks.First()
				.IntervalStart.Should()
				.Be.EqualTo(new DateTime(2015, 10, 22, 09, 00, 00, DateTimeKind.Utc));
			result.Second().IntervalTasks.First().Task.Should().Be.EqualTo(15);
			result.Second()
				.IntervalTasks.First()
				.IntervalStart.Should()
				.Be.EqualTo(new DateTime(2015, 10, 22, 09, 15, 00, DateTimeKind.Utc));
		}

		#endregion

	}
}