using Autofac;

namespace Teleopti.Ccc.IocCommon.Configuration
{
    internal sealed class IocContainerImpl : IIocContainer
    {
        private readonly IContainer _container;

        public IocContainerImpl(IContainer container)
        {
            _container = container;
        }


        public T Resolve<T>()
        {
            return _container.Resolve<T>();
        }


        public void Dispose()
        {
            _container.Dispose();
        }
    }
}