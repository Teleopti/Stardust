﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Http.Results;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Search.Controllers;

namespace Teleopti.Ccc.WebTest.Areas.Search
{
	[GlobalSearchTest]
	public class SearchControllerTest
	{
		public SearchController Target;
		public IToggleManager ToggleManager;
		public IApplicationRoleRepository ApplicationRoleRepository;
		public TeleoptiIdentity TeleoptiIdentity;

		[Test, SetCulture("en"), SetUICulture("en")]
		public void ShouldSearchForPlanningPeriod()
		{
			var person = PersonFactory.CreatePerson();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
			Thread.CurrentPrincipal = new TeleoptiPrincipal(TeleoptiIdentity, person);
			var result = (OkNegotiatedContentResult<IEnumerable<SearchResultModel>>)Target.GetResult("Next");
			result.Content.Count().Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldNotSearchIfSchedulingIsDisabled()
		{
			((FakeToggleManager)ToggleManager).Disable(Toggles.Wfm_ResourcePlanner_32892);
			var result = (OkNegotiatedContentResult<IEnumerable<SearchResultModel>>)Target.GetResult("Next");
			result.Content.Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldSearchPermissionRole()
		{
			var role = ApplicationRoleFactory.CreateRole("Admin Role", "this is an admin role");
			ApplicationRoleRepository.Add(role);
			var result = (OkNegotiatedContentResult<IEnumerable<SearchResultModel>>)Target.GetResult("role");
			result.Content.Count().Should().Be.EqualTo(1); 
		}
	}
}
