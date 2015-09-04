using System;
using System.Linq;
using Teleopti.Ccc.Domain.Payroll;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages.Payroll;

namespace Teleopti.Ccc.Sdk.WcfService.Factory
{
	public class PayrollResultFactory : IPayrollResultFactory
	{
		private readonly IMessagePopulatingServiceBusSender _serviceBusSender;
		private readonly IPayrollResultRepository _payrollResultRepository;
		private readonly IPersonRepository _personRepository;
		private readonly IPayrollExportRepository _payrollExportRepository;

		public PayrollResultFactory(IMessagePopulatingServiceBusSender serviceBusSender, IPayrollResultRepository payrollResultRepository, IPersonRepository personRepository, IPayrollExportRepository payrollExportRepository)
		{
			_serviceBusSender = serviceBusSender;
			_payrollResultRepository = payrollResultRepository;
			_personRepository = personRepository;
			_payrollExportRepository = payrollExportRepository;
		}

		public Guid RunPayrollOnBus(PayrollExportDto payrollExport)
		{
			if (payrollExport == null) throw new ArgumentNullException("payrollExport");

			var payrollResultId = SavePayrollResult(payrollExport);

			var message = new RunPayrollExport
							  {
								  PayrollExportId = payrollExport.Id.GetValueOrDefault(Guid.Empty),
								  OwnerPersonId = ((IUnsafePerson)TeleoptiPrincipal.CurrentPrincipal).Person.Id.GetValueOrDefault(Guid.Empty),
								  ExportPeriod = new DateOnlyPeriod(new DateOnly(payrollExport.DatePeriod.StartDate.DateTime), new DateOnly(payrollExport.DatePeriod.EndDate.DateTime)),
								  PayrollExportFormatId = payrollExport.PayrollFormat.FormatId,
								  PayrollResultId = payrollResultId
							  };
			if (payrollExport.ExportPersonCollection == null || payrollExport.ExportPersonCollection.Count == 0)
			{
				message.ExportPersonIdCollection =
					payrollExport.PersonCollection.Select(p => p.Id.GetValueOrDefault()).ToList();
			}
			if (payrollExport.ExportPersonCollection != null && payrollExport.ExportPersonCollection.Count > 0)
			{
				message.ExportPersonIdCollection =
					payrollExport.ExportPersonCollection;
			}

			_serviceBusSender.Send(message, true);

			return payrollResultId;
		}

		private Guid SavePayrollResult(PayrollExportDto payrollExport)
		{
			using (var unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var exportingPersonDomain = TeleoptiPrincipal.CurrentPrincipal.GetPerson(_personRepository);
				var payrollExportDomain = _payrollExportRepository.Get(payrollExport.Id.GetValueOrDefault(Guid.Empty));

				var payrollResult = GetPayrollResult(payrollExportDomain, exportingPersonDomain, DateTime.UtcNow);

				payrollResult.PayrollExport = payrollExportDomain;
				_payrollResultRepository.Add(payrollResult);
				unitOfWork.PersistAll();
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
