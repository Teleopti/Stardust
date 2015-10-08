using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Forecasting.Controllers;
using Teleopti.Ccc.Web.Core.Data;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

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
			intervalLengthFetcher.Stub(x => x.IntervalLength).Return(14);
			var queueSourceRepository = MockRepository.GenerateMock<IQueueSourceRepository>();
			var activityRepository = MockRepository.GenerateMock<IActivityRepository>();
			var skillTypeProvider = MockRepository.GenerateMock<ISkillTypeProvider>();
			skillTypeProvider.Stub(x=>x.InboundTelephony()).Return(new SkillTypePhone(new Description("Skill type"), ForecastSource.InboundTelephony));
			var workloadRepository = MockRepository.GenerateMock<IWorkloadRepository>();
			var target = new SkillController(null, skillRepository, intervalLengthFetcher, null, queueSourceRepository, activityRepository, skillTypeProvider, workloadRepository);
			var input = new SkillInput
			{
				Name = "test1",
				ActivityId = Guid.NewGuid(),
				TimezoneId = TimeZoneInfo.Utc.Id,
				Queues = new[] {Guid.NewGuid()}
			};
			var queueSource = new QueueSource("testQ","",1);
			queueSource.SetId(input.Queues[0]);
			queueSourceRepository.Stub(x => x.LoadAll()).Return(new IQueueSource[] {queueSource});
			var activity = ActivityFactory.CreateActivity("test1");
			activity.SetId(input.ActivityId);
			activityRepository.Stub(x => x.Load(input.ActivityId)).Return(activity);

			target.Create(input);

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
			workloadRepository.AssertWasCalled(x=>x.Add(Arg<IWorkload>.Is.Anything));
		}

		[Test]
		public void ShouldGetIntervalLengthFromExistingSkill()
		{
			var input = new SkillInput
			{
				Name = "test1",
				ActivityId = Guid.NewGuid(),
				TimezoneId = TimeZoneInfo.Utc.Id,
				Queues = new[] { Guid.NewGuid() }
			};
			var activity = ActivityFactory.CreateActivity("test1");
			activity.SetId(input.ActivityId);
			var skillRepository = MockRepository.GenerateMock<ISkillRepository>();
			var existingSkill = SkillFactory.CreateSkill("testSkill1");
			existingSkill.DefaultResolution = 16;
			existingSkill.Activity = activity;
			skillRepository.Stub(x => x.LoadAll()).Return(new[] { existingSkill });
			var intervalLengthFetcher = MockRepository.GenerateMock<IIntervalLengthFetcher>();
			intervalLengthFetcher.Stub(x => x.IntervalLength).Return(14);
			var queueSourceRepository = MockRepository.GenerateMock<IQueueSourceRepository>();
			var activityRepository = MockRepository.GenerateMock<IActivityRepository>();
			var queueSource = new QueueSource("testQ", "", 1);
			queueSource.SetId(input.Queues[0]);
			queueSourceRepository.Stub(x => x.LoadAll()).Return(new IQueueSource[] { queueSource });
			activityRepository.Stub(x => x.Load(input.ActivityId)).Return(activity);

			var skillTypeProvider = MockRepository.GenerateMock<ISkillTypeProvider>();
			skillTypeProvider.Stub(x => x.InboundTelephony()).Return(new SkillTypePhone(new Description("Skill type"), ForecastSource.InboundTelephony));
			var target = new SkillController(null, skillRepository, intervalLengthFetcher, null, queueSourceRepository, activityRepository, skillTypeProvider, MockRepository.GenerateMock<IWorkloadRepository>());

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
			var target = new SkillController(null, null, null, null, queueSourceRepository, null, null, MockRepository.GenerateMock<IWorkloadRepository>());
			var first = target.Queues().First();
			Assert.AreEqual(queueSource.Id, first.Id);
			Assert.AreEqual(queueSource.Name, first.Name);
			Assert.AreEqual(queueSource.LogObjectName, first.LogObjectName);
			Assert.AreEqual(queueSource.Description, first.Description);
		}

		[Test]
		public void ShouldGetDefaultTimezone()
		{
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			loggedOnUser.Stub(x => x.CurrentUser()).Return(PersonFactory.CreatePerson("test1"));
			var target = new SkillController(null, null, null, loggedOnUser, null, null, null, MockRepository.GenerateMock<IWorkloadRepository>());
			var result = target.Timezones();
			Assert.AreEqual(loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone().Id, result.DefaultTimezone);
		}

		[Test]
		public void ShouldGetTimezones()
		{
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			loggedOnUser.Stub(x => x.CurrentUser()).Return(PersonFactory.CreatePerson("test1"));
			var target = new SkillController(null, null, null, loggedOnUser, null, null, null, MockRepository.GenerateMock<IWorkloadRepository>());
			var timezones = TimeZoneInfo.GetSystemTimeZones();
			var result = target.Timezones();
			Assert.AreEqual(timezones.Count, ((IEnumerable<dynamic>)result.Timezones).Count());
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
			activityProvider.Stub(x => x.GetAll()).Return(new[] {activityViewModel });
			var skillRepository = MockRepository.GenerateMock<ISkillRepository>();
			var skill = SkillFactory.CreateSkill("testSkill1");
			skill.DefaultResolution = 30;
			var activity = ActivityFactory.CreateActivity(activityViewModel.Name);
			activity.SetId(activityViewModel.Id);
			skill.Activity = activity;
			skillRepository.Stub(x => x.LoadAll()).Return(new[] {skill});
			var intervalLengthFetcher = MockRepository.GenerateMock<IIntervalLengthFetcher>();
			intervalLengthFetcher.Stub(x => x.IntervalLength).Return(30);
			var target = new SkillController(activityProvider, skillRepository, intervalLengthFetcher, null, null, null, null, MockRepository.GenerateMock<IWorkloadRepository>());
			var result  = target.Activities();

			var first = result.First();
			Assert.AreEqual(activityViewModel.Id, first.Id);
			Assert.AreEqual(activityViewModel.Name, first.Name);
		}
	}
}