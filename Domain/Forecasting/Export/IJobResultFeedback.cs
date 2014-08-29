using System;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Client.Composite;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.Domain.Forecasting.Export
{
    public interface IJobResultFeedback : IDisposable
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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Error")]
		void Error(string message);

        /// <summary>
        /// Report an error in the payroll export. Will mark the payroll result as failed.
        /// </summary>
        /// <param name="message">The details of the error.</param>
        /// <param name="exception">Exception details for the error if available.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Error")]
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

		void SetJobResult(IJobResult jobResult, IMessageBroker messageBroker);

        void ChangeTotalProgress(int totalPercentage);
        Guid JobId();
    }
}