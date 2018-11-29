using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Intraday;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.WinCodeTest.Intraday
{
	[TestFixture]
	public class LoadStatisticsAndActualHeadsCommandTest
	{
		private MockRepository mocks;
		private IStatisticRepository repository;
		private LoadStatisticsAndActualHeadsCommand target;
		private ISkill skill;
		private DateOnly day;
		private DateTimePeriod period;
		private IWorkload workload;

		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			repository = mocks.DynamicMock<IStatisticRepository>();

			skill = SkillFactory.CreateSkillWithWorkloadAndSources();
			workload = skill.WorkloadCollection.First();
			day = new DateOnly(2012, 4, 23);
			period = new DateOnlyPeriod(day, day).ToDateTimePeriod(skill.TimeZone);

			target = new LoadStatisticsAndActualHeadsCommand(repository);
		}

		[Test]
		public void VerifyLoadStatistics()
		{
			var skillDay = mocks.DynamicMock<ISkillDay>();
			var activeAgentCounts = new List<IActiveAgentCount>();
			var statisticTasks = new List<IStatisticTask>();
			var skillStaffPeriod1 = mocks.DynamicMock<ISkillStaffPeriod>();
			var skillStaffPeriods = new List<ISkillStaffPeriod> { skillStaffPeriod1 };
			var workloadDay = mocks.DynamicMock<IWorkloadDay>();
			var skillDayCalculator = mocks.DynamicMock<ISkillDayCalculator>();
			var smallPeriod = new DateTimePeriod(period.StartDateTime, period.StartDateTime.AddMinutes(15));
			var dayPeriod = period.ChangeEndTime(TimeSpan.FromHours(1));

			using (mocks.Record())
			{
				Expect.Call(repository.LoadActiveAgentCount(skill, dayPeriod)).Return(activeAgentCounts).Repeat.Once();
				Expect.Call(repository.LoadSpecificDates(workload.QueueSourceCollection, dayPeriod)).Return(statisticTasks).Repeat.Once();
				Expect.Call(skillStaffPeriod1.SkillDay).Return(skillDay).Repeat.AtLeastOnce();
				Expect.Call(skillStaffPeriod1.Period).Return(smallPeriod).Repeat.AtLeastOnce();
				Expect.Call(workloadDay.Workload.Equals(workload)).Return(true);
				Expect.Call(workloadDay.OpenTaskPeriodList).Return(new ReadOnlyCollection<ITemplateTaskPeriod>(new List<ITemplateTaskPeriod>()));
				Expect.Call(skillDay.WorkloadDayCollection).Return(new ReadOnlyCollection<IWorkloadDay>(new[] { workloadDay }));
				Expect.Call(skillDay.SkillDayCalculator).Return(skillDayCalculator);
			}

			using (mocks.Playback())
			{
				target.Execute(day, skill, dayPeriod, skillStaffPeriods);
			}
		}

		[Test]
		public void VerifyLoadStatisticsWithMidnightBreak()
		{
			skill.MidnightBreakOffset = TimeSpan.FromHours(2);

			var skillDay1 = mocks.DynamicMock<ISkillDay>();
			var skillDay2 = mocks.DynamicMock<ISkillDay>();
			var activeAgentCounts = new List<IActiveAgentCount>();
			var statisticTasks = new List<IStatisticTask>();
			var skillStaffPeriod1 = mocks.DynamicMock<ISkillStaffPeriod>();
			var skillStaffPeriod2 = mocks.DynamicMock<ISkillStaffPeriod>();
			var skillStaffPeriods = new List<ISkillStaffPeriod> { skillStaffPeriod1,skillStaffPeriod2 };
			var workloadDay1 = mocks.DynamicMock<IWorkloadDay>();
			var workloadDay2 = mocks.DynamicMock<IWorkloadDay>();
			var skillDayCalculator = mocks.DynamicMock<ISkillDayCalculator>();
			var smallPeriod = new DateTimePeriod(period.StartDateTime, period.StartDateTime.AddMinutes(15));
			var dayPeriod = period.ChangeEndTime(skill.MidnightBreakOffset.Add(TimeSpan.FromHours(1)));

			using (mocks.Record())
			{
				Expect.Call(repository.LoadActiveAgentCount(skill, dayPeriod)).Return(activeAgentCounts).Repeat.Once();
				Expect.Call(repository.LoadSpecificDates(workload.QueueSourceCollection, dayPeriod)).Return(statisticTasks).Repeat.Once();
				Expect.Call(skillStaffPeriod1.SkillDay).Return(skillDay1).Repeat.AtLeastOnce();
				Expect.Call(skillStaffPeriod2.SkillDay).Return(skillDay2).Repeat.AtLeastOnce();
				Expect.Call(skillStaffPeriod1.Period).Return(smallPeriod).Repeat.AtLeastOnce();
				Expect.Call(skillStaffPeriod2.Period).Return(smallPeriod.MovePeriod(TimeSpan.FromMinutes(15))).Repeat.AtLeastOnce();
				Expect.Call(workloadDay1.Workload.Equals(workload)).Return(true);
				Expect.Call(workloadDay2.Workload.Equals(workload)).Return(true);
				Expect.Call(workloadDay1.OpenTaskPeriodList).Return(new ReadOnlyCollection<ITemplateTaskPeriod>(new List<ITemplateTaskPeriod>()));
				Expect.Call(workloadDay2.OpenTaskPeriodList).Return(new ReadOnlyCollection<ITemplateTaskPeriod>(new List<ITemplateTaskPeriod>()));
				Expect.Call(skillDay1.WorkloadDayCollection).Return(new ReadOnlyCollection<IWorkloadDay>(new[] { workloadDay1 }));
				Expect.Call(skillDay2.WorkloadDayCollection).Return(new ReadOnlyCollection<IWorkloadDay>(new[] { workloadDay2 }));
				Expect.Call(skillDay1.SkillDayCalculator).Return(skillDayCalculator);
				Expect.Call(skillDay2.SkillDayCalculator).Return(skillDayCalculator);
			}

			using (mocks.Playback())
			{
				target.Execute(day, skill, dayPeriod, skillStaffPeriods);
			}
		}
	}
}