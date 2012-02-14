using System.Reflection;

namespace Teleopti.Ccc.Sdk.ServiceBus.Payroll.FormatLoader
{
    public interface IAssemblyFileLoader
    {
        Assembly Find(string assemblyName);
    }
}