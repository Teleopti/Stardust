using Autofac;
using MbCache.Configuration;
using MbCache.Core;
using MbCache.DefaultImpl;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.IocConfig
{
    public class MbCacheModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(c => createCacheFactory(c))
                            .As<IMbCacheFactory>()
                            .SingleInstance();
        }

        private static IMbCacheFactory createCacheFactory(IComponentContext context)
        {
            var cacheBuilder = new CacheBuilder();
            cacheBuilder.UseCacheForInterface<IRuleSetProjectionService>
                                    (
                                        context.Resolve<IRuleSetProjectionService>("UnCached"), 
                                        c=>c.ProjectionCollection(null)
                                    );
            return cacheBuilder.BuildFactory(new AspNetCacheFactory(20), new ToStringMbCacheRegion());
        }
    }
}