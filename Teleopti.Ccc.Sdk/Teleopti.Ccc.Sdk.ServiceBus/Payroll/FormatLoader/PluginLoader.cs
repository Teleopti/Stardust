using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Teleopti.Ccc.Sdk.Common.Contracts;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.ServiceBus.Payroll.FormatLoader
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "PlugIn")]
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
				foreach (var file in files)
				{
					Assembly.LoadFile(file);
				}
				foreach (var file in files)
                {
                    var assembly = Assembly.LoadFile(file);
                    foreach (var type in assembly.GetExportedTypes())
                    {
                        if (!type.IsClass || type.IsNotPublic) continue;
                        var interfaces = type.GetInterfaces();
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.Reflection.Assembly.LoadFile")]
		public IList<PayrollFormatDto> LoadDtos()
		{
			var payrollFormats = new List<PayrollFormatDto>();
			var files = Directory.GetFiles(_searchPath.Path, "*Payroll*.dll", SearchOption.AllDirectories);

			AppDomain.CurrentDomain.AssemblyResolve += _domainAssemblyResolver.Resolve;
			foreach (var file in files)
			{
				Assembly.LoadFile(file);
			}
			foreach (var file in files)
			{
				var assembly = Assembly.LoadFile(file);
				foreach (var type in assembly.GetExportedTypes())
				{
					if (!type.IsClass || type.IsNotPublic) continue;
					var interfaces = type.GetInterfaces();
					if (!interfaces.Contains(typeof(IPayrollExportProcessor))) continue;
					var obj = Activator.CreateInstance(type);
					var t = (IPayrollExportProcessor)obj;
					var strippedfile = file.Replace(_searchPath.Path, "");
					var directory = strippedfile.IndexOf("\\", StringComparison.OrdinalIgnoreCase) > 0
						? strippedfile.Substring(0, strippedfile.IndexOf("\\", StringComparison.OrdinalIgnoreCase))
						: "";
					payrollFormats.Add(new PayrollFormatDto(t.PayrollFormat.FormatId, t.PayrollFormat.Name, directory));
				}
			}
			AppDomain.CurrentDomain.AssemblyResolve -= _domainAssemblyResolver.Resolve;

			return payrollFormats;
		}
    }
}