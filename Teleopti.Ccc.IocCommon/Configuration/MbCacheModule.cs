using Autofac;
using MbCache.Configuration;
using MbCache.Core;
using MbCache.ProxyImpl.LinFu;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public class MbCacheModule : Module
	{
		public MbCacheModule(ILockObjectGenerator lockObjectGenerator)
		{
			Builder = new CacheBuilder(new LinFuProxyFactory())
							.SetCache(new InMemoryCache(20))
							.SetCacheKey(new TeleoptiCacheKey())
							.SetLockObjectGenerator(lockObjectGenerator);
		}

		public CacheBuilder Builder { get; private set; }

		protected override void Load(ContainerBuilder builder)
		{
			builder.Register(c => Builder.BuildFactory())
				.As<IMbCacheFactory>()
				.SingleInstance();
		}
	}
}