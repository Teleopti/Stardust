using System;
using System.Globalization;
using Teleopti.Ccc.Sdk.Logic.MultiTenancy;
using Teleopti.Ccc.Sdk.ServiceBus.Payroll.FormatLoader;
using log4net;
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
		private static readonly ILog Logger = LogManager.GetLogger(typeof(PayrollExportHandler));

		public static bool IsRunning { get; private set; }

		public PayrollExportHandler(ICurrentUnitOfWork currentUnitOfWork,
			IPayrollExportRepository payrollExportRepository, IPayrollResultRepository payrollResultRepository,
			IPayrollDataExtractor payrollDataExtractor, IPersonBusAssembler personBusAssembler,
			IServiceBusPayrollExportFeedback serviceBusPayrollExportFeedback,
			IPayrollPeopleLoader payrollPeopleLoader, IDomainAssemblyResolver domainAssemblyResolver,
			ITenantPeopleLoader tenantPeopleLoader)
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
		}

		public void Handle(RunPayrollExportEvent @event)
		{
			if (MessageIsNull(@event)) return;
			IsRunning = true;
			Logger.DebugFormat("Consuming message for Payroll Export with Id = {0}. (Message timestamp = {1})", @event.PayrollExportId, @event.Timestamp);
			var origPeriod = new DateOnlyPeriod(new DateOnly(@event.ExportStartDate), new DateOnly(@event.ExportEndDate));
			Logger.DebugFormat("Payroll Export period = {0})", origPeriod);
			AppDomain.CurrentDomain.AssemblyResolve += _domainAssemblyResolver.Resolve;

			var payrollExport = _payrollExportRepository.Get(@event.PayrollExportId);
			var payrollResult = _payrollResultRepository.Get(@event.PayrollResultId);

			_serviceBusPayrollExportFeedback.SetPayrollResult(payrollResult);
			_serviceBusPayrollExportFeedback.ReportProgress(1, "Payroll export initiated.");

			var people = _payrollPeopleLoader.GetPeopleForExport(@event, origPeriod, _currentUnitOfWork.Current());
			var personDtos = _personBusAssembler.CreatePersonDto(people, _tenantPeopleLoader);
			try
			{
				payrollResult.XmlResult.SetResult(_payrollDataExtractor.Extract(payrollExport, @event, personDtos,
					_serviceBusPayrollExportFeedback));
			}
			catch (Exception exception)
			{
				Logger.Error("An error occurred while running the payroll export.", exception);
				_serviceBusPayrollExportFeedback.Error(@"An error occurred while running the payroll export.", exception);
				IsRunning = false;
			}

			_serviceBusPayrollExportFeedback.ReportProgress(100, "Payroll export finished.");
			_serviceBusPayrollExportFeedback.Dispose();
			_serviceBusPayrollExportFeedback = null;
			IsRunning = false;

			AppDomain.CurrentDomain.AssemblyResolve -= _domainAssemblyResolver.Resolve;
		}

		private static bool MessageIsNull(RunPayrollExportEvent @event)
		{
			if (@event == null)
			{
				Logger.DebugFormat(CultureInfo.CurrentCulture, "Error in Consume, message is null");
				return true;
			}
			return false;
		}
	}
}
