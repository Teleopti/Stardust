using System.Linq;
using System.Runtime.Caching;
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
				if (_configuration.Args().ClearCache)
				{
					MemoryCache.Default
						.Select(x => x.Key)
						.ToList()
						.ForEach(x => MemoryCache.Default.Remove(x));
				}
				var cacheKey = new TeleoptiCacheKey();
				var proxyFactory = new LinFuProxyFactory();
				var cacheBuilder = new CacheBuilder(proxyFactory)
					.SetCacheKey(cacheKey)
					;
				_configuration.Args().CacheRegistrations
					.ForEach(r => r.Invoke(cacheBuilder));
				return cacheBuilder;
			}).SingleInstance();

			builder.Register(c => c.Resolve<CacheBuilder>().BuildFactory())
				.As<IMbCacheFactory>()
				.SingleInstance();
		}
	}
}