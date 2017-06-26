using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;

namespace Teleopti.Ccc.Domain.Security.Principal
{
    public interface ILicensedFunctionsProvider
    {
        IEnumerable<IApplicationFunction> LicensedFunctions(string tenantName);
    }

	public class LicensedFunctionsProvider : ILicensedFunctionsProvider
	{
		private readonly IDefinedRaptorApplicationFunctionFactory _definedRaptorApplicationFunctionFactory;
		private readonly Dictionary<string, IEnumerable<IApplicationFunction>> _licensedFunctions = new Dictionary<string, IEnumerable<IApplicationFunction>>();
		private readonly object locker = new object();

		public LicensedFunctionsProvider(IDefinedRaptorApplicationFunctionFactory definedRaptorApplicationFunctionFactory)
		{
			_definedRaptorApplicationFunctionFactory = definedRaptorApplicationFunctionFactory;
		}

		public IEnumerable<IApplicationFunction> LicensedFunctions(string tenantName)
		{
			lock (locker)
			{
				if (_licensedFunctions.ContainsKey(tenantName))
					return _licensedFunctions[tenantName];
				_licensedFunctions.Add(tenantName, fetchLicensedFunctions(tenantName));
				return _licensedFunctions[tenantName];
			}
		}

		private IEnumerable<IApplicationFunction> fetchLicensedFunctions(string tenantName)
		{
			var result = new HashSet<IApplicationFunction>();
			foreach (var licenseOption in LicenseSchema.GetActiveLicenseSchema(tenantName).EnabledLicenseOptions)
			{
				licenseOption.EnableApplicationFunctions(_definedRaptorApplicationFunctionFactory.ApplicationFunctions);
				foreach (var enabledApplicationFunction in licenseOption.EnabledApplicationFunctions)
					result.Add(enabledApplicationFunction);
			}
			return result;
		}
	}
}
