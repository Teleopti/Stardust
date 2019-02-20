using System;
using System.Collections.Generic;
using log4net;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Payroll;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.MultiTenancy;
using Teleopti.Ccc.Sdk.ServiceBus.Payroll.FormatLoader;
using Teleopti.Wfm.Azure.Common;
using DateOnly = Teleopti.Interfaces.Domain.DateOnly;
using DateOnlyPeriod = Teleopti.Interfaces.Domain.DateOnlyPeriod;

namespace Teleopti.Ccc.Sdk.ServiceBus.Payroll
{
	public class PayrollExportHandlerNew : IHandleEvent<RunPayrollExportEvent>, IRunOnStardust
	{
		private readonly ILog logger = LogManager.GetLogger(typeof(PayrollExportHandlerNew));
		private readonly ICurrentUnitOfWork _currentUnitOfWork;
		private readonly IPayrollExportRepository _payrollExportRepository;
		private readonly IPayrollResultRepository _payrollResultRepository;
		private readonly IPersonBusAssembler _personBusAssembler;
		private IServiceBusPayrollExportFeedback _serviceBusPayrollExportFeedback;
		private readonly IPayrollPeopleLoader _payrollPeopleLoader;
		private readonly IDomainAssemblyResolver _domainAssemblyResolver;
		private readonly ITenantPeopleLoader _tenantPeopleLoader;
		private readonly IStardustJobFeedback _stardustJobFeedback;
		private readonly IBusinessUnitScope _businessUnitScope;
		private readonly IBusinessUnitRepository _businessUnitRepository;
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private readonly ISdkServiceFactory _sdkServiceFactory;
		private readonly IInstallationEnvironment _installationEnvironment;

		public PayrollExportHandlerNew(ICurrentUnitOfWork currentUnitOfWork,
			IPayrollExportRepository payrollExportRepository, IPayrollResultRepository payrollResultRepository, IPersonBusAssembler personBusAssembler,
			IServiceBusPayrollExportFeedback serviceBusPayrollExportFeedback,
			IPayrollPeopleLoader payrollPeopleLoader, IDomainAssemblyResolver domainAssemblyResolver,
			ITenantPeopleLoader tenantPeopleLoader, IStardustJobFeedback stardustJobFeedback, IBusinessUnitScope businessUnitScope, 
			IBusinessUnitRepository businessUnitRepository, ICurrentUnitOfWorkFactory currentUnitOfWorkFactory, ISdkServiceFactory sdkServiceFactory, 
			IInstallationEnvironment installationEnvironment
			)
		{
			_currentUnitOfWork = currentUnitOfWork;
			_payrollExportRepository = payrollExportRepository;
			_payrollResultRepository = payrollResultRepository;
			_personBusAssembler = personBusAssembler;
			_serviceBusPayrollExportFeedback = serviceBusPayrollExportFeedback;
			_payrollPeopleLoader = payrollPeopleLoader;
			_domainAssemblyResolver = domainAssemblyResolver;
			_tenantPeopleLoader = tenantPeopleLoader;
			_stardustJobFeedback = stardustJobFeedback;
			_businessUnitScope = businessUnitScope;
			_businessUnitRepository = businessUnitRepository;
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
			_sdkServiceFactory = sdkServiceFactory;
			_installationEnvironment = installationEnvironment;
		}

		[AsSystem]
		public virtual void Handle(RunPayrollExportEvent @event)
		{
			using (var uow = _currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				try
				{
					_stardustJobFeedback.SendProgress(
						$@"Received message for Payroll Export with Id = {
								@event.PayrollExportId
							}. (Message timestamp = {@event.Timestamp})");

					var origPeriod = new DateOnlyPeriod(new DateOnly(@event.ExportStartDate), new DateOnly(@event.ExportEndDate));
					_stardustJobFeedback.SendProgress($@"Payroll Export period = {origPeriod}");

					var payrollExport = _payrollExportRepository.Get(@event.PayrollExportId);
					var payrollResult = _payrollResultRepository.Get(@event.PayrollResultId);

					_serviceBusPayrollExportFeedback.SetPayrollResult(payrollResult);

					_serviceBusPayrollExportFeedback.ReportProgress(1, "Payroll export initiated.");

					_stardustJobFeedback.SendProgress("Payroll export initiated");
					IEnumerable<IPerson> people;
					var bu = _businessUnitRepository.Get(@event.LogOnBusinessUnitId);
					using (_businessUnitScope.OnThisThreadUse(bu))
					{
						people = _payrollPeopleLoader.GetPeopleForExport(@event, origPeriod, _currentUnitOfWork.Current());
					}

					var personDtos = _personBusAssembler.CreatePersonDto(people, _tenantPeopleLoader);

					var wrapper = new AppdomainCreatorWrapper();
					var searchPath = new SearchPath();
					var dto = createDto(payrollExport, @event, personDtos);
					
					if(!_installationEnvironment.IsAzure)
						PayrollDllCopy.CopyFiles(searchPath.PayrollDeployNewPath, searchPath.Path, @event.LogOnDatasource);
					
					var result = wrapper.RunPayroll(_sdkServiceFactory, dto, @event, payrollResult.Id.GetValueOrDefault(),
						_serviceBusPayrollExportFeedback, searchPath.Path);
					if (result != null)
						payrollResult.XmlResult.SetResult(result);
					payrollResult.FinishedOk = true;
				}
				catch (Exception exception)
				{
					logger.Error("An error occurred while running the payroll export. ", exception);
					//a very unusual way of reporting error but we need that to make the UI more responsive
					var payrollResult = _payrollResultRepository.Get(@event.PayrollResultId);
					_serviceBusPayrollExportFeedback.SetPayrollResult(payrollResult);
					_stardustJobFeedback.SendProgress("An error occurred while running the payroll export. " + exception.StackTrace);
					_serviceBusPayrollExportFeedback.Error(@"An error occurred while running the payroll export.", exception);
					
				}
				finally
				{
					_serviceBusPayrollExportFeedback.Dispose();
					_serviceBusPayrollExportFeedback = null;

					AppDomain.CurrentDomain.AssemblyResolve -= _domainAssemblyResolver.Resolve;
				}

			uow.PersistAll();
			}
		}

		private static PayrollExportDto createDto(IPayrollExport payrollExport, RunPayrollExportEvent @event, IEnumerable<PersonDto> personDtos)
		{
			var timeZone = payrollExport.CreatedBy.PermissionInformation.DefaultTimeZone();
			var origPeriod = new DateOnlyPeriod(new DateOnly(@event.ExportStartDate), new DateOnly(@event.ExportEndDate));
			var period = origPeriod.ToDateTimePeriod(timeZone);
			var exportDto = new PayrollExportDto
			{
				DatePeriod =
					new DateOnlyPeriodDto
					{
						StartDate = new DateOnlyDto { DateTime = @event.ExportStartDate },
						EndDate = new DateOnlyDto { DateTime = @event.ExportEndDate }
					},
				Id = payrollExport.Id,
				PayrollFormat = new PayrollFormatDto(@event.PayrollExportFormatId, string.Empty),
				Period =
					new DateTimePeriodDto
					{
						UtcStartTime = period.StartDateTime,
						UtcEndTime = period.EndDateTime,
						LocalStartDateTime = period.StartDateTimeLocal(timeZone),
						LocalEndDateTime = period.EndDateTimeLocal(timeZone)
					},
				TimeZoneId = timeZone.Id,
				Name = @event.LogOnDatasource
			};
			personDtos.ForEach(exportDto.PersonCollection.Add);

			return exportDto;
		}
	}

}