using System;
using System.Reflection;

namespace Teleopti.Ccc.Sdk.ServiceBus.Payroll.FormatLoader
{
    public class DomainAssemblyResolver : IDomainAssemblyResolver
    {
        private readonly IAssemblyFileLoader _assemblyFileLoader;

        public DomainAssemblyResolver(IAssemblyFileLoader assemblyFileLoader)
        {
            _assemblyFileLoader = assemblyFileLoader;
        }

        public Assembly Resolve(object sender, ResolveEventArgs args)
        {
            var currentAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            
            foreach (var t in currentAssemblies)
            {
                if (t.FullName == args.Name)
                {
                    return t;
                }
            }
            return _assemblyFileLoader.Find(args.Name);
        }
    }
}
