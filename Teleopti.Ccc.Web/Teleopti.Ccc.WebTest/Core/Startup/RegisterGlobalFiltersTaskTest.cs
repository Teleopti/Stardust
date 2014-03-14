using System.Linq;
using System.Web.Mvc;
using NUnit.Framework;
using SharpTestsEx;
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
			target = new RegisterGlobalFiltersTask(null,null);
			GlobalFilters.Filters.Clear();
		}

		[TearDown]
		public void Teardown()
		{
			GlobalFilters.Filters.Clear();
		}

		[Test]
		public void ShouldBeAdded()
		{
			target.Execute();
			GlobalFilters.Filters.Select(item => item.Instance.GetType())
				.Should().Have.SameValuesAs(new[]
				               	{
									typeof(CheckStartupResultAttribute),
				               		typeof(TeleoptiPrincipalAuthorizeAttribute),
				               		typeof(AjaxHandleErrorAttribute)
				               	});
		}
	}
}