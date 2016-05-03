
using System.Web.Http;
using Autofac;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.IocCommon;

namespace Teleopti.Ccc.Web.Core.IoC
{
	public class WebAppModule : Module
	{
		private readonly IIocConfiguration _configuration;
		private readonly HttpConfiguration _httpConfiguration;

		public WebAppModule(IIocConfiguration configuration)
		{
			_configuration = configuration;
		}

		public WebAppModule(IIocConfiguration configuration, HttpConfiguration httpConfiguration)
		{
			_configuration = configuration;
			_httpConfiguration = httpConfiguration;
		}

		protected override void Load(ContainerBuilder builder)
		{
			base.Load(builder);
			if (_configuration.Toggle(Toggles.WebByPassDefaultPermissionCheck_37984))
			{
				_configuration.Args().WebByPassDefaultPermissionCheck_37984 = true;
			}

			builder.RegisterModule(new CommonModule(_configuration));
			builder.RegisterModule(new WebModule(_configuration, _httpConfiguration));
		}
	}
}