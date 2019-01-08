using System;
using System.IO;
using System.Reflection;

namespace Teleopti.Ccc.Sdk.ServiceBus.Payroll.FormatLoader
{
	public class AssemblyFileLoaderTenant : IAssemblyFileLoader
	{
		public Assembly Find(string assemblyName)
		{
			//var tenantSpecificFiles = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory);
			var baseFiles = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory);
			//var allFiles = new List<string>();
			//allFiles.AddRange(tenantSpecificFiles);
			//allFiles.AddRange(baseFiles);
			AssemblyName asmName = new AssemblyName(assemblyName);
			assemblyName = asmName.Name;
			foreach (var file in baseFiles)
			{
				Assembly assm;
				if (TryLoad(file, assemblyName, out assm))
					return assm;
			}
			return null;
		}

		private static bool TryLoad(string file, string assemblyName, out Assembly assm)
		{
			try
			{
				// Convert the filename into an absolute file name for                 
				// use with LoadFile.                 
				file = new FileInfo(file).FullName;
				if (AssemblyName.GetAssemblyName(file).Name.Equals(assemblyName))
				{
					assm = Assembly.LoadFile(file);
					return true;
				}
			}
			catch
			{
				/* Do Nothing */
			}
			assm = null;
			return false;
		}

	}
}