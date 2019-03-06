using System;
using log4net;
using Teleopti.Ccc.Domain.Coders;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.Payroll;
using Teleopti.Ccc.Sdk.Logic;
using DetailLevel = Teleopti.Interfaces.Domain.DetailLevel;

namespace Teleopti.Ccc.Sdk.ServiceBus.Payroll
{
	public class PayrollExportFeedback : IServiceBusPayrollExportFeedback
	{
	    private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
	    private IMessageBrokerComposite _messageBroker;
        private IPayrollResult _payrollResult;
        private JobResultProgressEncoder _payrollResultProgressEncoder = new JobResultProgressEncoder();
		private readonly IStardustJobFeedback _stardustJobFeedback;
        private static readonly ILog Logger = LogManager.GetLogger(typeof (PayrollExportFeedback));

		public PayrollExportFeedback(ICurrentUnitOfWorkFactory unitOfWorkFactory, IMessageBrokerComposite messageBroker, 
			IStardustJobFeedback stardustJobFeedback)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
			_messageBroker = messageBroker;
			_stardustJobFeedback = stardustJobFeedback;
		}

	    public void SetPayrollResult(IPayrollResult payrollResult)
        {
            _payrollResult = payrollResult;
        }
        
        public void ReportProgress(int percentage, string information)
        {
	        var payrollExportProgress = new JobResultProgress
	        {
		        Message = information,
		        Percentage = percentage,
		        JobResultId = _payrollResult.Id.GetValueOrDefault()
	        };
	        var binaryData =
		        _payrollResultProgressEncoder.Encode(payrollExportProgress);
	        
			_messageBroker.Send(_unitOfWorkFactory.Current().Name,
		        _payrollResult.GetOrFillWithBusinessUnit_DONTUSE().Id.
			        GetValueOrDefault(), DateTime.UtcNow, DateTime.UtcNow, Guid.Empty,
		        Guid.Empty, typeof (IJobResultProgress), DomainUpdateType.NotApplicable,
		        binaryData);

			_stardustJobFeedback?.SendProgress(information);
		}

	    public void Error(string message)
        {
            Error(message,null);
        }

        private void AddPayrollResultDetail(DetailLevel detailLevel, string message, Exception exception)
        {
            _payrollResult.AddDetail(new PayrollResultDetail(detailLevel.Convert(), message, DateTime.UtcNow, exception));
        }

		public void AddPayrollResultDetail(IPayrollResultDetail payrollResultDetail)
		{
			_payrollResult.AddDetail(payrollResultDetail);
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
                _payrollResult = null;
            }
        }
    }
}
