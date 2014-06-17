using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.ServiceModel;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.WcfExtensions;

namespace Teleopti.Ccc.Sdk.Common.Contracts
{
    /// <summary>
    /// Used to log on to the SDK
    /// </summary>
    /// <example>
    /// C# Example of connecting and logging on to the Teleopti SDK Service
    /// This assumes that the web service is attached as a webreference in Visual Studio
    /// For more details on how to set up the AuthenticationHeader (SoapHeader) look at the example application.
    /// <code>
    /// //Create the Service object
    /// TeleoptiCccSdkService sdkService = new TeleoptiCccLogOnService();
    /// sdkService.UseDefaultCredentials = true;    
    /// //Get the datasources
    /// ICollection&lt;DataSourceDto&gt;  availableDataSources = sdkService.GetDataSources();
    /// //Select the first one (generally choosen by a user)
    /// DataSourceDto dataSource = availableDataSources.FirstOrDefault();
    /// //Login to that datasource, and get the available business units
    /// AuthenticationResultDto result = sdkService.LogOnApplicationUser("user name", "password", dataSource);
    /// //Select the first one (generally choosen by a user)
    /// if (result.Succesful)
    /// {
    ///     BusinessUnitDto businessUnit = result.BusinessUnitCollection.FirstOrDefault();
    ///     //Set the business unit
    ///     sdkService.SetBusinessUnit(businessUnit);
    /// }
    /// </code>
    /// </example>
    [ServiceContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/",
        Name = "TeleoptiCccLogOnService",
        ConfigurationName = "Teleopti.Ccc.Sdk.Common.Contracts.ITeleoptiCccLogOnService")]
    public interface ITeleoptiCccLogOnService 
    {
        /// <summary>
        /// Transfers the session.
        /// Used when logging on after a session has died.
        /// </summary>
        /// <param name="sessionDataDto">The SessionDataDto.</param>
        [OperationContract, FaultContract(typeof(FaultException)), Obsolete("Starting with version 7.1.322, the SDK is stateless and no session information is stored.")]
        void TransferSession(SessionDataDto sessionDataDto);

        /// <summary>
        /// Gets the data sources.
        /// </summary>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate"), OperationContract]
        ICollection<DataSourceDto> GetDataSources();

        /// <summary>
        /// Logs on using windows credentials (obsolete)
        /// </summary>
        /// <param name="dataSource">The data source to logon to.</param>
        /// <remarks>Use method LogOnWindowsUser instead.</remarks>
        [OperationContract, Obsolete("Use method LogOnWindowsUser instead")]
        ICollection<BusinessUnitDto> LogOnWindows(DataSourceDto dataSource);

        /// <summary>
        /// Logs on using windows credentials
        /// </summary>
        /// <param name="dataSource">The data source to logon to.</param>
        [OperationContract]
        AuthenticationResultDto LogOnWindowsUser(DataSourceDto dataSource);

        /// <summary>
        /// Logs on using forms credentials
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        /// <param name="dataSource">The data source to logon to.</param>
        /// <remarks>Use method LogOnApplicationUser instead.</remarks>
        /// <returns></returns>
        [OperationContract, Obsolete("Use method LogOnApplicationUser instead")]
        ICollection<BusinessUnitDto> LogOnApplication(string userName, string password, DataSourceDto dataSource);

        /// <summary>
        /// Logs on using forms credentials
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        /// <param name="dataSource">The data source to logon to.</param>
        /// <returns></returns>
        [OperationContract]
        AuthenticationResultDto LogOnApplicationUser(string userName, string password, DataSourceDto dataSource);

        /// <summary>
        /// Sets the business unit and does the actual login to the system.
        /// </summary>
        /// <param name="businessUnit">The business unit.</param>
        [OperationContract, Obsolete("Starting with version 7.1.322, the SDK is stateless and no session information is stored.")]
        void SetBusinessUnit(BusinessUnitDto businessUnit);

        /// <summary>
        /// Gets the logged on person.
        /// </summary>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate"), OperationContract]
        
        PersonDto GetLoggedOnPerson();

        /// <summary>
        /// Gets the application functions for person.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        [OperationContract]
        
        ICollection<ApplicationFunctionDto> GetApplicationFunctionsForPerson(PersonDto person);

        /// <summary>
        /// Gets the defined application function paths.
        /// </summary>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate"), OperationContract]
        
        DefinedRaptorApplicationFunctionPathsDto GetDefinedApplicationFunctionPaths();

        /// <summary>
        /// Logs off the current user. Clearing the cached user data.
        /// </summary>
        [OperationContract]
        
        void LogOffUser();

        /// <summary>
        /// Determines whether this instance is authenticated.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if this instance is authenticated; otherwise, <c>false</c>.
        /// </returns>
        [OperationContract, Obsolete("Starting with version 7.1.322, the SDK is stateless and no session information is stored.")]
        bool IsAuthenticated();

        /// <summary>
        /// Changes the password.
        /// </summary>
        /// <param name="personDto">The person dto.</param>
        /// <param name="oldPassword">The old password.</param>
        /// <param name="newPassword">The new password.</param>
        /// <returns></returns>
        [OperationContract]
        
        bool ChangePassword(PersonDto personDto, string oldPassword, string newPassword);

        /// <summary>
        /// Gets the app settings.
        /// </summary>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate"), OperationContract]
        IDictionary<string, string> GetAppSettings();

        /// <summary>
        /// Gets the hibernate configuration.
        /// </summary>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate"), OperationContract]
        ICollection<string> GetHibernateConfiguration();

        /// <summary>
        /// Verifies the Teleopti WFM license.
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        LicenseVerificationResultDto VerifyLicense();

        /// <summary>
        /// Gets the message broker configuration.
        /// </summary>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate"), OperationContract]
        
        MessageBrokerDto GetMessageBrokerConfiguration();

        /// <summary>
        /// Gets the matrix report info.
        /// </summary>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate"), OperationContract]
        
        ICollection<MatrixReportInfoDto> GetMatrixReportInfo();
    }
}
