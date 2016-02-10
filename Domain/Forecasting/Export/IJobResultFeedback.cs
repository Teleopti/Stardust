using System;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Client.Composite;

namespace Teleopti.Ccc.Domain.Forecasting.Export
{
	public interface IJobResultFeedback : IDisposable
	{
		void ReportProgress(int percentage, string information);
		void Error(string message);
		void Error(string message, Exception exception);
		void Warning(string message);
		void Warning(string message, Exception exception);
		void Info(string message);
		void Info(string message, Exception exception);

		void SetJobResult(IJobResult jobResult, IMessageBrokerComposite messageBroker);

		void ChangeTotalProgress(int totalPercentage);
		Guid JobId();

		event EventHandler<FeedbackEventArgs> FeedbackChanged;
		void Clear();
	}

	public class FeedbackEventArgs : EventArgs
	{
		public FeedbackEventArgs(JobResultProgress jobResultDetail)
		{
			JobResultDetail = jobResultDetail;
		}

		public JobResultProgress JobResultDetail { get; private set; }
	}
}