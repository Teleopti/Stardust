using System;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Payroll;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Payroll;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic;

namespace Teleopti.Ccc.Sdk.WcfHost.Service.Factory
{
	public class PayrollResultFactory : IPayrollResultFactory
	{
		private readonly IPayrollResultRepository _payrollResultRepository;
		private readonly IPersonRepository _personRepository;
		private readonly IPayrollExportRepository _payrollExportRepository;
		private readonly IStardustSender _stardustSender;

		public PayrollResultFactory(IPayrollResultRepository payrollResultRepository, IPersonRepository personRepository,
			IPayrollExportRepository payrollExportRepository, IStardustSender stardustSender)
		{
			_payrollResultRepository = payrollResultRepository;
			_personRepository = personRepository;
			_payrollExportRepository = payrollExportRepository;
			_stardustSender = stardustSender;
		}

		public Guid RunPayrollOnBus(PayrollExportDto payrollExport)
		{
			if (payrollExport == null) throw new ArgumentNullException(nameof(payrollExport));

			var payrollResultId = SavePayrollResult(payrollExport);
			var personId = ((IUnsafePerson) TeleoptiPrincipal.CurrentPrincipal).Person.Id.GetValueOrDefault(Guid.Empty);
			var message = new RunPayrollExportEvent
			{
				PayrollExportId = payrollExport.Id.GetValueOrDefault(Guid.Empty),
				OwnerPersonId = personId,
				ExportStartDate = payrollExport.DatePeriod.StartDate.DateTime, ExportEndDate = payrollExport.DatePeriod.EndDate.DateTime,
				PayrollExportFormatId = payrollExport.PayrollFormat.FormatId,
				PayrollResultId = payrollResultId,
				InitiatorId = personId
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
			_stardustSender.Send(message);

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
