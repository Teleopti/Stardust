using Autofac;

namespace Stardust.Node.Interfaces
{
    public interface INodeStarter
    {
        void Stop();

        void Start(INodeConfiguration nodeConfiguration, IContainer container);
    }
}