using Autofac;
using MbCache.Configuration;
using MbCache.Core;
using MbCache.ProxyImpl.LinFu;

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
			builder.RegisterType<CachePerDataSource>().SingleInstance();
			builder.RegisterType<TeleoptiCacheKey>().SingleInstance();
			builder.RegisterType<LinFuProxyFactory>().SingleInstance();
			builder.Register(c =>
			{
				var cacheBuilder = new CacheBuilder(c.Resolve<LinFuProxyFactory>())
					.SetCacheKey(c.Resolve<TeleoptiCacheKey>())
					;
				_configuration.Cache().Build(c, cacheBuilder);
				return cacheBuilder;
			}).SingleInstance();

			builder.Register(c => c.Resolve<CacheBuilder>().BuildFactory())
				.As<IMbCacheFactory>()
				.SingleInstance();
		}
		
	}
}