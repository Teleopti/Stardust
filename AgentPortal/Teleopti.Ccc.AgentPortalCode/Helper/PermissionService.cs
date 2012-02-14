using System.Collections.Generic;
using Teleopti.Ccc.Sdk.Client.SdkServiceReference;
using Teleopti.Ccc.AgentPortalCode.Foundation.StateHandlers;

namespace Teleopti.Ccc.AgentPortalCode.Helper
{
    public interface IPermissionService
    {
        bool IsPermitted(string applicationFunctionPath);
    }

    /// <summary>
    /// Represents a helper class that handles the permission related 
    /// functionalties in Agent Portal.
    /// </summary>
    public class PermissionService : IPermissionService
    {
        private static PermissionService _instance;
        private IList<string> _authorizedFunctions;

        /// <summary>
        /// Instances this instance.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-10-30
        /// </remarks>
        public static PermissionService Instance()
        {
            if (_instance == null)
            {
                _instance = new PermissionService();
            }
            return _instance;
        }

        /// <summary>
        /// Starts the permission service.
        /// </summary>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-11-26
        /// </remarks>
        public void StartPermissionService()
        {
            LoadPermissions();
        }

        /// <summary>
        /// Gets a value indicating whether [to create any request].
        /// </summary>
        /// <value><c>true</c> if [to create any request]; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: Sachinthaw
        /// Created date: 2008-10-30
        /// </remarks>
        public bool ToCreateAnyRequest
        {
            get
            {
                return (IsPermitted(ApplicationFunctionHelper.Instance().DefinedApplicationFunctionPaths.CreateShiftTradeRequest)) ||
                       (IsPermitted(ApplicationFunctionHelper.Instance().DefinedApplicationFunctionPaths.CreateAbsenceRequest)) ||
                       (IsPermitted(ApplicationFunctionHelper.Instance().DefinedApplicationFunctionPaths.CreateTextRequest));
            }
        }

        /// <summary>
        /// Determines whether the specified application function path is permitted.
        /// </summary>
        /// <param name="applicationFunctionPath">The application function path.</param>
        /// <returns>
        /// 	<c>true</c> if the specified application function path is permitted; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-10-30
        /// </remarks>
        public bool IsPermitted(string applicationFunctionPath)
        {
            return _authorizedFunctions.Contains(applicationFunctionPath);
        }

        /// <summary>
        /// Loads the permissions.
        /// </summary>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 2008-10-30
        /// </remarks>
        private void LoadPermissions()
        {
            _authorizedFunctions = new List<string>();

            IList<ApplicationFunctionDto> permittedApplicationFunctionCollection =
                SdkServiceHelper.LogOnServiceClient.GetApplicationFunctionsForPerson(
                    StateHolder.Instance.StateReader.SessionScopeData.LoggedOnPerson);

            foreach (ApplicationFunctionDto applicationFunctionDto in permittedApplicationFunctionCollection)
            {
                if (!_authorizedFunctions.Contains(applicationFunctionDto.FunctionPath))
                {
                    _authorizedFunctions.Add(applicationFunctionDto.FunctionPath);
                }
            }
        }
    }
}
