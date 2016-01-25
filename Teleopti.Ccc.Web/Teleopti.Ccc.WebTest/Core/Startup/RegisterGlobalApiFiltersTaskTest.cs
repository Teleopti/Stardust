using System;
using System.Linq;
using System.Web.Http;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Web.Core.Logging;
using Teleopti.Ccc.Web.Core.Startup;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.WebTest.Core.Startup
{
	[TestFixture]
	public class RegisterGlobalApiFiltersTaskTest
	{
		[Test]
		public void FiltersShouldBeAdded()
		{
			var config = new HttpConfiguration();
			var target = new RegisterGlobalApiFiltersTask(new FakeApiConfig(config), null);
			
			target.Execute(null);

			config.Filters.Select(item => item.Instance.GetType())
				.Should().Have.SameValuesAs(new[]
				{
					typeof(AuthorizeTeleoptiAttribute),
					typeof(Log4NetWebApiLogger)
				});
		}

		[Test]
		public void HandlersShouldBeAdded()
		{
			var config = new HttpConfiguration();
			var target = new RegisterGlobalApiFiltersTask(new FakeApiConfig(config), null);

			target.Execute(null);

			config.MessageHandlers.Select(item => item.GetType())
				.Should().Have.SameValuesAs(new[]
				{
					typeof(CancelledTaskBugWorkaroundMessageHandler)
				});
		}
	}

	public class FakeApiConfig : IGlobalConfiguration
	{
		private readonly HttpConfiguration _configuration;

		public FakeApiConfig(HttpConfiguration configuration)
		{
			_configuration = configuration;
		}

		public void Configure(Action<HttpConfiguration> configurationAction)
		{
			configurationAction(_configuration);
		}
	}
}