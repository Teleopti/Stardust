using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Principal
{
    public interface ILicensedFunctionsProvider
    {
        IEnumerable<IApplicationFunction> LicensedFunctions(string tenantName);
    }

    public class LicensedFunctionsProvider : ILicensedFunctionsProvider
    {
        private readonly IDefinedRaptorApplicationFunctionFactory _definedRaptorApplicationFunctionFactory;
        private readonly ConcurrentDictionary<string,IEnumerable<IApplicationFunction>> _licensedFunctions = new ConcurrentDictionary<string, IEnumerable<IApplicationFunction>>();

        public LicensedFunctionsProvider(IDefinedRaptorApplicationFunctionFactory definedRaptorApplicationFunctionFactory)
        {
            _definedRaptorApplicationFunctionFactory = definedRaptorApplicationFunctionFactory;
        }

        public IEnumerable<IApplicationFunction> LicensedFunctions(string tenantName)
        {
	        return _licensedFunctions.GetOrAdd(tenantName, e => fetchLicensedFunctions(tenantName));
        }

		private IEnumerable<IApplicationFunction> fetchLicensedFunctions(string tenantName)
    	{
			var applicationFunctions = _definedRaptorApplicationFunctionFactory.ApplicationFunctionList.ToList();
			var licensedFunctions = new HashSet<IApplicationFunction>();
			foreach (var enabledLicenseOption in LicenseSchema.GetActiveLicenseSchema(tenantName).EnabledLicenseOptions)
			{
				enabledLicenseOption.EnableApplicationFunctions(applicationFunctions);
				//Don't change this foreach to linq, please!
				foreach (var function in enabledLicenseOption.EnabledApplicationFunctions.ToList())
				{
					licensedFunctions.Add(function);
				}
			}
			return licensedFunctions;
    	}
    }
}
