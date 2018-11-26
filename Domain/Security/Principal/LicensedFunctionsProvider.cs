using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	public interface ILicensedFunctionsProvider
	{
		IEnumerable<IApplicationFunction> LicensedFunctions(string tenantName);
		void ClearLicensedFunctions(string tenantName);
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
				IEnumerable<IApplicationFunction> functions;
				if (_licensedFunctions.TryGetValue(tenantName, out functions))
					return functions;

				functions = fetchLicensedFunctions(tenantName);
				_licensedFunctions.Add(tenantName, functions);
				return functions;
			}
		}

		public void ClearLicensedFunctions(string tenantName)
		{
			_licensedFunctions.Remove(tenantName);
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
