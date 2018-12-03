using System.Web.Http.Results;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.Staffing.Controllers;


namespace Teleopti.Ccc.WebTest.Areas.Staffing
{
	[DomainTest]
	public class StaffingControllerTest : IExtendSystem, IIsolateSystem
	{
		public StaffingController Target;
		public MutableNow Now;
		public FakeScenarioRepository ScenarioRepository;
		public FakeSkillRepository SkillRepository;
		public FakeSkillGroupRepository SkillGroupRepository;
		public FakeActivityRepository ActivityRepository;
		public FakeUserUiCulture UserUiCulture;
		public FakeUserCulture UserCulture;
		private const string dateFormatter = "yyyy-MM-dd";

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeUserUiCulture>().For<IUserUiCulture>();
			isolate.UseTestDouble<FakeUserCulture>().For<IUserCulture>();
		}

		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<StaffingController>();
		}

		[Test]
		public void ShouldFailIfStartDateIsInvalidFormatForSkill()
		{
			UserCulture.IsSwedish();
			Now.Is("2018-04-24 14:30");
			ScenarioRepository.Has("Default");
			var activity = ActivityRepository.Has("Activity");
			var skill = SkillRepository.Has("Phone", activity);
			var result = (OkNegotiatedContentResult<ExportStaffingReturnObject>)Target.ExportForecastAndStaffing(skill.Id.GetValueOrDefault(), "2015-30-320", Now.UtcDateTime().AddDays(1).ToString(dateFormatter),false);

			result.Content.ErrorMessage.Should().Contain("The date formart is invalid");
			result.Content.Content.Should().Be.NullOrEmpty();
		}

		[Test]
		public void ShouldFailIfEndDateIsInvalidFormatForSkill()
		{
			UserCulture.IsSwedish();
			Now.Is("2018-04-24 14:30");
			ScenarioRepository.Has("Default");
			var activity = ActivityRepository.Has("Activity");
			var skill = SkillRepository.Has("Phone", activity);
			var result = (OkNegotiatedContentResult<ExportStaffingReturnObject>)Target.ExportForecastAndStaffing(skill.Id.GetValueOrDefault(), Now.UtcDateTime().AddDays(1).ToString(dateFormatter), "2019-30-320",false);

			result.Content.ErrorMessage.Should().Contain("The date formart is invalid");
			result.Content.Content.Should().Be.NullOrEmpty();
		}

		[Test]
		public void ShouldParseStartDateForSkill()
		{
			UserCulture.IsSwedish();
			Now.Is("2018-04-24 14:30");
			ScenarioRepository.Has("Default");
			var activity = ActivityRepository.Has("Activity");
			var skill = SkillRepository.Has("Phone", activity);
			var result = (OkNegotiatedContentResult<ExportStaffingReturnObject>)Target.ExportForecastAndStaffing(skill.Id.GetValueOrDefault(), "2018-04-20", Now.UtcDateTime().AddDays(1).ToString(dateFormatter), false);

			result.Content.ErrorMessage.Should().Be.Empty();
		}

		[Test]
		public void ShouldParseEndDateForSkill()
		{
			UserCulture.IsSwedish();
			Now.Is("2018-04-24 14:30");
			ScenarioRepository.Has("Default");
			var activity = ActivityRepository.Has("Activity");
			var skill = SkillRepository.Has("Phone", activity);
			var result = (OkNegotiatedContentResult<ExportStaffingReturnObject>)Target.ExportForecastAndStaffing(skill.Id.GetValueOrDefault(), Now.UtcDateTime().AddDays(1).ToString(dateFormatter), "2018-05-05", false);

			result.Content.ErrorMessage.Should().Be.Empty();
		}
	}
}