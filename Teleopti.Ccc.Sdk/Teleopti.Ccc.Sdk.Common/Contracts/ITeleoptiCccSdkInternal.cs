using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.ServiceModel;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;

namespace Teleopti.Ccc.Sdk.Common.Contracts
{
    /// <summary>
    /// Internal functions for Teleopti. Use these methods with caution.
    /// </summary>
    [ServiceContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/",
           Name = "TeleoptiCccSdkInternal", ConfigurationName = "Teleopti.Ccc.Sdk.Common.Contracts.ITeleoptiCccSdkInternal")]
    public interface ITeleoptiCccSdkInternal
    {
        /// <summary>
        /// Gets all available payroll formats.
        /// </summary>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate"), OperationContract]
        ICollection<PayrollFormatDto> GetPayrollFormats();

        /// <summary>
        /// Runs the payroll export specified. The dates and format from supplied PayrollExportDto will be used, but people will be loaded from database.
        /// </summary>
        /// <param name="payrollExport"></param>
        [OperationContract]
        void CreateServerPayrollExport(PayrollExportDto payrollExport);

        /// <summary>
        /// Inititalize the list with available payroll formats. Primarily used from the internal back-end services.
        /// </summary>
        /// <param name="payrollFormatDtos"></param>
        [OperationContract]
        void InitializePayrollFormats(ICollection<PayrollFormatDto> payrollFormatDtos);

        /// <summary>
        /// Gets the encrypted configuration for the available data sources.
        /// </summary>
        /// <returns></returns>
		  [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate"), OperationContract, Obsolete("This will only return an empty list and should not be used")]
        ICollection<string> GetHibernateConfigurationInternal();

        /// <summary>
        /// Force the SDK to verify license details.
        /// </summary>
        /// <returns></returns>
        /// <remarks>This method will not clear the cached license information if it exists.</remarks>
        [OperationContract]
        LicenseVerificationResultDto VerifyLicenseInternal();

        /// <summary>
        /// Transfer the session from the client to the server to remain logged on.
        /// </summary>
        /// <param name="sessionDataDto"></param>
        [OperationContract, FaultContract(typeof(FaultException)), Obsolete("Starting with version 7.1.322, the SDK is stateless and no session information is stored.")]
        void TransferSessionInternal(SessionDataDto sessionDataDto);

        /// <summary>
        /// Gets the encrypted application settings.
        /// </summary>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate"), OperationContract]
        IDictionary<string, string> GetAppSettingsInternal();

        /// <summary>
        /// Get the password policy information. The data is formatted xml.
        /// </summary>
        /// <returns>The xml data for password policy information</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate"), OperationContract]
        string GetPasswordPolicy();

		/// <summary>
		/// Executes a command derived from CommandDto and returns information about the execution.
		/// </summary>
		/// <param name="commandDto">The command derived from CommandDto.</param>
		/// <returns>The execution result details.</returns>
		[OperationContract,FaultContract(typeof(FaultException))]
		CommandResultDto ExecuteCommand(CommandDto commandDto);

        /// <summary>
        /// Queries the system after saved settings in the Agent Portal.
        /// Used internal by the Agent Portal.
        /// </summary>
        /// <param name="queryDto">The query.</param>
        /// <returns>A collection with one item with the saved settings.</returns>
        [OperationContract]
        ICollection<AgentPortalSettingsDto> GetAgentPortalSettingsByQuery(QueryDto queryDto);
    }
}
