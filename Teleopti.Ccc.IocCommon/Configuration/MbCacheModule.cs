using System;
using Autofac;
using MbCache.Configuration;
using MbCache.Core;
using MbCache.ProxyImpl.LinFu;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public class MbCacheModule : Module
	{
		public MbCacheModule(ICache cache)
		{
			if (cache == null)
				throw new InvalidOperationException();
			Builder = new CacheBuilder(new ProxyFactory(), cache, new TeleoptiCacheKey());
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