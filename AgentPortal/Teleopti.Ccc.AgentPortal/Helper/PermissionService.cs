#region Imports

using System;
using System.Collections;
using System.Collections.Generic;
using Teleopti.Ccc.AgentPortal.SdkServiceReference;
using Teleopti.Ccc.AgentPortal.Common;
using Teleopti.Ccc.AgentPortal.Foundation.StateHandlers;
using System.Reflection;

#endregion

namespace Teleopti.Ccc.AgentPortal.Helper
{

    /// <summary>
    /// Represents a helper class that handles the permission related 
    /// functionalties in Agent Portal.
    /// </summary>
    public class PermissionService
    {
        #region Fields - static Member

        /// <summary>
        /// holds the singolton instance of Permission Service
        /// </summary>
        private static PermissionService _instance;

        #endregion

        #region Fields - Instance Member

        private Dictionary<string, object> _authorizedFunctions;

        #endregion

        #region Methods - Static Memebers

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


        #endregion

        #region Methods - Instance  Member

        #region Methods - Instance Member - PermissionHelper Members

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
            ApplicationFunctionHelper.Initialize();
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
                return (IsPermitted(ApplicationFunctionHelper.ApplicationFunctionPaths.CreateShiftTradeRequest)) ||
                       (IsPermitted(ApplicationFunctionHelper.ApplicationFunctionPaths.CreateAbsenceRequest)) ||
                       (IsPermitted(ApplicationFunctionHelper.ApplicationFunctionPaths.CreateTextRequest));
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
            return CheckPermission(applicationFunctionPath);
        }

        /// <summary>
        /// Cehck whether Permitteds the specified application function or not.
        /// if not throws a Permission exception
        /// </summary>
        /// <param name="applicationFunctionPath">The application function path.</param>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-10-05
        /// </remarks>
        public void ValidatePermission(string applicationFunctionPath)
        {
            bool isPermitted = CheckPermission(applicationFunctionPath);
            if (!isPermitted)
            {
                throw (new PermissionException(UserTexts.Resources.InsufficientPermission));
            }
        }

        #endregion

        #region Methods - Instance  Member - PermissionHelper Members - (helpers)

        /// <summary>
        /// Checks the permission.
        /// </summary>
        /// <param name="applicationFunctionPath">The application function path.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 2008-10-30
        /// </remarks>
        private  bool CheckPermission(string applicationFunctionPath)
        {
            return _authorizedFunctions.ContainsKey(applicationFunctionPath);
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
            _authorizedFunctions = new Dictionary<string, object>();

            IList<ApplicationFunctionDto> permittedApplicationFunctionCollection =
                SdkServiceHelper.AgentServiceClient.GetApplicationFunctionsForPerson(
                    StateHolder.Instance.StateReader.SessionScopeData.LoggedOnPerson.Id);

            foreach (ApplicationFunctionDto applicationFunctionDto in permittedApplicationFunctionCollection)
            {
                if (!_authorizedFunctions.ContainsKey(applicationFunctionDto.FunctionPath))
                {
                    _authorizedFunctions.Add(applicationFunctionDto.FunctionPath, null);
                }
            }
        }
     
        #endregion

        #endregion

    }

}
