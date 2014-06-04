using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Security.Principal
{
    public interface ILicensedFunctionsProvider
    {
        IEnumerable<IApplicationFunction> LicensedFunctions();
    }

    public class LicensedFunctionsProvider : ILicensedFunctionsProvider
    {
        private readonly IDefinedRaptorApplicationFunctionFactory _definedRaptorApplicationFunctionFactory;
        private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
        private static object lockobject = new object();
    	private IEnumerable<IApplicationFunction> _licensedFunctions;

        public LicensedFunctionsProvider(IDefinedRaptorApplicationFunctionFactory definedRaptorApplicationFunctionFactory, ICurrentUnitOfWorkFactory currentUnitOfWorkFactory)
        {
            _definedRaptorApplicationFunctionFactory = definedRaptorApplicationFunctionFactory;
            _currentUnitOfWorkFactory = currentUnitOfWorkFactory;
        }

        public IEnumerable<IApplicationFunction> LicensedFunctions()
        {
			if(_licensedFunctions==null)
			{
				lock (lockobject)
				{
					if(_licensedFunctions==null)
					{
						_licensedFunctions = fetchLicensedFunctions();
					}
				}
			}
            return _licensedFunctions;
        }

		private IEnumerable<IApplicationFunction> fetchLicensedFunctions()
    	{
			var applicationFunctions = _definedRaptorApplicationFunctionFactory.ApplicationFunctionList.ToList();
			var licensedFunctions = new HashSet<IApplicationFunction>();
			foreach (var enabledLicenseOption in LicenseSchema.GetActiveLicenseSchema(_currentUnitOfWorkFactory.LoggedOnUnitOfWorkFactory().Name).EnabledLicenseOptions)
			{
				enabledLicenseOption.EnableApplicationFunctions(applicationFunctions);
				enabledLicenseOption.EnabledApplicationFunctions.ForEach(f => licensedFunctions.Add(f));
			}
			return licensedFunctions;
    	}
    }
}
