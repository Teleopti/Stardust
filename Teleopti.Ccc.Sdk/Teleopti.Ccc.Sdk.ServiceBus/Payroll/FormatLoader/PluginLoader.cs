using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Teleopti.Ccc.Sdk.Common.Contracts;

namespace Teleopti.Ccc.Sdk.ServiceBus.Payroll.FormatLoader
{
    public class PlugInLoader : IPlugInLoader
    {
        private readonly IDomainAssemblyResolver _domainAssemblyResolver;
        private readonly ISearchPath _searchPath;
        private List<IPayrollExportProcessor> _availablePayrollExportProcessors;

        public PlugInLoader(IDomainAssemblyResolver domainAssemblyResolver, ISearchPath searchPath)
        {
            _domainAssemblyResolver = domainAssemblyResolver;
            _searchPath = searchPath;
        }

        public IList<IPayrollExportProcessor> Load()
        {
            if (_availablePayrollExportProcessors == null)
            {
                _availablePayrollExportProcessors = new List<IPayrollExportProcessor>();
                string[] files = Directory.GetFiles(_searchPath.Path, "*Payroll*.dll", SearchOption.AllDirectories);

                AppDomain.CurrentDomain.AssemblyResolve += _domainAssemblyResolver.Resolve;
                foreach (string file in files)
                {
                    var assembly = Assembly.LoadFile(file);
                    foreach (Type type in assembly.GetTypes())
                    {
                        if (!type.IsClass || type.IsNotPublic) continue;
                        Type[] interfaces = type.GetInterfaces();
                        if (!interfaces.Contains(typeof(IPayrollExportProcessor))) continue;
                        var obj = Activator.CreateInstance(type);
                        var t = (IPayrollExportProcessor)obj;
                        _availablePayrollExportProcessors.Add(t);
                    }
                }
                AppDomain.CurrentDomain.AssemblyResolve -= _domainAssemblyResolver.Resolve;
            }
            return _availablePayrollExportProcessors;
        }
    }
}