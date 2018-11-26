using System.Web.Http;
using Autofac;
using Teleopti.Ccc.IocCommon;

namespace Teleopti.Ccc.Web.Core.IoC
{
	public class WebAppModule : Module
	{
		private readonly IocConfiguration _configuration;
		private readonly HttpConfiguration _httpConfiguration;

		public WebAppModule(IocConfiguration configuration)
		{
			_configuration = configuration;
		}

		public WebAppModule(IocConfiguration configuration, HttpConfiguration httpConfiguration)
		{
			_configuration = configuration;
			_httpConfiguration = httpConfiguration;
		}

		protected override void Load(ContainerBuilder builder)
		{
			base.Load(builder);

			builder.RegisterModule(new CommonModule(_configuration));
			builder.RegisterModule(new WebModule(_configuration, _httpConfiguration));
		}
	}
}