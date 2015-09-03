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
			builder.Register(c =>
			{
				var cacheKey = new TeleoptiCacheKey();
				var proxyFactory = new LinFuProxyFactory();
				var cacheBuilder = new CacheBuilder(proxyFactory)
					.SetCacheKey(cacheKey)
					;
				_configuration.Cache().Build(cacheBuilder);
				return cacheBuilder;
			}).SingleInstance();

			builder.Register(c => c.Resolve<CacheBuilder>().BuildFactory())
				.As<IMbCacheFactory>()
				.SingleInstance();
		}
		
	}
}