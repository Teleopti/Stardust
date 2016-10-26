using System.IO;
using System.Reflection;

namespace Teleopti.Ccc.Sdk.ServiceBus.Payroll.FormatLoader
{
    public class AssemblyFileLoader : IAssemblyFileLoader
    {
	    private readonly ISearchPath _searchPath;

	    public AssemblyFileLoader(ISearchPath searchPath)
	    {
		    _searchPath = searchPath;
	    }

	    public Assembly Find(string assemblyName)
        {
			foreach (var file in Directory.GetFiles(_searchPath.Path))
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
                if (AssemblyName.GetAssemblyName(file).FullName == assemblyName)
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