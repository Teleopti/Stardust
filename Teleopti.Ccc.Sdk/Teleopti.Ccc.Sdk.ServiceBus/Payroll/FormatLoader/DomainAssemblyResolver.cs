using System;
using System.Reflection;

namespace Teleopti.Ccc.Sdk.ServiceBus.Payroll.FormatLoader
{
    public class DomainAssemblyResolverNew : IDomainAssemblyResolver
    {
        private readonly IAssemblyFileLoader _assemblyFileLoader;

        public DomainAssemblyResolverNew(IAssemblyFileLoader assemblyFileLoader)
        {
            _assemblyFileLoader = assemblyFileLoader;
        }

        public Assembly Resolve(object sender, ResolveEventArgs args)
        {
            var currentAssemblies = AppDomain.CurrentDomain.GetAssemblies();
			//var argsShortAssemblyName = new AssemblyName(args.Name);
			foreach (var t in currentAssemblies)
            {
				//var tShortName = new AssemblyName(t.FullName);

				//if (t.GetName().Name == args.RequestingAssembly.GetName().Name)
				//if(tShortName.Name == argsShortAssemblyName.Name)
				
				if (new AssemblyName(t.FullName).Name == new AssemblyName(args.Name).Name)
				{
					return t;
                }
            }
			
			return _assemblyFileLoader.Find(args.Name);
        }
    }

	public class DomainAssemblyResolverOld : IDomainAssemblyResolver
	{
		private readonly IAssemblyFileLoader _assemblyFileLoader;

		public DomainAssemblyResolverOld(IAssemblyFileLoader assemblyFileLoader)
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
