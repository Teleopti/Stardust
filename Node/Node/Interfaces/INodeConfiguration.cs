using System;
using System.Reflection;

namespace Stardust.Node.Interfaces
{
    public interface INodeConfiguration
    {
        Uri BaseAddress { get; }
        Assembly HandlerAssembly { get; }
        Uri ManagerLocation { get; }
        string NodeName { get; }
    }
}