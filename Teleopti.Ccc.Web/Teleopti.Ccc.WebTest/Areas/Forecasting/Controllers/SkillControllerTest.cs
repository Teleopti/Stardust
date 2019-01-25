using System;
using System.Linq;
using System.Web.Http;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Forecasting.Controllers;
using Teleopti.Ccc.Web.Core.Data;


namespace Teleopti.Ccc.WebTest.Areas.Forecasting.Controllers
{
	[TestFixture]
	public class SkillControllerTest
	{
		[Test]
		public void ShouldCreateSkill()
		{
			var skillRepository = MockRepository.GenerateMock<ISkillRepository>();
			skillRepository.Stub(x => x.LoadAll()).Return(new ISkill[] {});
			var intervalLengthFetcher = MockRepository.GenerateMock<IIntervalLengthFetcher>();
			intervalLengthFetcher.Stub(x => x.GetIntervalLength()).Return(14);
			var queueSourceRepository = MockRepository.GenerateMock<IQueueSourceRepository>();
			var activityRepository = MockRepository.GenerateMock<IActivityRepository>();
			var skillTypeProvider = MockRepository.GenerateMock<ISkillTypeProvider>();
			skillTypeProvider.Stub(x=>x.InboundTelephony()).Return(new SkillTypePhone(new Description("Skill type"), ForecastSource.InboundTelephony));
			var workloadRepository = new FakeWorkloadRepository();
			var target = new SkillController(null, skillRepository, intervalLengthFetcher, queueSourceRepository, activityRepository, skillTypeProvider, workloadRepository);
			var input = new SkillInput
			{
				Name = "test1",
				ActivityId = Guid.NewGuid(),
				TimezoneId = TimeZoneInfo.Utc.Id,
				Queues = new[] {Guid.NewGuid()},
				ServiceLevelPercent = 10,
				ServiceLevelSeconds = 20,
				Shrinkage = 5
			};
			var queueSource = new QueueSource("testQ","", 1);
			queueSource.SetId(input.Queues[0]);
			queueSourceRepository.Stub(x => x.LoadAll()).Return(new IQueueSource[] {queueSource});
			var activity = ActivityFactory.CreateActivity("test1");
			activity.SetId(input.ActivityId);
			activityRepository.Stub(x => x.Load(input.ActivityId)).Return(activity);

			IHttpActionResult  result = target.Create(input);

			skillRepository.AssertWasCalled(
				x =>
					x.Add(
						Arg<ISkill>.Matches(
							s =>
								s.Activity.Id.GetValueOrDefault() == input.ActivityId &&
								s.Name == input.Name &&
								s.TimeZone.Id == input.TimezoneId &&
								s.WorkloadCollection.First().QueueSourceCollection.First().Id.GetValueOrDefault() == input.Queues[0] &&
								s.DefaultResolution == 14)));
			Assert.NotNull(((dynamic)result).Content.WorkloadId);
		}

		[Test]
		public void ShouldSetOpenHours()
		{
			var workloadRepository = MockRepository.GenerateMock<IWorkloadRepository>();
			var skillRepository = MockRepository.GenerateMock<ISkillRepository>();
			skillRepository.Stub(x => x.LoadAll()).Return(new ISkill[] { });
			var intervalLengthFetcher = MockRepository.GenerateMock<IIntervalLengthFetcher>();
			intervalLengthFetcher.Stub(x => x.GetIntervalLength()).Return(14);
			var skillTypeProvider = MockRepository.GenerateMock<ISkillTypeProvider>();
			var activityRepository = MockRepository.GenerateMock<IActivityRepository>();
			var queueSourceRepository = MockRepository.GenerateMock<IQueueSourceRepository>();
			skillTypeProvider.Stub(x => x.InboundTelephony()).Return(new SkillTypePhone(new Description("Skill type"), ForecastSource.InboundTelephony));
			var target = new SkillController(null, skillRepository, intervalLengthFetcher, queueSourceRepository, activityRepository, skillTypeProvider, workloadRepository);
			var input = new SkillInput
			{
				Name = "test1",
				ActivityId = Guid.NewGuid(),
				TimezoneId = TimeZoneInfo.Utc.Id,
				Queues = new[] {Guid.NewGuid()},
				ServiceLevelPercent = 10,
				ServiceLevelSeconds = 20,
				Shrinkage = 5,
				OpenHours = new[]
				{
					new OpenHoursInput
					{
						WeekDaySelections = new[]
						{
							new WeekDaySelection
							{
								WeekDay = DayOfWeek.Sunday,
								Checked = true
							}
						},
						StartTime = new TimeSpan(8, 0, 0),
						EndTime = new TimeSpan(17, 0, 0),
					}
				}
			};

			var queueSource = new QueueSource("testQ", "", 1);
			queueSource.SetId(input.Queues[0]);
			queueSourceRepository.Stub(x => x.LoadAll()).Return(new IQueueSource[] { queueSource });
			var activity = ActivityFactory.CreateActivity("test1");
			activity.SetId(input.ActivityId);
			activityRepository.Stub(x => x.Load(input.ActivityId)).Return(activity);

			target.Create(input);

			workloadRepository.AssertWasCalled(
				x =>
					x.Add(
						Arg<IWorkload>.Matches(
							w =>
								w.TemplateWeekCollection[(int) DayOfWeek.Sunday].OpenHourList.First() ==
								new TimePeriod(input.OpenHours[0].StartTime, input.OpenHours[0].EndTime))));
		}

		[Test]
		public void ShouldSetSkillDayTemplates()
		{
			var skillRepository = MockRepository.GenerateMock<ISkillRepository>();
			skillRepository.Stub(x => x.LoadAll()).Return(new ISkill[] { });
			var intervalLengthFetcher = MockRepository.GenerateMock<IIntervalLengthFetcher>();
			intervalLengthFetcher.Stub(x => x.GetIntervalLength()).Return(14);
			var queueSourceRepository = MockRepository.GenerateMock<IQueueSourceRepository>();
			var activityRepository = MockRepository.GenerateMock<IActivityRepository>();
			var skillTypeProvider = MockRepository.GenerateMock<ISkillTypeProvider>();
			skillTypeProvider.Stub(x => x.InboundTelephony()).Return(new SkillTypePhone(new Description("Skill type"), ForecastSource.InboundTelephony));
			var workloadRepository = MockRepository.GenerateMock<IWorkloadRepository>();
			var target = new SkillController(null, skillRepository, intervalLengthFetcher, queueSourceRepository, activityRepository, skillTypeProvider, workloadRepository);
			var input = new SkillInput
			{
				Name = "test1",
				ActivityId = Guid.NewGuid(),
				TimezoneId = TimeZoneInfo.Utc.Id,
				Queues = new[] { Guid.NewGuid() },
				ServiceLevelPercent = 20,
				ServiceLevelSeconds =10,
				Shrinkage= 21
			};
			var queueSource = new QueueSource("testQ", "", 1);
			queueSource.SetId(input.Queues[0]);
			queueSourceRepository.Stub(x => x.LoadAll()).Return(new IQueueSource[] { queueSource });
			var activity = ActivityFactory.CreateActivity("test1");
			activity.SetId(input.ActivityId);
			activityRepository.Stub(x => x.Load(input.ActivityId)).Return(activity);

			target.Create(input);

			skillRepository.AssertWasCalled(
				x =>
					x.Add(
						Arg<ISkill>.Matches(
							s =>
								s.TemplateWeekCollection[(int) DayOfWeek.Sunday].TemplateSkillDataPeriodCollection.First().ServiceLevelPercent==new Percent(input.ServiceLevelPercent/100.0)&&
								Math.Abs(s.TemplateWeekCollection[(int)DayOfWeek.Sunday].TemplateSkillDataPeriodCollection.First().ServiceLevelSeconds - 10.0) < 0.001&&
								s.TemplateWeekCollection[(int)DayOfWeek.Sunday].TemplateSkillDataPeriodCollection.First().MinOccupancy==new Percent(0.3)&&
								s.TemplateWeekCollection[(int)DayOfWeek.Sunday].TemplateSkillDataPeriodCollection.First().MaxOccupancy==new Percent(0.9)&&
								s.TemplateWeekCollection[(int)DayOfWeek.Sunday].TemplateSkillDataPeriodCollection.First().Shrinkage==new Percent(input.Shrinkage/100.0) &&
								s.TemplateWeekCollection[(int)DayOfWeek.Sunday].TemplateSkillDataPeriodCollection.First().Efficiency==new Percent(1.0)
								)));
		}

		[Test]
		public void ShouldGetIntervalLengthFromExistingSkill()
		{
			var input = new SkillInput
			{
				Name = "test1",
				ActivityId = Guid.NewGuid(),
				TimezoneId = TimeZoneInfo.Utc.Id,
				Queues = new[] { Guid.NewGuid() },
				ServiceLevelPercent = 10,
				ServiceLevelSeconds = 20,
				Shrinkage = 5
			};
			var activity = ActivityFactory.CreateActivity("test1");
			activity.SetId(input.ActivityId);
			var skillRepository = MockRepository.GenerateMock<ISkillRepository>();
			var existingSkill = SkillFactory.CreateSkill("testSkill1");
			existingSkill.DefaultResolution = 16;
			existingSkill.Activity = activity;
			skillRepository.Stub(x => x.LoadAll()).Return(new[] { existingSkill });
			var intervalLengthFetcher = MockRepository.GenerateMock<IIntervalLengthFetcher>();
			intervalLengthFetcher.Stub(x => x.GetIntervalLength()).Return(14);
			var queueSourceRepository = MockRepository.GenerateMock<IQueueSourceRepository>();
			var activityRepository = MockRepository.GenerateMock<IActivityRepository>();
			var queueSource = new QueueSource("testQ", "", 1);
			queueSource.SetId(input.Queues[0]);
			queueSourceRepository.Stub(x => x.LoadAll()).Return(new IQueueSource[] { queueSource });
			activityRepository.Stub(x => x.Load(input.ActivityId)).Return(activity);

			var skillTypeProvider = MockRepository.GenerateMock<ISkillTypeProvider>();
			skillTypeProvider.Stub(x => x.InboundTelephony()).Return(new SkillTypePhone(new Description("Skill type"), ForecastSource.InboundTelephony));
			var target = new SkillController(null, skillRepository, intervalLengthFetcher, queueSourceRepository, activityRepository, skillTypeProvider, MockRepository.GenerateMock<IWorkloadRepository>());

			target.Create(input);

			skillRepository.AssertWasCalled(
				x =>
					x.Add(
						Arg<ISkill>.Matches(
							s => s.DefaultResolution == 16)));
		}

		[Test]
		public void ShouldGetQueues()
		{
			var queueSourceRepository = MockRepository.GenerateMock<IQueueSourceRepository>();
			var queueSource = new QueueSource("name1", "description1", 1);
			queueSource.SetId(Guid.NewGuid());
			queueSource.LogObjectName = "logObjectName1";
			queueSourceRepository.Stub(x => x.LoadAll()).Return(new IQueueSource[] {queueSource});
			var target = new SkillController(null, null, null, queueSourceRepository, null, null, MockRepository.GenerateMock<IWorkloadRepository>());
			var first = target.Queues().First();
			Assert.AreEqual(queueSource.Id, first.Id);
			Assert.AreEqual(queueSource.Name, first.Name);
			Assert.AreEqual(queueSource.LogObjectName, first.LogObjectName);
			Assert.AreEqual(queueSource.Description, first.Description);
		}

		[Test]
		public void ShouldGetActivies()
		{
			var activityProvider = MockRepository.GenerateMock<IActivityProvider>();
			var activityViewModel = new ActivityViewModel
			{
				Id = Guid.NewGuid(),
				Name = "test1"
			};
			activityProvider.Stub(x => x.GetAllRequireSkill()).Return(new[] {activityViewModel });
			var skillRepository = MockRepository.GenerateMock<ISkillRepository>();
			var skill = SkillFactory.CreateSkill("testSkill1");
			skill.DefaultResolution = 30;
			var activity = ActivityFactory.CreateActivity(activityViewModel.Name);
			activity.SetId(activityViewModel.Id);
			skill.Activity = activity;
			skillRepository.Stub(x => x.LoadAll()).Return(new[] {skill});
			var intervalLengthFetcher = MockRepository.GenerateMock<IIntervalLengthFetcher>();
			intervalLengthFetcher.Stub(x => x.GetIntervalLength()).Return(30);
			var target = new SkillController(activityProvider, skillRepository, intervalLengthFetcher, null, null, null, MockRepository.GenerateMock<IWorkloadRepository>());
			var result  = target.Activities();

			var first = result.First();
			Assert.AreEqual(activityViewModel.Id, first.Id);
			Assert.AreEqual(activityViewModel.Name, first.Name);
		}
	}
}