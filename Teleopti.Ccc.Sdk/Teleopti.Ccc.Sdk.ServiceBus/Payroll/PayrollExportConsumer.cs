using System;
using System.Globalization;
using log4net;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Sdk.ServiceBus.Payroll
{
    public class PayrollExportConsumer : ConsumerOf<RunPayrollExport>
    {
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly IPayrollDataExtractor _payrollDataExtractor;
        private readonly IPersonBusAssembler _personBusAssembler;
        private IServiceBusPayrollExportFeedback _serviceBusPayrollExportFeedback;
        private readonly IPayrollPeopleLoader _payrollPeopleLoader;
        private readonly static ILog Logger = LogManager.GetLogger(typeof(PayrollExportConsumer));

        public PayrollExportConsumer(IRepositoryFactory repositoryFactory, IPayrollDataExtractor payrollDataExtractor, IPersonBusAssembler personBusAssembler, IServiceBusPayrollExportFeedback serviceBusPayrollExportFeedback, IPayrollPeopleLoader payrollPeopleLoader)
        {
            _repositoryFactory = repositoryFactory;
            _payrollDataExtractor = payrollDataExtractor;
            _personBusAssembler = personBusAssembler;
            _serviceBusPayrollExportFeedback = serviceBusPayrollExportFeedback;
            _payrollPeopleLoader = payrollPeopleLoader;
        }

        public void Consume(RunPayrollExport message)
        {
            if(MessageIsNull(message)) return;

            Logger.DebugFormat("Consuming message for Payroll Export with Id = {0}. (Message timestamp = {1})", message.PayrollExportId, message.Timestamp);
            Logger.DebugFormat("Payroll Export period = {0})", message.ExportPeriod);

            using (var unitOfWork = UnitOfWorkFactoryContainer.Current.CreateAndOpenUnitOfWork())
            {
                var exportingPerson = _payrollPeopleLoader.GetOwningPerson(message,unitOfWork);
                
                var rep = _repositoryFactory.CreatePayrollExportRepository(unitOfWork);
                var payrollExport = rep.Get(message.PayrollExportId);

                var repResult = _repositoryFactory.CreatePayrollResultRepository(unitOfWork);
                var payrollResult = repResult.Get(message.PayrollResultId);

                _serviceBusPayrollExportFeedback.SetPayrollResult(payrollResult);
                _serviceBusPayrollExportFeedback.ReportProgress(1,"Payroll export initiated.");

                var people = _payrollPeopleLoader.GetPeopleForExport(message, message.ExportPeriod, unitOfWork);
                var personDtos = _personBusAssembler.CreatePersonDto(people);
                try
                {
                    payrollResult.XmlResult.AddResult(_payrollDataExtractor.Extract(payrollExport, message, personDtos, _serviceBusPayrollExportFeedback));
                }
                catch (Exception exception)
                {
                    Logger.Error("An error occurred while running the payroll export.",exception);
                    _serviceBusPayrollExportFeedback.Error(@"An error occurred while running the payroll export.", exception);
                }
                
                using (new MessageBrokerSendEnabler())
                {
                    unitOfWork.PersistAll();
                }
     
                _serviceBusPayrollExportFeedback.ReportProgress(100, "Payroll export finished.");
                _serviceBusPayrollExportFeedback.Dispose();
                _serviceBusPayrollExportFeedback = null;
            }
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
