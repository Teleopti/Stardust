using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Principal
{
    public interface ILicensedFunctionsProvider
    {
        IEnumerable<IApplicationFunction> LicensedFunctions(string dataSourceName);
    }

    public class LicensedFunctionsProvider : ILicensedFunctionsProvider
    {
        private readonly IDefinedRaptorApplicationFunctionFactory _definedRaptorApplicationFunctionFactory;
        private readonly ConcurrentDictionary<string,IEnumerable<IApplicationFunction>> _licensedFunctions = new ConcurrentDictionary<string, IEnumerable<IApplicationFunction>>();

        public LicensedFunctionsProvider(IDefinedRaptorApplicationFunctionFactory definedRaptorApplicationFunctionFactory)
        {
            _definedRaptorApplicationFunctionFactory = definedRaptorApplicationFunctionFactory;
        }

        public IEnumerable<IApplicationFunction> LicensedFunctions(string dataSourceName)
        {
	        return _licensedFunctions.GetOrAdd(dataSourceName, e=>fetchLicensedFunctions(dataSourceName));
        }

		private IEnumerable<IApplicationFunction> fetchLicensedFunctions(string dataSource)
    	{
			var applicationFunctions = _definedRaptorApplicationFunctionFactory.ApplicationFunctionList.ToList();
			var licensedFunctions = new HashSet<IApplicationFunction>();
			foreach (var enabledLicenseOption in LicenseSchema.GetActiveLicenseSchema(dataSource).EnabledLicenseOptions)
			{
				enabledLicenseOption.EnableApplicationFunctions(applicationFunctions);
				enabledLicenseOption.EnabledApplicationFunctions.ForEach(f => licensedFunctions.Add(f));
			}
			return licensedFunctions;
    	}
    }
}
