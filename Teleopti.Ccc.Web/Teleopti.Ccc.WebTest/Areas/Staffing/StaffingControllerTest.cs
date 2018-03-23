using System;
using System.Web.Http.Results;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.Staffing.Controllers;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.WebTest.Areas.Staffing
{
	[DomainTest]
	public class StaffingControllerTest : ISetup
	{
		public StaffingController Target;
		public MutableNow Now;
		public FakeScenarioRepository ScenarioRepository;
		public FakeSkillRepository SkillRepository;
		public FakeActivityRepository ActivityRepository;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<StaffingController>();
		}

		[Test]
		public void ShouldHandleSkillNotFound()
		{
			ScenarioRepository.Has("Default");
			var result = (OkNegotiatedContentResult<ExportStaffingReturnObject>)Target.ExportBpo(Guid.NewGuid(), Now.UtcDateTime(), Now.UtcDateTime().AddDays(1));

			result.Content.ErrorMessage.Should().Contain("skill");
		}

		[Test]
		public void ShouldHandleEndDateBeforeStartDate()
		{
			ScenarioRepository.Has("Default");
			var result = (OkNegotiatedContentResult<ExportStaffingReturnObject>)Target.ExportBpo(Guid.NewGuid(), Now.UtcDateTime(), Now.UtcDateTime().AddDays(-1));

			result.Content.ErrorMessage.Should().Contain(Resources.BpoExportPeriodStartDateBeforeEndDate);
		}

		[Test]
		public void ShouldHandlePeriodBeingOutside()
		{
			ScenarioRepository.Has("Default");
			var result = (OkNegotiatedContentResult<ExportStaffingReturnObject>)Target.ExportBpo(Guid.NewGuid(), Now.UtcDateTime(), Now.UtcDateTime().AddDays(365));

			result.Content.ErrorMessage.Should().Contain(Resources.BpoOnlyExportPeriodBetweenDates.Substring(0,20));
		}

		[Test]
		[RemoveMeWithToggle(Toggles.Forecast_FileImport_UnifiedFormat_46585)]
		public void ShouldHandleExportBpo()
		{
			ScenarioRepository.Has("Default");
			var activity = ActivityRepository.Has("Activity");
			var skill = SkillRepository.Has("Phone", activity);
			var result = (OkNegotiatedContentResult<ExportStaffingReturnObject>)Target.ExportBpo(skill.Id.GetValueOrDefault(), Now.UtcDateTime(), Now.UtcDateTime().AddDays(1));

			result.Content.ErrorMessage.Should().Be.Empty();
			result.Content.Content.Should().Be.Empty();
		}

		[Test]
		[Toggle(Toggles.Forecast_FileImport_UnifiedFormat_46585)]
		public void ShouldHandleExportBpoUnifiedFormat()
		{
			ScenarioRepository.Has("Default");
			var activity = ActivityRepository.Has("Activity");
			var skill = SkillRepository.Has("Phone", activity);
			var result = (OkNegotiatedContentResult<ExportStaffingReturnObject>)Target.ExportBpo(skill.Id.GetValueOrDefault(), Now.UtcDateTime(), Now.UtcDateTime().AddDays(1));

			result.Content.ErrorMessage.Should().Be.Empty();
			result.Content.Content.Should().Be.Empty();
		}
	}
}