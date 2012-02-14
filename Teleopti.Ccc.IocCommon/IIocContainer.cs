using System;

namespace Teleopti.Ccc.IocCommon
{
    public interface IIocContainer : IDisposable
    {
        T Resolve<T>();
    }
}