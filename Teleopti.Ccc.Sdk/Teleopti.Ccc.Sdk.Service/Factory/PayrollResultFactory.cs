using System;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Payroll;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages.Payroll;

namespace Teleopti.Ccc.Sdk.WcfService.Factory
{
	public class PayrollResultFactory : IPayrollResultFactory
    {
		private readonly IServiceBusEventPublisher _serviceBusSender;

		public PayrollResultFactory(IServiceBusEventPublisher serviceBusSender)
        {
            _serviceBusSender = serviceBusSender;
        }

        public Guid RunPayrollOnBus(PayrollExportDto payrollExport)
        {
            if (payrollExport == null) throw new ArgumentNullException("payrollExport");

            var payrollResultId = SavePayrollResult(payrollExport);

            if (_serviceBusSender.EnsureBus())
            {
                var message = new RunPayrollExport
                                  {
                                      PayrollExportId = payrollExport.Id.GetValueOrDefault(Guid.Empty),
                                      OwnerPersonId = ((IUnsafePerson)TeleoptiPrincipal.Current).Person.Id.GetValueOrDefault(Guid.Empty),
                                      ExportPeriod = new DateOnlyPeriod(new DateOnly(payrollExport.DatePeriod.StartDate.DateTime), new DateOnly(payrollExport.DatePeriod.EndDate.DateTime)),
                                      PayrollExportFormatId = payrollExport.PayrollFormat.FormatId,
                                      PayrollResultId = payrollResultId
                                  };
                if (payrollExport.ExportPersonCollection == null || payrollExport.ExportPersonCollection.Count==0)
                {
                    message.ExportPersonIdCollection =
                        payrollExport.PersonCollection.Select(p => p.Id.GetValueOrDefault()).ToList();
                }
                if (payrollExport.ExportPersonCollection != null && payrollExport.ExportPersonCollection.Count>0)
                {
                    message.ExportPersonIdCollection =
                        payrollExport.ExportPersonCollection;
                }

                _serviceBusSender.Publish(message);
            }
            return payrollResultId;
        }

        private static Guid SavePayrollResult(PayrollExportDto payrollExport)
        {
            using (var unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                IRepositoryFactory repositoryFactory = new RepositoryFactory();
                var personRepository = repositoryFactory.CreatePersonRepository(unitOfWork);
                var exportingPersonDomain = TeleoptiPrincipal.Current.GetPerson(personRepository);
  
                var rep = repositoryFactory.CreatePayrollExportRepository(unitOfWork);
                var payrollExportDomain = rep.Get(payrollExport.Id.GetValueOrDefault(Guid.Empty));

                var payrollResult = GetPayrollResult(payrollExportDomain, exportingPersonDomain, DateTime.UtcNow);

                var payrollResultRepository = repositoryFactory.CreatePayrollResultRepository(unitOfWork);

                payrollResult.PayrollExport = payrollExportDomain;
                payrollResultRepository.Add(payrollResult);

                using (new MessageBrokerSendEnabler())
                {
                    unitOfWork.PersistAll();
                }
                return payrollResult.Id.GetValueOrDefault();
            }
        }

        private static IPayrollResult GetPayrollResult(IPayrollExport payrollExportInfo, IPerson owner, DateTime dateTime)
        {
            IPayrollResult payrollResult = new PayrollResult(payrollExportInfo, owner, dateTime);
            return payrollResult;
        }
    }
}
