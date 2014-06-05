using System.Collections.Generic;
using System.Linq;
using Teleopti.Analytics.Etl.Interfaces.Common;
using Teleopti.Analytics.Etl.Interfaces.PerformanceManager;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Analytics.Etl.Transformer
{
    public class PmPermissionExtractor : IPmPermissionExtractor
    {
        private readonly ILicensedFunctionsProvider _licensedFunctionsProvider;
        private bool _isPmLicenseChecked;
        private bool _isPmLicenseValid;

        public PmPermissionExtractor(ILicensedFunctionsProvider licensedFunctionsProvider)
        {
            _licensedFunctionsProvider = licensedFunctionsProvider;
        }

	    public bool IsPerformanceManagerLicenseValid(IUnitOfWorkFactory unitOfWorkFactory)
	    {
		    if (!_isPmLicenseChecked)
		    {
			    checkPmLicense(unitOfWorkFactory.Name); // Check the PM license once per instance
			    _isPmLicenseChecked = true;
		    }

		    return _isPmLicenseValid;
	    }

	    public PmPermissionType ExtractPermission(ICollection<IApplicationFunction> applicationFunctionCollection, IUnitOfWorkFactory unitOfWorkFactory)
        {
            if (!IsPerformanceManagerLicenseValid(unitOfWorkFactory))
                return PmPermissionType.None;

            bool createPermission = ApplicationFunction.FindByPath(applicationFunctionCollection, DefinedRaptorApplicationFunctionPaths.All) != null;
            if (createPermission)
                return PmPermissionType.ReportDesigner;

            createPermission = ApplicationFunction.FindByPath(applicationFunctionCollection, DefinedRaptorApplicationFunctionPaths.CreatePerformanceManagerReport) != null;
            if (createPermission)
                return PmPermissionType.ReportDesigner;

            bool viewPermission = ApplicationFunction.FindByPath(applicationFunctionCollection, DefinedRaptorApplicationFunctionPaths.ViewPerformanceManagerReport) != null;
            if (viewPermission)
                return PmPermissionType.GeneralUser;

            return PmPermissionType.None;
        }

        private void checkPmLicense(string dataSourceName)
        {
            IEnumerable<IApplicationFunction> licensedApplicationFunctions =
                _licensedFunctionsProvider.LicensedFunctions(dataSourceName);

            if (licensedApplicationFunctions.Any(
                f => f.ForeignId == DefinedRaptorApplicationFunctionForeignIds.ViewPerformanceManagerReport
                     | f.ForeignId == DefinedRaptorApplicationFunctionForeignIds.CreatePerformanceManagerReport))
            {
                _isPmLicenseValid = true;
            }
        }
    }
}