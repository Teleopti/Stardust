using System;
using log4net;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting.Export;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Messaging.Coders;

namespace Teleopti.Ccc.Sdk.ServiceBus.Forecast
{
    public class JobResultFeedback : IJobResultFeedback
    {
        private IMessageBroker _messageBroker;
        private IJobResult _jobResult;
        private JobResultProgressEncoder _payrollResultProgressEncoder;
        private static readonly ILog Logger = LogManager.GetLogger(typeof(JobResultFeedback));

        public void SetJobResult(IJobResult jobResult,IMessageBroker messageBroker)
        {
        	_messageBroker = messageBroker;
            _jobResult = jobResult;
			_payrollResultProgressEncoder = new JobResultProgressEncoder();
        }
        
        public void ReportProgress(int percentage, string information)
        {
            var payrollExportProgress = new JobResultProgress
                                            {
                                                Message = information,
                                                Percentage = percentage,
                                                JobResultId = _jobResult.Id.GetValueOrDefault()
                                            };
            var binaryData =
                _payrollResultProgressEncoder.Encode(payrollExportProgress);
            using (new MessageBrokerSendEnabler())
            {
                if (MessageBrokerIsRunning())
                {
                    _messageBroker.SendEventMessage(DateTime.UtcNow, DateTime.UtcNow, Guid.Empty, Guid.Empty, typeof(IJobResultProgress), DomainUpdateType.NotApplicable, binaryData);
                }
                else
                {
                    Logger.Warn("Job export progress could not be sent because the message broker is unavailable.");
                }
            }
        }

        private bool MessageBrokerIsRunning()
        {
            return _messageBroker != null && _messageBroker.IsInitialized;
        }

        public void Error(string message)
        {
            Error(message,null);
        }

        private void AddPayrollResultDetail(DetailLevel detailLevel, string message, Exception exception)
        {
            _jobResult.AddDetail(new JobResultDetail(detailLevel, message, DateTime.UtcNow, exception));
        }

        public void Error(string message, Exception exception)
        {
            Logger.Error(message,exception);
            AddPayrollResultDetail(DetailLevel.Error,message,exception);
        }

        public void Warning(string message)
        {
            Warning(message,null);
        }

        public void Warning(string message, Exception exception)
        {
            Logger.Warn(message,exception);
            AddPayrollResultDetail(DetailLevel.Warning, message, exception);
        }

        public void Info(string message)
        {
            Info(message,null);
        }

        public void Info(string message, Exception exception)
        {
            Logger.Info(message,exception);
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
                _payrollResultProgressEncoder = null;
                _jobResult = null;
            }
        }
    }
}
