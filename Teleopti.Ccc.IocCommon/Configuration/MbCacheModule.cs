using Autofac;
using MbCache.Core;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class MbCacheModule : Module
	{
		private readonly IIocConfiguration _configuration;

		public MbCacheModule(IIocConfiguration configuration)
		{
			_configuration = configuration;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.Register(c => _configuration.Args().CacheBuilder.BuildFactory())
				.As<IMbCacheFactory>()
				.SingleInstance();
		}
	}
}