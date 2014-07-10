using System;
using System.Globalization;
using Teleopti.Ccc.Sdk.ServiceBus.Payroll.FormatLoader;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Payroll;
using log4net;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Sdk.ServiceBus.Payroll
{
    public class PayrollExportConsumer : ConsumerOf<RunPayrollExport>
    {
	    private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
	    private readonly IPayrollExportRepository _payrollExportRepository;
	    private readonly IPayrollResultRepository _payrollResultRepository;
        private readonly IPayrollDataExtractor _payrollDataExtractor;
        private readonly IPersonBusAssembler _personBusAssembler;
        private IServiceBusPayrollExportFeedback _serviceBusPayrollExportFeedback;
        private readonly IPayrollPeopleLoader _payrollPeopleLoader;
    	private readonly IDomainAssemblyResolver _domainAssemblyResolver;
    	private readonly static ILog Logger = LogManager.GetLogger(typeof(PayrollExportConsumer));

    	public static bool IsRunning { get; private set; }

		public PayrollExportConsumer(ICurrentUnitOfWorkFactory unitOfWorkFactory, IPayrollExportRepository payrollExportRepository, IPayrollResultRepository payrollResultRepository, IPayrollDataExtractor payrollDataExtractor,
			IPersonBusAssembler personBusAssembler, IServiceBusPayrollExportFeedback serviceBusPayrollExportFeedback,
			IPayrollPeopleLoader payrollPeopleLoader, IDomainAssemblyResolver domainAssemblyResolver)
        {
			_unitOfWorkFactory = unitOfWorkFactory;
			_payrollExportRepository = payrollExportRepository;
			_payrollResultRepository = payrollResultRepository;
            _payrollDataExtractor = payrollDataExtractor;
            _personBusAssembler = personBusAssembler;
            _serviceBusPayrollExportFeedback = serviceBusPayrollExportFeedback;
            _payrollPeopleLoader = payrollPeopleLoader;
    		_domainAssemblyResolver = domainAssemblyResolver;
        }

        public void Consume(RunPayrollExport message)
        {
            if(MessageIsNull(message)) return;
        	IsRunning = true;
            Logger.DebugFormat("Consuming message for Payroll Export with Id = {0}. (Message timestamp = {1})", message.PayrollExportId, message.Timestamp);
            Logger.DebugFormat("Payroll Export period = {0})", message.ExportPeriod);
			AppDomain.CurrentDomain.AssemblyResolve += _domainAssemblyResolver.Resolve;
            using (var unitOfWork = _unitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork())
            {
	            var payrollExport = _payrollExportRepository.Get(message.PayrollExportId);
	            var payrollResult = _payrollResultRepository.Get(message.PayrollResultId);

	            _serviceBusPayrollExportFeedback.SetPayrollResult(payrollResult);
	            _serviceBusPayrollExportFeedback.ReportProgress(1, "Payroll export initiated.");

	            var people = _payrollPeopleLoader.GetPeopleForExport(message, message.ExportPeriod, unitOfWork);
	            var personDtos = _personBusAssembler.CreatePersonDto(people);
	            try
	            {
		            payrollResult.XmlResult.AddResult(_payrollDataExtractor.Extract(payrollExport, message, personDtos,
			            _serviceBusPayrollExportFeedback));
	            }
	            catch (Exception exception)
	            {
		            Logger.Error("An error occurred while running the payroll export.", exception);
		            _serviceBusPayrollExportFeedback.Error(@"An error occurred while running the payroll export.", exception);
		            IsRunning = false;
	            }
	            unitOfWork.PersistAll();

	            _serviceBusPayrollExportFeedback.ReportProgress(100, "Payroll export finished.");
	            _serviceBusPayrollExportFeedback.Dispose();
	            _serviceBusPayrollExportFeedback = null;
	            IsRunning = false;
            }
	        AppDomain.CurrentDomain.AssemblyResolve -= _domainAssemblyResolver.Resolve;
        }
    
        private static bool MessageIsNull(RunPayrollExport message)
        {
            if (message == null)
            {
                Logger.DebugFormat(CultureInfo.CurrentCulture, "Error in Consume, message is null");
                return true;
            }
            return false;
        }
    }
}
