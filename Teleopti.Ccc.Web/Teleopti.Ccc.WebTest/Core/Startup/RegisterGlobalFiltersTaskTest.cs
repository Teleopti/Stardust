﻿using System.Linq;
using System.Web.Mvc;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.Web.Core.Logging;
using Teleopti.Ccc.Web.Core.Startup;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.WebTest.Core.Startup
{
	[TestFixture]
	public class RegisterGlobalFiltersTaskTest
	{
		private RegisterGlobalFiltersTask target;

		[SetUp]
		public void Setup()
		{
			target = new RegisterGlobalFiltersTask(null,null, null, null, null, new FakeConfigReader());
			GlobalFilters.Filters.Clear();
		}

		[TearDown]
		public void Teardown()
		{
			GlobalFilters.Filters.Clear();
		}

		[Test, Ignore("blocking issue 40733")]
		public void ShouldBeAdded()
		{
			target.Execute(null);
			GlobalFilters.Filters.Select(item => item.Instance.GetType())
				.Should().Have.SameValuesAs(new[]
				               	{
				               		typeof(TeleoptiPrincipalAuthorizeAttribute),
									   typeof(CsrfFilter),
									typeof(AjaxHandleErrorAttribute),
									typeof(Log4NetMvCLogger),
									typeof(NoCacheFilterMvc)
				               	});
		}
	}
}