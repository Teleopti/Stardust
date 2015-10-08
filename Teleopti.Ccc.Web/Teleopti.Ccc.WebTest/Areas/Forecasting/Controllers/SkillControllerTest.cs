using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
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
		public void ShouldGetDefaultTimezone()
		{
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			loggedOnUser.Stub(x => x.CurrentUser()).Return(PersonFactory.CreatePerson("test1"));
			var target = new SkillController(null, null, null, loggedOnUser);
			var result = target.Timezones();
			Assert.AreEqual(loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone().Id, result.DefaultTimezone);
		}

		[Test]
		public void ShouldGetTimezones()
		{
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			loggedOnUser.Stub(x => x.CurrentUser()).Return(PersonFactory.CreatePerson("test1"));
			var target = new SkillController(null, null, null, loggedOnUser);
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
			var target = new SkillController(activityProvider, skillRepository, intervalLengthFetcher, null);
			var result  = target.Activities();

			var first = result.First();
			Assert.AreEqual(activityViewModel.Id, first.Id);
			Assert.AreEqual(activityViewModel.Name, first.Name);
		}

		[Test]
		public void ShouldGetIntervalLengthFromExistingSkill()
		{
			var activityProvider = MockRepository.GenerateMock<IActivityProvider>();
			var activityViewModel = new ActivityViewModel
			{
				Id = Guid.NewGuid(),
				Name = "test1"
			};
			activityProvider.Stub(x => x.GetAll()).Return(new[] { activityViewModel });
			var skillRepository = MockRepository.GenerateMock<ISkillRepository>();
			var skill = SkillFactory.CreateSkill("testSkill1");
			skill.DefaultResolution = 30;
			var activity = ActivityFactory.CreateActivity(activityViewModel.Name);
			activity.SetId(activityViewModel.Id);
			skill.Activity = activity;
			skillRepository.Stub(x => x.LoadAll()).Return(new[] { skill });
			var intervalLengthFetcher = MockRepository.GenerateMock<IIntervalLengthFetcher>();
			intervalLengthFetcher.Stub(x => x.IntervalLength).Return(31);
			var target = new SkillController(activityProvider, skillRepository, intervalLengthFetcher, null);
			var result = target.Activities();

			var first = result.First();
			Assert.AreEqual(skill.DefaultResolution, first.IntervalLength);
		}

		[Test]
		public void ShouldGetIntervalLengthFromAnalytics()
		{
			var activityProvider = MockRepository.GenerateMock<IActivityProvider>();
			var activityViewModel = new ActivityViewModel
			{
				Id = Guid.NewGuid(),
				Name = "test1"
			};
			activityProvider.Stub(x => x.GetAll()).Return(new[] { activityViewModel });
			var skillRepository = MockRepository.GenerateMock<ISkillRepository>();
			var skill = SkillFactory.CreateSkill("testSkill1");
			skill.DefaultResolution = 30;
			skillRepository.Stub(x => x.LoadAll()).Return(new[] { skill });
			var intervalLengthFetcher = MockRepository.GenerateMock<IIntervalLengthFetcher>();
			intervalLengthFetcher.Stub(x => x.IntervalLength).Return(31);
			var target = new SkillController(activityProvider, skillRepository, intervalLengthFetcher, null);
			var result = target.Activities();

			var first = result.First();
			Assert.AreEqual(31, first.IntervalLength);
		}
	}
}