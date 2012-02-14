using System;
using System.Reflection;

namespace Teleopti.Ccc.Sdk.ServiceBus.Payroll.FormatLoader
{
    public interface IDomainAssemblyResolver
    {
        Assembly Resolve(object sender, ResolveEventArgs args);
    }
}