using System;
using System.Xml.XPath;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.Common.Contracts
{
    /// <summary>
    /// Implement this interface to create custom payroll exports to be executed in the service bus.
    /// </summary>
    /// <remarks>Only one instance will be created of your payroll assembly. Please make sure that no data is unintentionally saved between payroll export executions.</remarks>
    public interface IPayrollExportProcessor
    {
        /// <summary>
        /// Gets the payroll format.
        /// </summary>
        /// <value>The payroll format.</value>
        /// <remarks>This property should always return the same guid and name.</remarks>
        PayrollFormatDto PayrollFormat { get; }

        /// <summary>
        /// Processes the payroll data.
        /// </summary>
        /// <param name="schedulingService">The scheduling service providing operations related to scheduling.</param>
        /// <param name="organizationService">The organisation service providing operations for organisation related data.</param>
        /// <param name="payrollExport">The payroll export.</param>
        /// <returns>
        /// Processed data as xml
        /// </returns>
        /// <remarks>This the entry point for your custom payroll export</remarks>
        IXPathNavigable ProcessPayrollData(ITeleoptiSchedulingService schedulingService, ITeleoptiOrganizationService organizationService, PayrollExportDto payrollExport);
    }

    /// <summary>
    /// Implement this interface to create custom payroll exports to be executed in the service bus. This version allows to give more feedback of what's going on inside the payroll export.
    /// </summary>
    public interface IPayrollExportProcessorWithFeedback : IPayrollExportProcessor
    {
        /// <summary>
        /// The feedback object which is used to give the user feedback during the payroll export process.
        /// </summary>
        IPayrollExportFeedback PayrollExportFeedback{ get; set; }
    }

    /// <summary>
    /// Class with operations to give the user feedback during the payroll export process.
    /// </summary>
    public interface IPayrollExportFeedback
    {
        /// <summary>
        /// Reports the progress percentage and description of the current state.
        /// </summary>
        /// <param name="percentage">Percentage between 1 and 100%</param>
        /// <param name="information">Details about the current state of the payroll export process</param>
        /// <remarks>The progress will not be saved to the database.</remarks>
        void ReportProgress(int percentage, string information);
        /// <summary>
        /// Report an error in the payroll export. Will mark the payroll result as failed.
        /// </summary>
        /// <param name="message">The details of the error.</param>
        void Error(string message);
        /// <summary>
        /// Report an error in the payroll export. Will mark the payroll result as failed.
        /// </summary>
        /// <param name="message">The details of the error.</param>
        /// <param name="exception">Exception details for the error if available.</param>
        void Error(string message, Exception exception);
        /// <summary>
        /// Report a warning in the payroll export. Will not mark the payroll result as failed.
        /// </summary>
        /// <param name="message">The details of the warning.</param>
        void Warning(string message);
        /// <summary>
        /// Report a warning in the payroll export. Will not mark the payroll result as failed.
        /// </summary>
        /// <param name="message">The details of the warning.</param>
        /// <param name="exception">Exception details for the warning if available.</param>
        void Warning(string message, Exception exception);
        /// <summary>
        /// Report information about the payroll export.
        /// </summary>
        /// <param name="message">The information to expose to the users.</param>
        void Info(string message);
        /// <summary>
        /// Report information about the payroll export.
        /// </summary>
        /// <param name="message">The information to expose to the users.</param>
        /// <param name="exception">Exception details for the information if available.</param>
        void Info(string message, Exception exception);
    }
}