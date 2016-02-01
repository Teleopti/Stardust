using Autofac;

namespace Stardust.Node.Interfaces
{
    public interface INodeStarter
    {
        void Start(INodeConfiguration nodeConfiguration, IContainer container);
    }
}