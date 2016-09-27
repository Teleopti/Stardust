using System;
using System.Linq;
using System.Web.Http;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.TestCommon;
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
			var target = new RegisterGlobalApiFiltersTask(new FakeApiConfig(config), null, new FakeConfigReader());
			
			target.Execute(null);

			config.Filters.Select(item => item.Instance.GetType())
				.Should().Have.SameValuesAs(new[]
				{
					typeof(AuthorizeTeleoptiAttribute),
					typeof(CsrfFilterHttp),
					typeof(NoCacheFilterHttp),
				});
		}

	    [Test]
	    public void LoggerShouldBeAddedAsAService()
	    {
	        var config = new HttpConfiguration();
            var target = new RegisterGlobalApiFiltersTask(new FakeApiConfig(config), null, new FakeConfigReader());

            target.Execute(null);

	        config.Services.GetExceptionLoggers().Select(item => item.GetType()).Should().Have.SameValuesAs(new[]
	        {
	            typeof (Log4NetWebApiLogger)
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