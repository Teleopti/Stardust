using System;
using Teleopti.Ccc.Sdk.Logic.MultiTenancy;
using Teleopti.Ccc.Sdk.ServiceBus.Payroll.FormatLoader;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Payroll;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBus.Payroll
{
	public class PayrollExportHandler : IHandleEvent<RunPayrollExportEvent>, IRunOnStardust
	{
		private readonly ICurrentUnitOfWork _currentUnitOfWork;
		private readonly IPayrollExportRepository _payrollExportRepository;
		private readonly IPayrollResultRepository _payrollResultRepository;
		private readonly IPayrollDataExtractor _payrollDataExtractor;
		private readonly IPersonBusAssembler _personBusAssembler;
		private  IServiceBusPayrollExportFeedback _serviceBusPayrollExportFeedback;
		private readonly IPayrollPeopleLoader _payrollPeopleLoader;
		private readonly IDomainAssemblyResolver _domainAssemblyResolver;
		private readonly ITenantPeopleLoader _tenantPeopleLoader;
		private readonly IStardustJobFeedback _stardustJobFeedback;		

		public PayrollExportHandler(ICurrentUnitOfWork currentUnitOfWork,
			IPayrollExportRepository payrollExportRepository, IPayrollResultRepository payrollResultRepository,
			IPayrollDataExtractor payrollDataExtractor, IPersonBusAssembler personBusAssembler,
			IServiceBusPayrollExportFeedback serviceBusPayrollExportFeedback,
			IPayrollPeopleLoader payrollPeopleLoader, IDomainAssemblyResolver domainAssemblyResolver,
			ITenantPeopleLoader tenantPeopleLoader, IStardustJobFeedback stardustJobFeedback)
		{
			_currentUnitOfWork = currentUnitOfWork;
			_payrollExportRepository = payrollExportRepository;
			_payrollResultRepository = payrollResultRepository;
			_payrollDataExtractor = payrollDataExtractor;
			_personBusAssembler = personBusAssembler;
			_serviceBusPayrollExportFeedback = serviceBusPayrollExportFeedback;
			_payrollPeopleLoader = payrollPeopleLoader;
			_domainAssemblyResolver = domainAssemblyResolver;
			_tenantPeopleLoader = tenantPeopleLoader;
			_stardustJobFeedback = stardustJobFeedback;
			
		}

		public void Handle(RunPayrollExportEvent @event)
		{
			_stardustJobFeedback.SendProgress($@"Consuming message for Payroll Export with Id = {@event.PayrollExportId}. (Message timestamp = {@event.Timestamp})");

			var origPeriod = new DateOnlyPeriod(new DateOnly(@event.ExportStartDate), new DateOnly(@event.ExportEndDate));
			_stardustJobFeedback.SendProgress($@"Payroll Export period = {origPeriod}");

			AppDomain.CurrentDomain.AssemblyResolve += _domainAssemblyResolver.Resolve;

			var payrollExport = _payrollExportRepository.Get(@event.PayrollExportId);
			var payrollResult = _payrollResultRepository.Get(@event.PayrollResultId);

			_serviceBusPayrollExportFeedback.SetPayrollResult(payrollResult);

			_serviceBusPayrollExportFeedback.ReportProgress(1, "Payroll export initiated.");

			var people = _payrollPeopleLoader.GetPeopleForExport(@event, origPeriod, _currentUnitOfWork.Current());
			var personDtos = _personBusAssembler.CreatePersonDto(people, _tenantPeopleLoader);
			try
			{
				var result = _payrollDataExtractor.Extract(payrollExport, @event, personDtos, _serviceBusPayrollExportFeedback);
				if(result != null)
					payrollResult.XmlResult.SetResult(result);
			}
			catch (Exception exception)
			{
				_serviceBusPayrollExportFeedback.Error(@"An error occurred while running the payroll export.", exception);
				throw;
			}
			finally
			{
				payrollResult.FinishedOk = true;
				
				_serviceBusPayrollExportFeedback.Dispose();
				_serviceBusPayrollExportFeedback = null;
	
				AppDomain.CurrentDomain.AssemblyResolve -= _domainAssemblyResolver.Resolve;
			}
		}
	}
}
