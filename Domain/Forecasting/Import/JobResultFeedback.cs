using System;
using log4net;
using Teleopti.Ccc.Domain.Coders;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting.Export;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.MessageBroker.Client;

namespace Teleopti.Ccc.Domain.Forecasting.Import
{
	public class JobResultFeedback : IJobResultFeedback
	{
		private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
		private IMessageBrokerComposite _messageBroker;
		private IJobResult _jobResult;
		private JobResultProgressEncoder _jobResultProgressEncoder;
		private static readonly ILog Logger = LogManager.GetLogger(typeof(JobResultFeedback));
		public event EventHandler<FeedbackEventArgs> FeedbackChanged;

		public void Clear()
		{
			_jobResult = null;
			_messageBroker = null;
			_jobResultProgressEncoder = null;
		}

		public JobResultFeedback(ICurrentUnitOfWorkFactory unitOfWorkFactory)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
		}

		public void SetJobResult(IJobResult jobResult, IMessageBrokerComposite messageBroker)
		{
			_messageBroker = messageBroker;
			_jobResult = jobResult;
			_jobResultProgressEncoder = new JobResultProgressEncoder();
		}

		public void ChangeTotalProgress(int totalPercentage)
		{
			var jobResultProgress = new JobResultProgress
			{
				Message = string.Empty,
				Percentage = 0,
				JobResultId = _jobResult.Id.GetValueOrDefault(),
				TotalPercentage = totalPercentage
			};
			OnFeedbackChanged(new FeedbackEventArgs(jobResultProgress));
			var binaryData = _jobResultProgressEncoder.Encode(jobResultProgress);
			sendMessage(binaryData);
		}

		public Guid JobId()
		{
			return _jobResult.Id.GetValueOrDefault();
		}

		public void ReportProgress(int percentage, string information)
		{
			var jobResultProgress = new JobResultProgress
			{
				Message = information,
				Percentage = percentage,
				JobResultId = _jobResult.Id.GetValueOrDefault()
			};
			OnFeedbackChanged(new FeedbackEventArgs(jobResultProgress));
			var binaryData = _jobResultProgressEncoder.Encode(jobResultProgress);
			sendMessage(binaryData);
		}

		private void sendMessage(byte[] binaryData)
		{
			_messageBroker.Send(_unitOfWorkFactory.Current().Name,
				_jobResult.GetOrFillWithBusinessUnit_DONTUSE().Id.GetValueOrDefault
					(), DateTime.UtcNow, DateTime.UtcNow, Guid.Empty, Guid.Empty,
				typeof(IJobResultProgress), DomainUpdateType.NotApplicable, binaryData);
		}

		public void Error(string message)
		{
			Error(message, null);
		}

		private void AddPayrollResultDetail(DetailLevel detailLevel, string message, Exception exception)
		{
			_jobResult.AddDetail(new JobResultDetail(detailLevel, message, DateTime.UtcNow, exception));
		}

		public void Error(string message, Exception exception)
		{
			Logger.Error(message, exception);
			AddPayrollResultDetail(DetailLevel.Error, message, exception);
		}

		public void Warning(string message)
		{
			Warning(message, null);
		}

		public void Warning(string message, Exception exception)
		{
			Logger.Warn(message, exception);
			AddPayrollResultDetail(DetailLevel.Warning, message, exception);
		}

		public void Info(string message)
		{
			Info(message, null);
		}

		public void Info(string message, Exception exception)
		{
			Logger.Info(message, exception);
			AddPayrollResultDetail(DetailLevel.Info, message, exception);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				_messageBroker = null;
				_jobResultProgressEncoder = null;
				_jobResult = null;
			}
		}

		protected virtual void OnFeedbackChanged(FeedbackEventArgs e)
		{
			var handler = FeedbackChanged;
			if (handler != null)
			{
				handler(this, e);
			}
		}
	}


}
