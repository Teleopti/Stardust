using System;
using System.Web.Http.Results;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.Staffing.Controllers;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Staffing
{
	[DomainTest]
	public class BpoControllerTest : IExtendSystem, IIsolateSystem
	{
		public BpoController Target;
		public MutableNow Now;
		public FakeScenarioRepository ScenarioRepository;
		public FakeSkillRepository SkillRepository;
		public FakeActivityRepository ActivityRepository;
		public FakeUserUiCulture UserUiCulture;
		public FakeUserCulture UserCulture;
		
		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeUserUiCulture>().For<IUserUiCulture>();
			isolate.UseTestDouble<FakeUserCulture>().For<IUserCulture>();
		}
		
		public void Extend(IExtend extend, IIocConfiguration configuration)
		{
			extend.AddService<BpoController>();
		}

		[Test]
		public void ShouldHandleSkillNotFound()
		{
			ScenarioRepository.Has("Default");
			var result = (OkNegotiatedContentResult<ExportStaffingReturnObject>)Target.ExportBpo(Guid.NewGuid(), Now.UtcDateTime(), Now.UtcDateTime().AddDays(1));

			result.Content.ErrorMessage.Should().Contain("skill");
		}

		[Test]
		[SetUICulture("sv-SE")]
		public void ShouldHandleEndDateBeforeStartDate()
		{
			ScenarioRepository.Has("Default");
			var result = (OkNegotiatedContentResult<ExportStaffingReturnObject>)Target.ExportBpo(Guid.NewGuid(), Now.UtcDateTime(), Now.UtcDateTime().AddDays(-1));

			result.Content.ErrorMessage.Should().Contain(Resources.BpoExportPeriodStartDateBeforeEndDate);
		}

		[Test]
		[SetUICulture("sv-SE")]
		public void ShouldHandlePeriodBeingOutside()
		{
			ScenarioRepository.Has("Default");
			var result = (OkNegotiatedContentResult<ExportStaffingReturnObject>)Target.ExportBpo(Guid.NewGuid(), Now.UtcDateTime(), Now.UtcDateTime().AddDays(365));

			result.Content.ErrorMessage.Should().Contain(Resources.BpoOnlyExportPeriodBetweenDates.Substring(0,20));
		}
	  
		[Test]
		public void ShouldHandleExportBpoUnifiedFormat()
		{
			ScenarioRepository.Has("Default");
			var activity = ActivityRepository.Has("Activity");
			var skill = SkillRepository.Has("Phone", activity);
			var result = (OkNegotiatedContentResult<ExportStaffingReturnObject>)Target.ExportBpo(skill.Id.GetValueOrDefault(), Now.UtcDateTime(), Now.UtcDateTime().AddDays(1));

			result.Content.ErrorMessage.Should().Be.Empty();
			result.Content.Content.Should().Be.Empty();
		}
		
		[Test]		
		public void ShouldFailWhenUsingPastDates()
		{
			UserCulture.IsSwedish();
			Now.Is("2018-04-24 14:30");
			ScenarioRepository.Has("Default");
			var activity = ActivityRepository.Has("Activity");
			var skill = SkillRepository.Has("Phone", activity);
			var result = (OkNegotiatedContentResult<ExportStaffingReturnObject>)Target.ExportBpo(skill.Id.GetValueOrDefault(), Now.UtcDateTime().AddDays(-1), Now.UtcDateTime().AddDays(1));

			result.Content.ErrorMessage.Should().Contain("2018-04-24");
			result.Content.Content.Should().Be.NullOrEmpty();
		}
	}

}