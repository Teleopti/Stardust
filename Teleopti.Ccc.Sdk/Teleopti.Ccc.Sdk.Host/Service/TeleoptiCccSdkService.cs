using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.ServiceModel;
using System.Threading;
using Autofac;
using log4net;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.Common.Contracts;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Common.WcfExtensions;
using Teleopti.Ccc.Sdk.Logic;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.Logic.MultiTenancy;
using Teleopti.Ccc.Sdk.Logic.Payroll;
using Teleopti.Ccc.Sdk.Logic.QueryHandler;
using Teleopti.Ccc.Sdk.Logic.Restrictions;
using Teleopti.Ccc.Sdk.WcfHost.Service.Factory;
using Teleopti.Ccc.Sdk.WcfHost.Service.LogOn;


namespace Teleopti.Ccc.Sdk.WcfHost.Service
{
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, ConcurrencyMode = ConcurrencyMode.Multiple, Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/")]
	public class TeleoptiCccSdkService : ITeleoptiCccLogOnService,
											ITeleoptiCccSdkService,
											ITeleoptiForecastingService,
											ITeleoptiSchedulingService,
											ITeleoptiOrganizationService,
											ITeleoptiCccSdkInternal,
											IDisposable
	{
		private readonly IFactoryProvider _factoryProvider;
		private readonly ILifetimeScope _lifetimeScope;
		private readonly ITenantPeopleSaver _tenantPeopleSaver;
		private readonly IChangePassword _changePassword;
		private readonly IUserTimeZone _userTimeZone;
		private static readonly object PayrollExportLock = new object();
		private static readonly ILog Logger = LogManager.GetLogger(typeof(TeleoptiCccSdkService));
		private readonly IAuthenticationFactory _authenticationFactory;
		private readonly IPayrollResultFactory _payrollResultFactory;

		public TeleoptiCccSdkService(IAuthenticationFactory authenticationFactory,
												  IPayrollResultFactory payrollResultFactory,
			 IFactoryProvider factoryProvider,
			 ILifetimeScope lifetimeScope,
			ITenantPeopleSaver tenantPeopleSaver,
			IChangePassword changePassword, 
			ICurrentBusinessUnit currentBusinessUnit,
			IUserTimeZone userTimeZone)
		{
			_authenticationFactory = authenticationFactory;
			_payrollResultFactory = payrollResultFactory;
			_factoryProvider = factoryProvider;
			_lifetimeScope = lifetimeScope;
			_tenantPeopleSaver = tenantPeopleSaver;
			_changePassword = changePassword;
			_userTimeZone = userTimeZone;
			Logger.Info("Creating new instance of the service.");
		}

		/// <summary>
		/// Gets the application settings.
		/// </summary>
		/// <returns>IDictionary of application settings</returns>
		/// <example>
		/// <code>
		/// //The service converts the IDictionary to an ArrayOfKeyValueOfstringstringKeyValueOfstringstring[]
		/// ArrayOfKeyValueOfstringstringKeyValueOfstringstring[] appSettings = sdkService.GetAppSettings();
		/// </code>
		/// </example>
		public IDictionary<string, string> GetAppSettings()
		{
			return new Dictionary<string, string>();
		}

		/// <summary>
		/// Gets the hibernate configuration.
		/// </summary>
		/// <returns>A collection of <see cref="string"/>.</returns>
		/// <example>
		/// <code>
		/// IColection&lt;string&gt; encryptedNHibConfigs = sdkService.GetHibernateConfiguration();
		/// </code>
		/// </example>
		public ICollection<string> GetHibernateConfiguration()
		{
			ICollection<string> hibConfigs = new List<string>();
			return hibConfigs;
		}

		public void SetWriteProtectionDateOnPerson(PersonWriteProtectionDto personWriteProtectionDto)
		{
			try
			{
				using (var inner = _lifetimeScope.BeginLifetimeScope())
				{
					_factoryProvider.CreateWriteProtectionFactory(inner).SetWriteProtectionDate(personWriteProtectionDto);
				}
			}
			catch (PermissionException exception)
			{

				throw new FaultException(exception.Message);
			}
		}

		public PersonWriteProtectionDto GetWriteProtectionDateOnPerson(PersonDto personDto)
		{
			using (var inner = _lifetimeScope.BeginLifetimeScope())
			{
				return _factoryProvider.CreateWriteProtectionFactory(inner).GetWriteProtectionDate(personDto);
			}
		}

		/// <summary>
		/// Converts a local DateTime to UTC
		/// </summary>
		/// <param name="localDateTime">The local date time.</param>
		/// <param name="fromTimeZoneId">From time zone id.</param>
		/// <returns>DateTime converted to UTC</returns>
		/// <example>
		/// <code>
		/// DateTime localDateTime = new DateTime(2009,05,01,0,0,0,DateTimeKind.Unspecified);
		///    
		/// TeleoptiCccSdkService sdkService = new TeleoptiCccSdkService(); 
		/// DateTime utcDateTime = sdkService.ConvertToUtc(localDateTime, "W. Europe Standard Time");
		/// </code>
		/// </example>
		public DateTime ConvertToUtc(DateTime localDateTime, string fromTimeZoneId)
		{
			TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(fromTimeZoneId);
			return timeZoneInfo.SafeConvertTimeToUtc(localDateTime);
		}
		public DateTime ConvertFromUtc(DateTime utcDateTime, string toTimeZoneId)
		{
			TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(toTimeZoneId);
			return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, timeZoneInfo);
		}

		#region Methods - Instance Methods - Login Contracts

		/// <summary>
		/// Gets the data source collection.
		/// </summary>
		/// <returns>A collection of <see cref="DataSourceDto"/>.</returns>
		[OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
		public ICollection<DataSourceDto> GetDataSources()
		{
			return _authenticationFactory.GetDataSources();
		}

		/// <summary>
		/// Log on Windows 
		/// </summary>
		/// <param name="dataSource">The data source.</param>
		/// <returns>A collection of <see cref="BusinessUnitDto"/>.</returns>
		public ICollection<BusinessUnitDto> LogOnWindows(DataSourceDto dataSource)
		{
			return _authenticationFactory.LogOnWindows(dataSource).BusinessUnitCollection;
		}

		public AuthenticationResultDto LogOnWindowsUser(DataSourceDto dataSource)
		{
			return _authenticationFactory.LogOnWindows(dataSource);
		}

		public AuthenticationResultDto LogOnAsWindowsUser()
		{
			return _authenticationFactory.LogOnWindows(null);
		}

		/// <summary>
		/// Log on application.
		/// </summary>
		/// <param name="userName">Name of the user.</param>
		/// <param name="password">The password.</param>
		/// <param name="dataSource">The data source.</param>
		/// <returns>A collection of <see cref="BusinessUnitDto"/>.</returns>
		public ICollection<BusinessUnitDto> LogOnApplication(string userName, string password, DataSourceDto dataSource)
		{
			return _authenticationFactory.LogOnApplication(userName, password, dataSource).BusinessUnitCollection;
		}

		public AuthenticationResultDto LogOnApplicationUser(string userName, string password, DataSourceDto dataSource)
		{
			return _authenticationFactory.LogOnApplication(userName, password, dataSource);
		}

		public AuthenticationResultDto LogOnAsApplicationUser(string userName, string password)
		{
			return _authenticationFactory.LogOnApplication(userName, password, null);
		}

		/// <summary>
		/// Transfers the session.
		/// </summary>
		/// <param name="sessionDataDto">The session data dto.</param>
		/// <example>
		/// <code>
		/// sdkService.TransferSession(sessionDataDto);
		/// </code>
		/// </example>
		[OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
		public void TransferSession(SessionDataDto sessionDataDto)
		{
		}

		/// <summary>
		/// Sets the business unit.
		/// </summary>
		/// <param name="businessUnit">The business unit.</param>
		public void SetBusinessUnit(BusinessUnitDto businessUnit)
		{
		}

		/// <summary>
		/// Verifies the license.
		/// </summary>
		/// <returns>A <see cref="LicenseVerificationResultDto"/>.</returns>
		public LicenseVerificationResultDto VerifyLicense()
		{
			//TODO: tenant, do need to do something here? Detect tenant?
			//wrong - if multidb...

			return _factoryProvider.CreateLicenseFactory().VerifyLicense(UnitOfWorkFactory.Current, "");
		}

		/// <summary>
		/// Gets the logged on person.
		/// </summary>
		/// <returns>The logged on <see cref="PersonDto"/>.</returns>
		public PersonDto GetLoggedOnPerson()
		{
			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var personDtoFactory = _lifetimeScope.Resolve<PersonDtoFactory>();
				return personDtoFactory.GetLoggedOnPerson();
			}
		}

		/// <summary>
		/// Gets the defined application function paths.
		/// </summary>L
		/// <returns>A <see cref="DefinedRaptorApplicationFunctionPathsDto"/>.</returns>
		public DefinedRaptorApplicationFunctionPathsDto GetDefinedApplicationFunctionPaths()
		{
			DefinedRaptorApplicationFunctionPathsDto dto = new DefinedRaptorApplicationFunctionPathsDto();

			dto.OpenRaptorApplication = DefinedRaptorApplicationFunctionPaths.OpenRaptorApplication;
			dto.RaptorGlobal = DefinedRaptorApplicationFunctionPaths.RaptorGlobal;
			dto.ModifyPersonAbsence = DefinedRaptorApplicationFunctionPaths.ModifyPersonAbsence;
			dto.ModifyPersonDayOff = DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment;
			dto.ModifyPersonAssignment = DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment;
			dto.ViewUnpublishedSchedules = DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules;
			dto.AccessToReports = DefinedRaptorApplicationFunctionPaths.AccessToReports;

			return dto;
		}

		public ICollection<PayrollExportDto> GetPayrollExportByQuery(QueryDto queryDto)
		{
			var invoker = _lifetimeScope.Resolve<IInvokeQuery<ICollection<PayrollExportDto>>>();
			return invoker.Invoke(queryDto);
		}

		public ICollection<PayrollResultDto> GetPayrollResultStatusByQuery(QueryDto queryDto)
		{
			var invoker = _lifetimeScope.Resolve<IInvokeQuery<ICollection<PayrollResultDto>>>();
			return invoker.Invoke(queryDto);
		}

		/// <summary>
		/// Gets the payroll formats.
		/// </summary>
		/// <returns>A collection of <see cref="PayrollFormatDto"/>.</returns>
		public ICollection<PayrollFormatDto> GetPayrollFormats()
		{
			var dataSource = ((TeleoptiIdentity)TeleoptiPrincipal.CurrentPrincipal.Identity).DataSource.DataSourceName;
			var formatter = _lifetimeScope.Resolve<PayrollFormatHandler>();
			return formatter.Load(dataSource);
		}

		//Added this for testing and separation of old stuff, dont know if the name 
		//is final
		public void CreateServerPayrollExport(PayrollExportDto payrollExport)
		{
			_payrollResultFactory.RunPayrollOnBus(payrollExport);
		}

		public void InitializePayrollFormats(ICollection<PayrollFormatDto> payrollFormatDtos)
		{
			var formatter = _lifetimeScope.Resolve<PayrollFormatHandler>();
			formatter.Save(payrollFormatDtos);
		}

		public ICollection<string> GetHibernateConfigurationInternal()
		{
			return GetHibernateConfiguration();
		}

		public LicenseVerificationResultDto VerifyLicenseInternal()
		{
			return VerifyLicense();
		}

		public void TransferSessionInternal(SessionDataDto sessionDataDto)
		{
			TransferSession(sessionDataDto);
		}

		public IDictionary<string, string> GetAppSettingsInternal()
		{
			return GetAppSettings();
		}

		/// <summary>
		/// Logs off the user
		/// </summary>
		public void LogOffUser()
		{
			var authorizationPolicies = OperationContext.Current.ServiceSecurityContext.AuthorizationPolicies.OfType<TeleoptiPrincipalAuthorizationPolicy>();
			var teleoptiAuthPolicy = authorizationPolicies.FirstOrDefault();
			if (teleoptiAuthPolicy != null)
			{
				var personCache = new PersonCache();
				personCache.Remove(teleoptiAuthPolicy.PersonContainer);
			}
		}

		/// <summary>
		/// Determines whether this instance is authenticated.
		/// </summary>
		/// <returns>
		/// 	<c>true</c> if this instance is authenticated; otherwise, <c>false</c>.
		/// </returns>
		public bool IsAuthenticated()
		{
			//also wrong if multidb
			//TODO: tenant, but this is obsolete so it is no problem
			var principal = Thread.CurrentPrincipal as ITeleoptiPrincipal;
			return (principal != null && principal.Identity.IsAuthenticated)
					 && new LicenseCache().Get("") != null;
		}

		#endregion

		#region Methods - Instance Methods - Schedule Contracts

		/// <summary>
		/// Gets the person schedule part.
		/// </summary>
		/// <param name="person">The person.</param>
		/// <param name="startDate">The start date.</param>
		/// <param name="timeZoneId">The time zone id.</param>
		/// <returns>A <see cref="SchedulePartDto"/>.</returns>
		/// <remarks>
		/// </remarks>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public SchedulePartDto GetSchedulePart(PersonDto person, DateOnlyDto startDate, string timeZoneId)
		{
			return
				GetSchedulesByQuery(new GetSchedulesByPersonQueryDto
											{
												StartDate = startDate,
												EndDate = startDate,
												PersonId = person.Id.GetValueOrDefault(),
												TimeZoneId = timeZoneId
											}).First();
		}

		public ICollection<SchedulePartDto> GetScheduleParts(PersonDto person, DateOnlyDto startDate, DateOnlyDto endDate, string timeZoneId)
		{
			if (person == null) throw new FaultException("PersonId cannot be null.");
			return
			GetSchedulesByQuery(new GetSchedulesByPersonQueryDto { PersonId = person.Id.GetValueOrDefault(), StartDate = startDate, EndDate = endDate, TimeZoneId = timeZoneId });
		}

		public ICollection<SchedulePartDto> GetSchedulePartsForPersons(PersonDto[] personList, DateOnlyDto startDate, DateOnlyDto endDate, string timeZoneId)
		{
			using (var inner = _lifetimeScope.BeginLifetimeScope())
			{
				return _factoryProvider.CreateScheduleFactory(inner).CreateSchedulePartCollection(personList, startDate,
																													  endDate, timeZoneId, string.Empty);
			}
		}

		public ICollection<PayrollBaseExportDto> GetTeleoptiTimeExportData(PersonDto[] personList, DateOnlyDto startDate, DateOnlyDto endDate,
																	  string timeZoneId)
		{
			using (var inner = _lifetimeScope.BeginLifetimeScope())
			{
				return _factoryProvider.CreateTeleoptiPayrollFactory(inner).GetTeleoptiTimeExportData(personList, startDate,
																													  endDate, timeZoneId);
			}
		}

		public ICollection<PayrollBaseExportDto> GetTeleoptiDetailedExportData(PersonDto[] personList, DateOnlyDto startDate, DateOnlyDto endDate,
																			string timeZoneId)
		{
			using (var inner = _lifetimeScope.BeginLifetimeScope())
			{
				var absences = GetAbsences(new AbsenceLoadOptionDto { LoadDeleted = true });
				var absenceDictinary = absences
					.Where(a => a.Id.HasValue)
					.ToDictionary(a => a.Id.GetValueOrDefault());

				return _factoryProvider.CreateTeleoptiPayrollFactory(inner).GetTeleoptiDetailedExportData(personList, startDate,
																															endDate, timeZoneId, absenceDictinary);
			}
		}

		public ICollection<PayrollBaseExportDto> GetTeleoptiActivitiesExportData(PersonDto[] personList, DateOnlyDto startDate, DateOnlyDto endDate, string timeZoneId)
		{
			using (var inner = _lifetimeScope.BeginLifetimeScope())
			{
				var loadDeleted = new LoadOptionDto { LoadDeleted = true };

				var absences = GetAbsences(new AbsenceLoadOptionDto { LoadDeleted = true });
				var absenceDictinary = absences
					.Where(a => a.Id.HasValue)
					.ToDictionary(a => a.Id.GetValueOrDefault());

				var dayOffCodes = GetDaysOffs(loadDeleted)
					.Where(d => d.Id.HasValue)
					.Select(d => d.Id.GetValueOrDefault())
					.ToList();

				var activites = GetActivities(loadDeleted);
				var activityDictionary = activites
					.Where(a => a.Id.HasValue)
					.ToDictionary(a => a.Id.GetValueOrDefault());

				return
					_factoryProvider.CreateTeleoptiPayrollFactory(inner)
										 .GetTeleoptiPayrollActivitiesExportData(personList, startDate,
																							  endDate, timeZoneId,
																							  absenceDictinary,
																		dayOffCodes,
																		activityDictionary);
			}
		}

		public ICollection<SchedulePartDto> GetSchedules(ScheduleLoadOptionDto scheduleLoadOptionDto, DateOnlyDto startDate, DateOnlyDto endDate, string timeZoneId)
		{
			if (scheduleLoadOptionDto == null)
				throw new FaultException("Parameter scheduleLoadOptionDto cannot be null.");

			QueryDto query = null;
			if (scheduleLoadOptionDto.LoadAll)
			{
				query = new GetSchedulesForAllPeopleQueryDto
							{
								StartDate = startDate,
								EndDate = endDate,
								TimeZoneId = timeZoneId,
								SpecialProjection = scheduleLoadOptionDto.SpecialProjection
							};
			}
			if (scheduleLoadOptionDto.LoadSite != null)
			{
				query = new GetSchedulesBySiteQueryDto
					{
						StartDate = startDate,
						EndDate = endDate,
						TimeZoneId = timeZoneId,
						SiteId = scheduleLoadOptionDto.LoadSite.Id.GetValueOrDefault(),
						SpecialProjection = scheduleLoadOptionDto.SpecialProjection
					};
			}
			if (scheduleLoadOptionDto.LoadTeam != null)
			{
				query = new GetSchedulesByTeamQueryDto
												{
													StartDate = startDate,
													EndDate = endDate,
													TimeZoneId = timeZoneId,
													TeamId = scheduleLoadOptionDto.LoadTeam.Id.GetValueOrDefault(),
													SpecialProjection = scheduleLoadOptionDto.SpecialProjection
												};
			}
			if (scheduleLoadOptionDto.LoadPerson != null)
			{
				query = new GetSchedulesByPersonQueryDto
							{
								StartDate = startDate,
								EndDate = endDate,
								TimeZoneId = timeZoneId,
								PersonId = scheduleLoadOptionDto.LoadPerson.Id.GetValueOrDefault(),
								SpecialProjection = scheduleLoadOptionDto.SpecialProjection
							};
			}

			if (query == null)
			{
				return new Collection<SchedulePartDto>();
			}
			return GetSchedulesByQuery(query);
		}

		public ICollection<SchedulePartDto> GetSchedulesByQuery(QueryDto queryDto)
		{
			using (var inner = _lifetimeScope.BeginLifetimeScope())
			{
				var invoker = inner.Resolve<IInvokeQuery<ICollection<SchedulePartDto>>>();
				return invoker.Invoke(queryDto);
			}
		}

		public ICollection<MultiplicatorDto> GetMultiplicatorsByQuery(QueryDto queryDto)
		{
			using (var inner = _lifetimeScope.BeginLifetimeScope())
			{
				var invoker = inner.Resolve<IInvokeQuery<ICollection<MultiplicatorDto>>>();
				return invoker.Invoke(queryDto);
			}
		}

		public ICollection<DefinitionSetDto> GetMultiplicatorDefinitionSetByQuery(QueryDto queryDto)
		{
			using (var inner = _lifetimeScope.BeginLifetimeScope())
			{
				var invoker = inner.Resolve<IInvokeQuery<ICollection<DefinitionSetDto>>>();
				return invoker.Invoke(queryDto);
			}
		}

		public ICollection<ScheduleTagDto> GetScheduleTagByQuery(QueryDto queryDto)
		{
			using (var inner = _lifetimeScope.BeginLifetimeScope())
			{
				var invoker = inner.Resolve<IInvokeQuery<ICollection<ScheduleTagDto>>>();
				return invoker.Invoke(queryDto);
			}
		}

		public ICollection<ScheduleChangesSubscriptionsDto> GetScheduleChangeSubscriptionsByQuery(QueryDto queryDto)
		{
			using (var inner = _lifetimeScope.BeginLifetimeScope())
			{
				var invoker = inner.Resolve<IInvokeQuery<ICollection<ScheduleChangesSubscriptionsDto>>>();
				return invoker.Invoke(queryDto);
			}
		}

		public IAsyncResult BeginCreateServerScheduleDistribution(PersonDto[] personList, DateOnlyDto startDate, DateOnlyDto endDate, string timeZoneId, AsyncCallback callback, object asyncState)
		{
			CreateScheduleDistributionAsyncResult asyncResult = new CreateScheduleDistributionAsyncResult(callback, asyncState);
			asyncResult.PersonList = personList;
			asyncResult.DateOnlyPeriod = new DateOnlyPeriodDto { StartDate = startDate, EndDate = endDate };
			asyncResult.TimeZone = timeZoneId;
			asyncResult.Principal = (IPrincipal)ServiceSecurityContext.Current.AuthorizationContext.Properties["Principal"];

			ThreadPool.QueueUserWorkItem(
				CreateServerScheduleDistributionCallback,
				asyncResult);

			return asyncResult;
		}

		public ICollection<PublicNoteDto> GetPublicNotes(PublicNoteLoadOptionDto publicNoteLoadOptionDto, DateOnlyDto startDate, DateOnlyDto endDate)
		{
			if (publicNoteLoadOptionDto == null)
				throw new FaultException("Parameter publicNoteLoadOptionDto cannot be null.");

			ICollection<TeamDto> teamDtos = null;

			if (publicNoteLoadOptionDto.LoadSite != null)
				teamDtos = GetTeamsOnSite(publicNoteLoadOptionDto.LoadSite);

			return _factoryProvider.CreatePublicNoteTypeFactory().GetPublicNotes(publicNoteLoadOptionDto, teamDtos, startDate, endDate);
		}

		public void SavePublicNote(PublicNoteDto publicNoteDto)
		{
			var publicNotesFactory = _factoryProvider.CreatePublicNoteTypeFactory();
			publicNotesFactory.SavePublicNote(publicNoteDto);
		}

		public void DeletePublicNote(PublicNoteDto publicNoteDto)
		{
			var publicNotesFactory = _factoryProvider.CreatePublicNoteTypeFactory();
			publicNotesFactory.DeletePublicNote(publicNoteDto);
		}

		public PlanningTimeBankDto GetPlanningTimeBank(PersonDto personDto, DateOnlyDto dateOnlyDto)
		{
			using (var inner = _lifetimeScope.BeginLifetimeScope())
			{
				return _factoryProvider.CreatePlanningTimeBankFactory(inner).GetPlanningTimeBank(personDto, dateOnlyDto);
			}
		}

		public void SavePlanningTimeBank(PersonDto personDto, DateOnlyDto dateOnlyDto, int balanceOutMinute)
		{
			using (var inner = _lifetimeScope.BeginLifetimeScope())
			{
				_factoryProvider.CreatePlanningTimeBankFactory(inner).SavePlanningTimeBank(personDto, dateOnlyDto, balanceOutMinute);
			}
		}

		private void CreateServerScheduleDistributionCallback(object state)
		{
			//Need to add a lock here. Otherwise there's a risk that sessions will hijack eachother
			CreateScheduleDistributionAsyncResult asyncResult = (CreateScheduleDistributionAsyncResult)state;
			lock (PayrollExportLock)
			{
				try
				{
					CreateServerScheduleDistribution(asyncResult.PersonList,
													 asyncResult.DateOnlyPeriod.StartDate,
													 asyncResult.DateOnlyPeriod.EndDate,
													 asyncResult.TimeZone);
				}
				finally
				{
					asyncResult.OnCompleted();
				}
			}
		}

		public void EndCreateServerScheduleDistribution(IAsyncResult result)
		{
			using (CreateScheduleDistributionAsyncResult asyncResult =
				result as CreateScheduleDistributionAsyncResult)
			{
				asyncResult?.AsyncWaitHandle.WaitOne();
			}
		}

		private void CreateServerScheduleDistribution(IList<PersonDto> personList, DateOnlyDto startDate, DateOnlyDto endDate, string timeZoneId)
		{
			using (var inner = _lifetimeScope.BeginLifetimeScope())
			{
				_factoryProvider.CreateScheduleMailFactory(inner).SendScheduleMail(personList, startDate, endDate,
																										 timeZoneId);
			}
		}

		/// <summary>
		/// Gets the person multiplicator data.
		/// </summary>
		/// <param name="person">The person.</param>
		/// <param name="startDate">The start date.</param>
		/// <param name="endDate">The end date.</param>
		/// <returns>
		/// A collection of <see cref="MultiplicatorDataDto"/>.
		/// </returns>
		public ICollection<MultiplicatorDataDto> GetPersonMultiplicatorDataForPerson(PersonDto person, DateOnlyDto startDate, DateOnlyDto endDate)
		{
			using (var inner = _lifetimeScope.BeginLifetimeScope())
			{
				return _factoryProvider.CreateScheduleFactory(inner).CreateMultiplicatorData(new List<PersonDto> { person },
																												startDate, endDate,
																												person.TimeZoneId);
			}
		}

		public ICollection<MultiplicatorDataDto> GetPersonMultiplicatorDataForPersons(PersonDto[] personCollection, DateOnlyDto startDate, DateOnlyDto endDate, string timeZoneId)
		{
			using (var inner = _lifetimeScope.BeginLifetimeScope())
			{
				return _factoryProvider.CreateScheduleFactory(inner).CreateMultiplicatorData(personCollection, startDate,
																												endDate, timeZoneId);
			}
		}

		/// <summary>
		/// Gets the activity types.
		/// </summary>
		/// <param name="loadOptionDto">The load option dto.</param>
		/// <returns>
		/// A collection of <see cref="ActivityDto"/>.
		/// </returns>
		public ICollection<ActivityDto> GetActivities(LoadOptionDto loadOptionDto)
		{
			return ActivityTypeFactory.GetActivities(loadOptionDto);
		}

		/// <summary>
		/// Gets absences depending on <see cref="loadOptionDto"/>.
		/// </summary>
		/// <param name="loadOptionDto">The load option dto.</param>
		/// <returns></returns>
		public ICollection<AbsenceDto> GetAbsences(AbsenceLoadOptionDto loadOptionDto)
		{
			return AbsenceTypeFactory.GetAbsences(loadOptionDto);
		}

		/// <summary>
		/// Finds all <see cref="DayOffInfoDto"/>.
		/// </summary>
		/// <returns>
		/// A collection of <see cref="DayOffInfoDto"/>.
		/// </returns>
		public ICollection<DayOffInfoDto> GetDaysOffs(LoadOptionDto loadOptionDto)
		{
			return DayOffFactory.GetDayOffs(loadOptionDto);
		}

		/// <summary>
		/// Gets all <see cref="ShiftCategoryDto"/>.
		/// </summary>
		/// <returns>
		/// A collection of <see cref="ShiftCategoryDto"/>.
		/// </returns>
		public ICollection<ShiftCategoryDto> GetShiftCategories(LoadOptionDto loadOptionDto)
		{
			return ShiftCategoryFactory.GetShiftCategories(loadOptionDto);
		}

		/// <summary>
		/// Gets the contracts.
		/// </summary>
		/// <returns></returns>
		public ICollection<ContractDto> GetContracts(LoadOptionDto loadOptionDto)
		{
			return ContractFactory.GetContracts(loadOptionDto);
		}

		public ICollection<PartTimePercentageDto> GetPartTimePercentages(LoadOptionDto loadOptionDto)
		{
			return PartTimePercentageFactory.GetPartTimePercentages(loadOptionDto);
		}

		public ICollection<OvertimeDefinitionSetDto> GetOvertimeDefinitions(LoadOptionDto loadOptionDto)
		{
			return OvertimeDefinitionFactory.GetOvertimeDefinitions(loadOptionDto);
		}

		public ICollection<ContractScheduleDto> GetContractSchedules(LoadOptionDto loadOptionDto)
		{
			return ContractScheduleFactory.GetContractSchedules(loadOptionDto);
		}

		public void AddPerson(PersonDto personDto)
		{
			if (personDto.Id.HasValue)
				throw new FaultException("new person should not have an id");
			var result = ExecuteCommand(new AddPersonCommandDto
			{
				FirstName = personDto.FirstName,
				LastName = personDto.LastName,
				ApplicationLogonName = personDto.ApplicationLogOnName,
				ApplicationLogOnPassword = personDto.ApplicationLogOnPassword,
				Identity = personDto.Identity,
				Email = personDto.Email,
				EmploymentNumber = personDto.EmploymentNumber,
				CultureLanguageId = personDto.CultureLanguageId,
				UICultureLanguageId = personDto.UICultureLanguageId,
				Note = personDto.Note,
				TimeZoneId = personDto.TimeZoneId,
				WorkWeekStart = personDto.FirstDayOfWeek,
				WorkflowControlSetId = personDto.WorkflowControlSet?.Id,
				IsDeleted = personDto.IsDeleted
			});
			if (result.AffectedId != null)
			{
				personDto.Id = result.AffectedId;
				if (personDto.PersonPeriodCollection != null)
				{
					foreach (var personPeriodDto in personDto.PersonPeriodCollection)
					{
						AddPersonPeriod(personDto, personPeriodDto);
					}
				}
			}
		}
		public void UpdatePerson(PersonDto personDto)
		{
			if (!personDto.Id.HasValue)
				throw new FaultException("Person must have an id to be updated");
			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				IRepositoryFactory repositoryFactory = new RepositoryFactory();
				IPersonRepository repository = repositoryFactory.CreatePersonRepository(uow);
				var assembler = _factoryProvider.CreatePersonAssembler();
				((PersonAssembler)assembler).EnableSaveOrUpdate = true;
				IPerson person = assembler.DtoToDomainEntity(personDto);
				repository.Add(person);
				uow.PersistAll();
				_tenantPeopleSaver.SaveTenantData(personDto, person.Id.Value);
			}
		}

		public void AddPersonPeriod(PersonDto personDto, PersonPeriodDto personPeriodDto)
		{
			var command = new ChangePersonEmploymentCommandDto
									{
										Person = personDto,
										Period = personPeriodDto.Period,
										PersonContract = personPeriodDto.PersonContract,
										Team = personPeriodDto.Team
									};
			ExecuteCommand(command);
		}

		public ICollection<SiteDto> GetSitesOnBusinessUnit(BusinessUnitDto businessUnitDto)
		{
			ICollection<SiteDto> sitesOnBusinessUnit = new Collection<SiteDto>();
			using (IUnitOfWork unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				ISiteRepository repository = new SiteRepository(unitOfWork);
				var sites = repository.LoadAll();

				foreach (ISite site in sites)
				{
					if (businessUnitDto.Id.Value == site.BusinessUnit.Id.Value)
					{
						SiteDto dto = new SiteDto { DescriptionName = site.Description.Name, Id = site.Id };
						sitesOnBusinessUnit.Add(dto);
					}
				}
			}
			return sitesOnBusinessUnit;
		}

		public ICollection<TeamDto> GetTeamsOnSite(SiteDto siteDto)
		{
			ICollection<TeamDto> teamsOnSite = new Collection<TeamDto>();
			using (IUnitOfWork unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				ITeamRepository repository = new TeamRepository(unitOfWork);
				var teams = repository.LoadAll();

				foreach (ITeam team in teams)
				{
					if (team.Site.Id.Value == siteDto.Id.Value)
					{
						TeamDto dto = new TeamDto { Description = team.Description.Name, Id = team.Id, SiteAndTeam = team.SiteAndTeam };
						teamsOnSite.Add(dto);
					}
				}
			}
			return teamsOnSite;
		}

		public ICollection<ShiftCategoryDto> GetShiftCategoriesBelongingToRuleSetBag(PersonDto personDto, DateOnlyDto startDateOnlyDto, DateOnlyDto endDateOnlyDto)
		{
			ICollection<ShiftCategoryDto> list = new List<ShiftCategoryDto>();
			DateOnly start = new DateOnly(startDateOnlyDto.DateTime);
			DateOnly end = new DateOnly(endDateOnlyDto.DateTime);
			DateOnlyPeriod dateOnlyPeriod = new DateOnlyPeriod(start, end);
			using (IUnitOfWork unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				IRepositoryFactory repositoryFactory = new RepositoryFactory();
				ShiftCategoryAssembler assembler =
					new ShiftCategoryAssembler(repositoryFactory.CreateShiftCategoryRepository(unitOfWork));
				IPersonRepository personRepository = repositoryFactory.CreatePersonRepository(unitOfWork);
				IPerson person = personRepository.Get(personDto.Id.Value);
				IList<IPersonPeriod> personPeriods = person.PersonPeriods(dateOnlyPeriod);

				foreach (IPersonPeriod period in personPeriods)
				{
					if (period.RuleSetBag == null) continue;
					foreach (IShiftCategory category in period.RuleSetBag.ShiftCategoriesInBag())
					{
						IShiftCategory shiftCategory = category;
						if (list.All(s => s.Id != shiftCategory.Id))
							list.Add(assembler.DomainEntityToDto(shiftCategory));
					}
				}
			}
			return list.ToList();
		}


		private static int GetMatrixReportSetting()
		{
			AdherenceReportSetting adherenceReportSetting;
			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				IRepositoryFactory repositoryFactory = new RepositoryFactory();
				adherenceReportSetting = repositoryFactory.CreateGlobalSettingDataRepository(uow).FindValueByKey(AdherenceReportSetting.Key, new AdherenceReportSetting());
			}
			return adherenceReportSetting.AdherenceIdForReport();
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.Convert.ToInt32(System.String)")]
		public AdherenceDto GetAdherenceData(DateTime dateTime, string timeZoneId, PersonDto personDto,
										PersonDto agentPersonDto, int languageId)
		{
			//Kolla permissions på datat
			//TeleoptiPrincipal.Current.Person.PermissionInformation.

			AdherenceDto adherenceDto = new AdherenceDto();
			if (!personDto.Id.HasValue || !agentPersonDto.Id.HasValue)
				return adherenceDto;

			IRepositoryFactory repositoryFactory = new RepositoryFactory();
			
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var personRepository = repositoryFactory.CreatePersonRepository(uow);
				var person = personRepository.Get(personDto.Id.Value);
				var schedulePublishedToDate = person.WorkflowControlSet.SchedulePublishedToDate ?? new DateTime(1900, 01, 01);

				// A quick fix for bug #34706
				// If dateTime exceed schedule published date, then do not return adherence data.
				if (dateTime.Date > schedulePublishedToDate.Date)
				{
					return adherenceDto;
				}
			}
			
			IStatisticRepository repository = repositoryFactory.CreateStatisticRepository();
			int adherenceCalculationId = GetMatrixReportSetting();

			IList returnValues = repository.LoadAdherenceData(dateTime, timeZoneId, personDto.Id.Value, agentPersonDto.Id.Value, languageId, adherenceCalculationId);
			if (returnValues.Count == 0)
				return adherenceDto;
			foreach (object[] data in returnValues)
			{
				string temp = data[2].ToString();
				int startHour = Convert.ToInt32(temp.Substring(0, 2));
				int startMinutes = Convert.ToInt32(temp.Substring(3, 2));
				int endHour = Convert.ToInt32(temp.Substring(6, 2));
				int endMinutes = Convert.ToInt32(temp.Substring(9, 2));
				DateTime calendarDateTime = Convert.ToDateTime(data[0].ToString());
				DateTime shiftBelongsToDateTime = Convert.ToDateTime(data[29].ToString());
				int day = 0;

				if (calendarDateTime != shiftBelongsToDateTime)
					day = 1;
				TimeSpan startTime = new TimeSpan(day, startHour, startMinutes, 0);
				TimeSpan endTime = new TimeSpan(day, endHour, endMinutes, 0);

				decimal deviation;
				decimal dayAdherence;
				decimal readyTime;
				decimal adherence;
				decimal? adherenceForPeriod = null;
				decimal? adherenceForDay = null;
				if (data[16] == null)
					deviation = 0;
				else
					deviation = (decimal)data[16];
				if (data[13] == null)
					dayAdherence = 0;
				else
				{
					dayAdherence = (decimal)data[13];
					adherenceForDay = (decimal)data[13];
				}
				if (data[17] == null)
					readyTime = 0;
				else
					readyTime = (decimal)data[17];
				if (data[12] == null)
					adherence = 0;
				else
				{
					adherence = (decimal)data[12];
					adherenceForPeriod = (decimal)data[12];
				}
				var adherenceDataDto = new AdherenceDataDto(startTime.Ticks, endTime.Ticks, readyTime, deviation, adherenceForPeriod, calendarDateTime, shiftBelongsToDateTime)
					{
#pragma warning disable 612,618
						Adherence = adherence,
						DayAdherence = dayAdherence,
#pragma warning restore 612,618
						AdherenceForDay = adherenceForDay
					};
				adherenceDto.AdherenceDataDtos.Add(adherenceDataDto);
			}
			return adherenceDto;
		}

		public IList<AgentQueueStatDetailsDto> GetAgentQueueStatDetails(DateTime startDate, DateTime endDate, string timeZoneId, PersonDto personDto)
		{
			IList<AgentQueueStatDetailsDto> agentQueueStatDetailsDtos = new List<AgentQueueStatDetailsDto>();
			IRepositoryFactory repositoryFactory = new RepositoryFactory();
			IStatisticRepository repository = repositoryFactory.CreateStatisticRepository();
			IList returnValues = repository.LoadAgentQueueStat(startDate, endDate, timeZoneId, personDto.Id.Value);

			if (returnValues.Count == 0)
				return agentQueueStatDetailsDtos;

			foreach (object[] data in returnValues)
			{
				string queueName;
				int answeredContacts;
				int averageTalkTime;
				int averageAfterContactWork;
				int averageHandlingTime;
				if (data[1] == null)
					queueName = string.Empty;
				else
					queueName = (string)data[1];
				if (data[2] == null)
					answeredContacts = 0;
				else
					answeredContacts = (int)data[2];
				if (data[3] == null)
					averageTalkTime = 0;
				else
					averageTalkTime = (int)(decimal)data[3];
				if (data[4] == null)
					averageAfterContactWork = 0;
				else
					averageAfterContactWork = (int)(decimal)data[4];
				if (data[5] == null)
					averageHandlingTime = 0;
				else
					averageHandlingTime = (int)(decimal)data[5];
				AgentQueueStatDetailsDto agentQueueStatDetailsDto = new AgentQueueStatDetailsDto();
				agentQueueStatDetailsDto.AfterContactWorkTime = new TimeSpan(0, 0, averageAfterContactWork).Ticks;
				agentQueueStatDetailsDto.AverageTalkTime = new TimeSpan(0, 0, averageTalkTime).Ticks;
				agentQueueStatDetailsDto.AnsweredContacts = answeredContacts;
				agentQueueStatDetailsDto.QueueName = queueName;
				agentQueueStatDetailsDto.AverageHandlingTime = new TimeSpan(0, 0, averageHandlingTime).Ticks;
				agentQueueStatDetailsDtos.Add(agentQueueStatDetailsDto);
			}
			return agentQueueStatDetailsDtos;
		}

		public IList<AdherenceInfoDto> GetAdherenceInfo(DateTime startDate, DateTime endDate, string timeZoneId, PersonDto personDto)
		{
			IRepositoryFactory repositoryFactory = new RepositoryFactory();

			IList<AdherenceInfoDto> adherenceInfoDtos = new List<AdherenceInfoDto>();
			if (!personDto.Id.HasValue)
			{
				return adherenceInfoDtos;
			}

			IScenario defaultScenario;
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var personRepository = repositoryFactory.CreatePersonRepository(uow);
				var person = personRepository.Get(personDto.Id.Value);
				var schedulePublishedToDate = person.WorkflowControlSet.SchedulePublishedToDate ?? new DateTime(1900, 01, 01);

				// A quick fix for bug #34706
				// If dateTime exceed schedule published date, then do not return adherence data.
				if (endDate.Date > schedulePublishedToDate.Date)
				{
					return adherenceInfoDtos;
				}

				defaultScenario = repositoryFactory.CreateScenarioRepository(uow).LoadDefaultScenario();
			}
			
			IStatisticRepository repository = repositoryFactory.CreateStatisticRepository();
			IList returnValues = repository.LoadAgentStat(defaultScenario.Id.Value, startDate, endDate, timeZoneId, personDto.Id.Value);
			if (returnValues.Count == 0)
				return adherenceInfoDtos;
			foreach (object[] data in returnValues)
			{
				DateTime dateTime = (DateTime)data[1];
				int availableTime;
				int idleTime;
				int loggedInTime;
				int scheduledWorkCtiTime;
				if (data[2] == null)
					scheduledWorkCtiTime = 0;
				else
					scheduledWorkCtiTime = (int)data[2];
				if (data[3] == null)
					loggedInTime = 0;
				else
					loggedInTime = (int)data[3];
				if (data[4] == null)
					idleTime = 0;
				else
					idleTime = (int)data[4];
				if (data[5] == null)
					availableTime = 0;
				else
					availableTime = (int)data[5];

				AdherenceInfoDto adherenceInfoDto = new AdherenceInfoDto
				{
					AvailableTime = new TimeSpan(0, 0, availableTime).Ticks,
					DateOnlyDto = new DateOnlyDto {DateTime = dateTime},
					IdleTime = new TimeSpan(0, 0, idleTime).Ticks,
					LoggedInTime = new TimeSpan(0, 0, loggedInTime).Ticks,
					ScheduleWorkCtiTime = new TimeSpan(0, 0, scheduledWorkCtiTime).Ticks
				};
				adherenceInfoDtos.Add(adherenceInfoDto);
			}
			return adherenceInfoDtos;
		}

		public void SavePreference(PreferenceRestrictionDto preferenceRestrictionDto)
		{
			IRepositoryFactory repositoryFactory = new RepositoryFactory();
			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				deletePreference(preferenceRestrictionDto, repositoryFactory, uow);
				IPreferenceDayRepository repository = repositoryFactory.CreatePreferenceDayRepository(uow);
				var assembler = _factoryProvider.CreatePreferenceDayAssembler();

				preferenceRestrictionDto.Id = null;
				var preferenceDay = assembler.DtoToDomainEntity(preferenceRestrictionDto);
				repository.Add(preferenceDay);
				uow.PersistAll();
			}
		}

		public void DeletePreference(PreferenceRestrictionDto preferenceRestrictionDto)
		{
			IRepositoryFactory repositoryFactory = new RepositoryFactory();

			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				deletePreference(preferenceRestrictionDto, repositoryFactory, uow);
				uow.PersistAll();
			}
		}

		private static void deletePreference(PreferenceRestrictionDto preferenceRestrictionDto,
														 IRepositoryFactory repositoryFactory, IUnitOfWork uow)
		{
			var personRepository = repositoryFactory.CreatePersonRepository(uow);
			var repository = repositoryFactory.CreatePreferenceDayRepository(uow);

			var person = personRepository.Get(preferenceRestrictionDto.Person.Id.GetValueOrDefault());
			if (person == null) throw new FaultException("Given person was not found.");
			var days = repository.FindAndLock(preferenceRestrictionDto.RestrictionDate.ToDateOnly(), person);
			foreach (IPreferenceDay day in days)
			{
				repository.Remove(day);
			}
		}

		public void SaveStudentAvailabilityDay(StudentAvailabilityDayDto studentAvailabilityDayDto)
		{
			DeleteStudentAvailabilityDay(studentAvailabilityDayDto);
			IRepositoryFactory repositoryFactory = new RepositoryFactory();

			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var studentAvailabilityDay = GetStudentAvailabilityDomainFromDto(studentAvailabilityDayDto);
				var repository = repositoryFactory.CreateStudentAvailabilityDayRepository(uow);
				repository.Add(studentAvailabilityDay);
				uow.PersistAll();
			}
		}

		public void DeleteStudentAvailabilityDay(StudentAvailabilityDayDto studentAvailabilityDayDto)
		{
			var repositoryFactory = new RepositoryFactory();
			var studentDay = GetStudentAvailabilityDomainFromDto(studentAvailabilityDayDto);

			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var repository = repositoryFactory.CreateStudentAvailabilityDayRepository(uow);
				var days = repository.Find(studentDay.RestrictionDate, studentDay.Person);
				foreach (var day in days)
				{
					repository.Remove(day);
				}
				uow.PersistAll();
			}
		}

		public void SaveExtendedPreferenceTemplate(ExtendedPreferenceTemplateDto extendedPreferenceTemplateDto)
		{
			var repositoryFactory = new RepositoryFactory();
			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var person = TeleoptiPrincipal.CurrentPrincipal.GetPerson(repositoryFactory.CreatePersonRepository(uow));
				var extendedPreferenceTemplateRepository = repositoryFactory.CreateExtendedPreferenceTemplateRepository(uow);

				var assembler = new ExtendedPreferenceTemplateAssembler(person,
																		new RestrictionAssembler
																			<IPreferenceRestrictionTemplate,
																			ExtendedPreferenceTemplateDto, IActivityRestrictionTemplate>(new PreferenceRestrictionTemplateConstructor(),
																			new ShiftCategoryAssembler(
																				repositoryFactory.
																					CreateShiftCategoryRepository(uow)),
																			new DayOffAssembler(
																				repositoryFactory.CreateDayOffRepository
																					(uow)),
																			new ActivityRestrictionAssembler<IActivityRestrictionTemplate>(new ActivityRestrictionTemplateDomainObjectCreator(),
																				new ActivityAssembler(repositoryFactory.CreateActivityRepository(uow))),
																									 new AbsenceAssembler(repositoryFactory.CreateAbsenceRepository(uow))));
				var template = assembler.DtoToDomainEntity(extendedPreferenceTemplateDto);

				extendedPreferenceTemplateRepository.Add(template);
				uow.PersistAll();
			}
		}

		public void DeleteExtendedPreferenceTemplate(ExtendedPreferenceTemplateDto extendedPreferenceTemplateDto)
		{
			var repositoryFactory = new RepositoryFactory();
			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var extendedPreferenceTemplateRepository = repositoryFactory.CreateExtendedPreferenceTemplateRepository(uow);
				var template = extendedPreferenceTemplateRepository.Load(extendedPreferenceTemplateDto.Id.GetValueOrDefault(Guid.Empty));
				extendedPreferenceTemplateRepository.Remove(template);
				uow.PersistAll();
			}
		}

		public ICollection<ExtendedPreferenceTemplateDto> GetExtendedPreferenceTemplates(PersonDto personDto)
		{
			var repositoryFactory = new RepositoryFactory();
			var dtoTemplates = new List<ExtendedPreferenceTemplateDto>();
			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var extendedPreferenceTemplateRepository = repositoryFactory.CreateExtendedPreferenceTemplateRepository(uow);
				var perRep = repositoryFactory.CreatePersonRepository(uow);
				var person = perRep.Load(personDto.Id.GetValueOrDefault(Guid.Empty));
				var domainTemplates = extendedPreferenceTemplateRepository.FindByUser(person);
				var assembler = new ExtendedPreferenceTemplateAssembler(person,
																		new RestrictionAssembler
																			<IPreferenceRestrictionTemplate,
																			ExtendedPreferenceTemplateDto, IActivityRestrictionTemplate>(new PreferenceRestrictionTemplateConstructor(),
																			new ShiftCategoryAssembler(
																				repositoryFactory.
																					CreateShiftCategoryRepository(uow)),
																			new DayOffAssembler(
																				repositoryFactory.CreateDayOffRepository
																					(uow)),
																			new ActivityRestrictionAssembler<IActivityRestrictionTemplate>(new ActivityRestrictionTemplateDomainObjectCreator(),
																				new ActivityAssembler(repositoryFactory.
																					CreateActivityRepository(uow))),
																									 new AbsenceAssembler(repositoryFactory.CreateAbsenceRepository(uow))));
				domainTemplates.ForEach(r => dtoTemplates.Add(assembler.DomainEntityToDto(r)));
			}
			return dtoTemplates;
		}

		public ICollection<ValidatedSchedulePartDto> GetValidatedSchedulePartsOnSchedulePeriod(PersonDto person, DateOnlyDto dateInPeriod, string timeZoneId)
		{
			return
				GetValidatedSchedulePartsOnSchedulePeriodByQuery(new GetValidatedSchedulePartsForPreferenceQueryDto
																					{
																						Person = person,
																						DateInPeriod = dateInPeriod,
																						TimeZoneId = timeZoneId
																					});
		}

		public ICollection<ValidatedSchedulePartDto> GetValidatedSchedulePartsOnSchedulePeriodByQuery(QueryDto queryDto)
		{
			using (var inner = _lifetimeScope.BeginLifetimeScope())
			{
				var invoker = inner.Resolve<IInvokeQuery<ICollection<ValidatedSchedulePartDto>>>();
				return invoker.Invoke(queryDto);
			}
		}

		private IStudentAvailabilityDay GetStudentAvailabilityDomainFromDto(StudentAvailabilityDayDto dto)
		{
			IStudentAvailabilityDay studentAvailabilityDay;
			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var assembler = _factoryProvider.CreateStudentAvailabilityDayAssembler();
				studentAvailabilityDay = assembler.DtoToDomainEntity(dto);
			}
			return studentAvailabilityDay;
		}
		#endregion

		#region Methods - Instance Methods - Request Contracts

		/// <summary>
		/// Gets all requests for person.
		/// </summary>
		/// <param name="person">The person.</param>
		/// <returns>
		/// A collection of <see cref="PersonRequestDto"/>.
		/// </returns>
		public ICollection<PersonRequestDto> GetAllPersonRequests(PersonDto person)
		{
			using (var inner = _lifetimeScope.BeginLifetimeScope())
			{
				using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
				{
					return _factoryProvider.CreatePersonRequestFactory(inner).GetAllRequestsForPerson(person);
				}
			}
		}

		/// <summary>
		/// Gets the requests for person.
		/// </summary>
		/// <param name="person">The person.</param>
		/// <param name="utcStartDate">The UTC start date.</param>
		/// <param name="utcEndDate">The UTC end date.</param>
		/// <returns>
		/// A collection of <see cref="PersonRequestDto"/>.
		/// </returns>
		public ICollection<PersonRequestDto> GetPersonRequests(PersonDto person, DateTime utcStartDate,
																  DateTime utcEndDate)
		{
			using (var inner = _lifetimeScope.BeginLifetimeScope())
			{
				using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
				{
					return _factoryProvider.CreatePersonRequestFactory(inner).GetPersonRequests(person, utcStartDate, utcEndDate);
				}
			}
		}

		public ICollection<PersonRequestDto> GetAllRequestModifiedWithinPeriodOrPending(PersonDto person, DateTime utcStartDate, DateTime utcEndDate)
		{
			using (var inner = _lifetimeScope.BeginLifetimeScope())
			{
				using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
				{
					return
						 _factoryProvider.CreatePersonRequestFactory(inner).GetAllRequestModifiedWithinPeriodOrPending(
							  person, utcStartDate, utcEndDate);
				}
			}
		}

		/// <summary>
		/// Deletes the person request.
		/// </summary>
		/// <param name="personRequestDto">The person request dto.</param>
		public void DeletePersonRequest(PersonRequestDto personRequestDto)
		{
			using (var inner = _lifetimeScope.BeginLifetimeScope())
			{
				using (IUnitOfWork unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
				{
					_factoryProvider.CreatePersonRequestFactory(inner).DeletePersonRequest(personRequestDto, unitOfWork);
					unitOfWork.PersistAll();
				}
			}
		}

		/// <summary>
		/// Saves the person request of type shift trade or text.
		/// </summary>
		/// <param name="personRequestDto">The person request dto.</param>
		/// <returns>
		/// A collection of <see cref="PersonRequestDto"/>.
		/// </returns>
		public PersonRequestDto SavePersonRequest(PersonRequestDto personRequestDto)
		{
			using (var inner = _lifetimeScope.BeginLifetimeScope())
			{
				var factory = _factoryProvider.CreatePersonRequestFactory(inner);
				using (IUnitOfWork unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
				{
					personRequestDto = factory.SavePersonRequest(personRequestDto, unitOfWork);
				}
				using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
				{
					return factory.GetPersonRequestDto(personRequestDto);
				}
			}
		}

		/// <summary>
		/// Creates a new shift trade request data transfer object.
		/// The request must be persisted using SavePersonRequest.
		/// </summary>
		/// <param name="requester">The requester.</param>
		/// <param name="subject">The subject.</param>
		/// <param name="message">The message.</param>
		/// <param name="shiftTradeSwapDetailDtos">The shift trade swap detail dtos containing persons and dates.</param>
		/// <returns>A <see cref="PersonRequestDto"/> for a <see cref="ShiftTradeRequestDto"/></returns>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2009-09-07
		/// </remarks>
		public PersonRequestDto CreateShiftTradeRequest(PersonDto requester, string subject, string message, ICollection<ShiftTradeSwapDetailDto> shiftTradeSwapDetailDtos)
		{
			using (var inner = _lifetimeScope.BeginLifetimeScope())
			{
				using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
				{
					return _factoryProvider.CreatePersonRequestFactory(inner).CreateShiftTradeRequest(requester, subject,
																																 message,
																																 shiftTradeSwapDetailDtos);
				}
			}
		}

		/// <summary>
		/// Sets updated details the shift trade request dto.
		/// The request must be persisted using SavePersonRequest.
		/// </summary>
		/// <param name="personRequestDto">The person request dto.</param>
		/// <param name="subject">The subject.</param>
		/// <param name="message">The message.</param>
		/// <param name="shiftTradeSwapDetailDtos">The shift trade swap detail dtos containing persons and dates.</param>
		/// <returns>A <see cref="PersonRequestDto"/> for a <see cref="ShiftTradeRequestDto"/></returns>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2009-09-07
		/// </remarks>
		public PersonRequestDto SetShiftTradeRequest(PersonRequestDto personRequestDto, string subject, string message, ICollection<ShiftTradeSwapDetailDto> shiftTradeSwapDetailDtos)
		{
			using (var inner = _lifetimeScope.BeginLifetimeScope())
			{
				using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
				{
					return _factoryProvider.CreatePersonRequestFactory(inner).SetShiftTradeRequest(personRequestDto,
																															 subject,
																															 message,
																															 shiftTradeSwapDetailDtos);
				}
			}
		}

		public PersonRequestDto UpdatePersonRequestMessage(PersonRequestDto personRequest)
		{
			using (var inner = _lifetimeScope.BeginLifetimeScope())
			{
				var factory = _factoryProvider.CreatePersonRequestFactory(inner);
				using (IUnitOfWork unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
				{
					personRequest = factory.UpdatePersonRequestMessage(personRequest, unitOfWork);
				}
				using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
				{
					return factory.GetPersonRequestDto(personRequest);
				}
			}
		}
		/// <summary>
		/// Accepts the shift trade request.
		/// </summary>
		/// <param name="personRequest">The person request.</param>
		/// <returns>A <see cref="PersonRequestDto"/>.</returns>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2009-09-07
		/// </remarks>
		public PersonRequestDto AcceptShiftTradeRequest(PersonRequestDto personRequest)
		{
			using (var inner = _lifetimeScope.BeginLifetimeScope())
			{
				var factory = _factoryProvider.CreatePersonRequestFactory(inner);
				using (IUnitOfWork unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
				{
					var person = TeleoptiPrincipal.CurrentPrincipal.GetPerson(new PersonRepository(new ThisUnitOfWork(unitOfWork)));
					personRequest = factory.AcceptShiftTradeRequest(personRequest, unitOfWork, person);
				}
				using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
				{
					return factory.GetPersonRequestDto(personRequest);
				}
			}
		}

		/// <summary>
		/// Denies the shift trade request.
		/// </summary>
		/// <param name="personRequest">The person request.</param>
		/// <returns>A <see cref="PersonRequestDto"/>.</returns>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2009-09-07
		/// </remarks>
		public PersonRequestDto DenyShiftTradeRequest(PersonRequestDto personRequest)
		{
			using (var inner = _lifetimeScope.BeginLifetimeScope())
			{
				var factory = _factoryProvider.CreatePersonRequestFactory(inner);
				using (IUnitOfWork unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
				{
					personRequest = factory.DenyShiftTradeRequest(personRequest, unitOfWork);
				}
				using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
				{
					return factory.GetPersonRequestDto(personRequest);
				}
			}
		}

		#endregion

		#region Methods - Instance Methods - Report Contracts

		/// <summary>
		/// Gets the matrix report info.
		/// </summary>
		/// <returns>
		/// A collection of <see cref="MatrixReportInfoDto"/>.
		/// </returns>
		public ICollection<MatrixReportInfoDto> GetMatrixReportInfo()
		{
			return AnalyticsReportsFactory.Create();
		}

		public void SaveSchedulePart(SchedulePartDto schedulePartDto)
		{
			throw new FaultException("Method SaveSchedulePart is no longer available. Please use newer commands to change schedule data instead!");
		}

		#endregion

		#region Methods - Instance Methods - Person Contracts

		/// <summary>
		/// Gets the site collection.
		/// </summary>
		/// <param name="applicationFunction">The application function.</param>
		/// <param name="utcDateTime">The UTC date time.</param>
		/// <returns>A collection of <see cref="SiteDto"/>.</returns>
		public ICollection<SiteDto> GetSites(ApplicationFunctionDto applicationFunction, DateTime utcDateTime)
		{
			string func = applicationFunction.FunctionPath;
			IList<SiteDto> dtos = new List<SiteDto>();
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var localDate = TimeZoneHelper.ConvertFromUtc(utcDateTime, _userTimeZone.TimeZone());
				var teamCollection = OrganizationFactory.CreateTeamCollectionLight(uow, func, new DateOnly(localDate));
				foreach (ISite site in teamCollection.AllPermittedSites)
				{
					dtos.Add(new SiteDto { DescriptionName = site.Description.Name, Id = site.Id });
				}
			}

			return dtos;
		}

		public ICollection<SiteDto> GetSitesByQuery(QueryDto queryDto)
		{
			var invoker = _lifetimeScope.Resolve<IInvokeQuery<ICollection<SiteDto>>>();
			return invoker.Invoke(queryDto);
		}

		public ICollection<BusinessUnitDto> GetBusinessUnitsByQuery(QueryDto queryDto)
		{
			var invoker = _lifetimeScope.Resolve<IInvokeQuery<ICollection<BusinessUnitDto>>>();
			return invoker.Invoke(queryDto);
		}

		/// <summary>
		/// Gets the team collection.
		/// </summary>
		/// <param name="site">The site.</param>
		/// <param name="applicationFunction">The application function.</param>
		/// <param name="utcDateTime">The UTC date time.</param>
		/// <returns>A collection of <see cref="TeamDto"/>.</returns>
		public ICollection<TeamDto> GetTeams(SiteDto site, ApplicationFunctionDto applicationFunction, DateTime utcDateTime)
		{
			var func = applicationFunction.FunctionPath;

			List<TeamDto> dtos = new List<TeamDto>();
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var localDate = TimeZoneHelper.ConvertFromUtc(utcDateTime, _userTimeZone.TimeZone());
				var teamCollection =
					OrganizationFactory.CreateTeamCollectionLight(uow, func, new DateOnly(localDate));

				foreach (ITeam team in teamCollection.AllPermittedTeams)
				{
					if (team.Site.Id == site.Id.GetValueOrDefault())
					{
						dtos.Add(new TeamDto { Description = team.Description.Name, Id = team.Id, SiteAndTeam = team.SiteAndTeam });
					}
				}
			}
			return dtos;
		}

		public ICollection<TeamDto> GetTeamsByQuery(QueryDto queryDto)
		{
			var invoker = _lifetimeScope.Resolve<IInvokeQuery<ICollection<TeamDto>>>();
			return invoker.Invoke(queryDto);
		}

		public ICollection<ScenarioDto> GetScenariosByQuery(QueryDto queryDto)
		{
			var invoker = _lifetimeScope.Resolve<IInvokeQuery<ICollection<ScenarioDto>>>();
			return invoker.Invoke(queryDto);
		}

		public ICollection<PersonOptionalValuesDto> GetPersonOptionalValuesByQuery(QueryDto queryDto)
		{
			var invoker = _lifetimeScope.Resolve<IInvokeQuery<ICollection<PersonOptionalValuesDto>>>();
			return invoker.Invoke(queryDto);
		}

		public CommandResultDto SetSchedulePeriodWorktimeOverride(
			 SetSchedulePeriodWorktimeOverrideCommandDto setSchedulePeriodWorktimeOverrideCommandDto)
		{
			return ExecuteCommand(setSchedulePeriodWorktimeOverrideCommandDto);
		}

		public ICollection<RoleDto> GetRolesByQuery(QueryDto queryDto)
		{
			var invoker = _lifetimeScope.Resolve<IInvokeQuery<ICollection<RoleDto>>>();
			return invoker.Invoke(queryDto);
		}

		public CommandResultDto GrantPersonRole(GrantPersonRoleCommandDto grantPersonRoleCommandDto)
		{
			return ExecuteCommand(grantPersonRoleCommandDto);
		}

		public CommandResultDto RevokePersonRole(RevokePersonRoleCommandDto revokePersonRoleCommandDto)
		{
			return ExecuteCommand(revokePersonRoleCommandDto);
		}

		public ICollection<ExternalLogOnDto> ExternalLogOnsByQuery(QueryDto queryDto)
		{
			var invoker = _lifetimeScope.Resolve<IInvokeQuery<ICollection<ExternalLogOnDto>>>();
			return invoker.Invoke(queryDto);
		}

		public CommandResultDto SetPersonExternalLogOn(SetPersonExternalLogOnCommandDto setPersonExternalLogOnCommandDto)
		{
			return ExecuteCommand(setPersonExternalLogOnCommandDto);
		}

		[SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public CommandResultDto ExecuteCommand(CommandDto commandDto)
		{
			var invoker = _lifetimeScope.Resolve<ICommandDispatcher>();
			try
			{
				invoker.Execute(commandDto);
			}
			catch (TargetInvocationException e)
			{
				if (e.InnerException != null)
					throw new FaultException(e.InnerException.Message);
			}
			return commandDto.Result;
		}

		public ICollection<AgentPortalSettingsDto> GetAgentPortalSettingsByQuery(QueryDto queryDto)
		{
			var invoker = _lifetimeScope.Resolve<IInvokeQuery<ICollection<AgentPortalSettingsDto>>>();
			return invoker.Invoke(queryDto);
		}

		/// <summary>
		/// Gets all permitted teams.
		/// </summary>
		/// <param name="applicationFunction">The application function.</param>
		/// <param name="utcDateTime">The UTC date time.</param>
		/// <returns></returns>
		public ICollection<TeamDto> GetAllPermittedTeams(ApplicationFunctionDto applicationFunction, DateTime utcDateTime)
		{
			var func = applicationFunction.FunctionPath;

			ITeamCollection teamCollection;
			IList<TeamDto> dtos = new List<TeamDto>();
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var localDate = TimeZoneHelper.ConvertFromUtc(utcDateTime, _userTimeZone.TimeZone());
				teamCollection = OrganizationFactory.CreateTeamCollectionLight(uow, func, new DateOnly(localDate));

				foreach (ITeam team in teamCollection.AllPermittedTeams)
				{
					dtos.Add(new TeamDto { Description = team.Description.Name, Id = team.Id, SiteAndTeam = team.SiteAndTeam });
				}
			}

			return dtos;
		}


		/// <summary>
		/// Gets the persons by team.
		/// </summary>
		/// <param name="team">The team.</param>
		/// <param name="applicationFunction">The application function.</param>
		/// <param name="utcDateTime">The UTC date time.</param>
		/// <returns>
		/// A collection of <see cref="PersonDto"/>.
		/// </returns>
		[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		public ICollection<PersonDto> GetPersonsByTeam(TeamDto team, ApplicationFunctionDto applicationFunction, DateTime utcDateTime)
		{
			var func = applicationFunction.FunctionPath;

			using (IUnitOfWork unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var localDate = new DateOnly(TimeZoneHelper.ConvertFromUtc(utcDateTime, _userTimeZone.TimeZone()));
				IPersonCollection personCollection =
					OrganizationFactory.CreatePersonCollectionLight(unitOfWork, func, localDate);
				var personDtoFactory = _lifetimeScope.Resolve<PersonDtoFactory>();
				return personDtoFactory.GetPersonsByTeam(team, utcDateTime, personCollection);
			}
		}

		/// <summary>
		/// Gets the logged on person's team.
		/// </summary>
		/// <param name="utcDate">The UTC date time.</param>
		/// <returns>A TeamDto</returns>
		public TeamDto GetLoggedOnPersonTeam(DateTime utcDate)
		{
			using (IUnitOfWork unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				IPerson person = TeleoptiPrincipal.CurrentPrincipal.GetPerson(new PersonRepository(new ThisUnitOfWork(unitOfWork)));
				DateTime localTime = GetPersonLocalTime(utcDate, person);
				DateOnly localDate = new DateOnly(localTime);
				ITeam loggedOnPersonsTeam = person.MyTeam(localDate);
				TeamDto returnTeamDto = null;
				if (loggedOnPersonsTeam != null)
					returnTeamDto = new TeamDto { Description = loggedOnPersonsTeam.Description.Name, Id = loggedOnPersonsTeam.Id, SiteAndTeam = loggedOnPersonsTeam.SiteAndTeam };
				return returnTeamDto;
			}
		}

		private static DateTime GetPersonLocalTime(DateTime utcTime, IPerson person)
		{
			TimeZoneInfo timeZone = person.PermissionInformation.DefaultTimeZone();
			return TimeZoneInfo.ConvertTimeFromUtc(utcTime, timeZone);
		}
		/// <summary>
		/// Changes the password.
		/// </summary>
		/// <param name="personDto">The person dto.</param>
		/// <param name="oldPassword">The old password.</param>
		/// <param name="newPassword">The new password.</param>
		/// <returns>
		/// 	<c>true</c> if the password was changed; otherwise, <c>false</c>.
		/// </returns>
		public bool ChangePassword(PersonDto personDto, string oldPassword, string newPassword)
		{
			var ret = _changePassword.SetNewPassword(new ChangePasswordInput
			{
				PersonId = personDto.Id.Value,
				NewPassword = newPassword,
				OldPassword = oldPassword
			}).Success;

			if (!ret)
				throw new FaultException(UserTexts.Resources.ChangePasswordValidationError);//Blir fel språk på klienten? => Isåfall hämta uiculture från state holder

			return true;
		}

		public ICollection<SkillDayDto> GetSkillDataByQuery(QueryDto queryDto)
		{
			var invoker = _lifetimeScope.Resolve<IInvokeQuery<ICollection<SkillDayDto>>>();
			return invoker.Invoke(queryDto);
		}

		/// <summary>
		/// Gets the skills.
		/// </summary>
		/// <returns>A collection of SkillDto</returns>
		public ICollection<SkillDto> GetSkills()
		{
			var returnList = new List<SkillDto>();
			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				using (var inner = _lifetimeScope.BeginLifetimeScope())
				{
					var repository = inner.Resolve<ISkillRepository>();
					var skillAssembler = inner.Resolve<IAssembler<ISkill, SkillDto>>();
					var skills = repository.LoadAll().ToList();
					returnList.AddRange(skillAssembler.DomainEntitiesToDtos(skills));
				}
			}
			return returnList;
		}

		/// <summary>
		/// Gets the skill data for given skill, date, scenario and time zone.
		/// </summary>
		/// <param name="dateOnlyDto">The date only dto.</param>
		/// <param name="timeZoneId">The time zone id e.g "W. Europe Standard Time".</param>
		/// <returns></returns>
		[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		public ICollection<SkillDayDto> GetSkillData(DateOnlyDto dateOnlyDto, string timeZoneId)
		{
			return
				GetSkillDataByQuery(new GetSkillDaysByPeriodQueryDto
				{
					Period = new DateOnlyPeriodDto { StartDate = dateOnlyDto, EndDate = dateOnlyDto },
					TimeZoneId = timeZoneId
				});
		}

		/// <summary>
		/// Gets the person team members.
		/// </summary>
		/// <param name="person">The person.</param>
		/// <param name="utcDate">The UTC date.</param>
		/// <returns>A collection of <see cref="PersonDto"/>.</returns>
		public ICollection<PersonDto> GetPersonTeamMembers(PersonDto person, DateTime utcDate)
		{
			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var personDtoFactory = _lifetimeScope.Resolve<PersonDtoFactory>();
				return personDtoFactory.GetPersonTeamMembers(person, utcDate);
			}
		}

		public ICollection<PersonPeriodDetailDto> GetPersonPeriodsByQuery(QueryDto queryDto)
		{
			var invoker = _lifetimeScope.Resolve<IInvokeQuery<ICollection<PersonPeriodDetailDto>>>();
			return invoker.Invoke(queryDto);
		}

		/// <summary>
		/// Saves the person.
		/// </summary>
		/// <param name="personDto">The person dto.</param>
		public void SavePerson(PersonDto personDto)
		{
			using (IUnitOfWork unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				PersonAssembler personAssembler = (PersonAssembler)_factoryProvider.CreatePersonAssembler();
				personAssembler.EnableSaveOrUpdate = true;
				var person = personAssembler.DtoToDomainEntity(personDto);
				unitOfWork.PersistAll();
				_tenantPeopleSaver.SaveTenantData(personDto, person.Id.Value);
			}
		}

		/// <summary>
		/// Gets the person accounts on the date.
		/// </summary>
		/// <param name="person">The person.</param>
		/// <param name="containingDate">The containing date.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: Ola
		/// Created date: 2009-06-15
		/// </remarks>
		[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		public ICollection<PersonAccountDto> GetPersonAccounts(PersonDto person, DateOnlyDto containingDate)
		{
			ICollection<PersonAccountDto> result = new Collection<PersonAccountDto>();

			using (IUnitOfWork unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var repositoryFactory = new RepositoryFactory();
				var accRep = repositoryFactory.CreatePersonAbsenceAccountRepository(unitOfWork);
				var perRep = repositoryFactory.CreatePersonRepository(unitOfWork);
				var loadedPerson = perRep.Load(person.Id.GetValueOrDefault());
				var accounts = accRep.Find(loadedPerson);
				var dateOnly = containingDate.ToDateOnly();
				var timeAssembler = new PersonAccountAssembler();
				foreach (IPersonAbsenceAccount personAbsenceAccount in accounts)
				{
					var personAccount = personAbsenceAccount.Find(dateOnly);
					if (personAccount == null) continue;
					result.Add(timeAssembler.DomainEntityToDto(personAccount));
				}
			}
			return result;
		}


		public ICollection<PersonSkillPeriodDto> GetPersonSkillPeriodsForPersons(PersonDto[] personList, DateOnlyDto startDate, DateOnlyDto endDate)
		{
			IList<PersonSkillPeriodDto> personSkillPeriodDtos;
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var personRepository = new RepositoryFactory().CreatePersonRepository(uow);
				var personCollection = personRepository.FindPeople(personList.Select(p => p.Id.GetValueOrDefault()));
				var personPeriods = from p in personCollection
										  from pp in p.PersonPeriodCollection
										  select pp;
				personSkillPeriodDtos = new PersonSkillPeriodAssembler().DomainEntitiesToDtos(personPeriods).ToList();
			}
			return personSkillPeriodDtos;
		}

		public ICollection<MultiplicatorDataDto> GetPersonMultiplicatorData(PersonDto person, DateTime utcStartTime, DateTime utcEndTime)
		{
			TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(person.TimeZoneId);
			DateOnlyDto startDate = new DateOnlyDto
										{
											DateTime = TimeZoneInfo.ConvertTimeFromUtc(utcStartTime, timeZoneInfo).Date
										};
			DateOnlyDto endDate = new DateOnlyDto
									  {
										  DateTime = TimeZoneInfo.ConvertTimeFromUtc(utcEndTime, timeZoneInfo).Date
									  };
			return GetPersonMultiplicatorDataForPerson(person, startDate, endDate);
		}

		/// <summary>
		/// Gets the application functions for person.
		/// </summary>
		/// <param name="person">The person.</param>
		/// <returns>A collection of <see cref="ApplicationFunctionDto"/>.</returns>
		public ICollection<ApplicationFunctionDto> GetApplicationFunctionsForPerson(PersonDto person)
		{
			ICollection<ApplicationFunctionDto> applicationFunctionCollectionToRetrun =
				new List<ApplicationFunctionDto>();
			List<IApplicationFunction> afUnionCollection = new List<IApplicationFunction>();

			if (((IUnsafePerson)TeleoptiPrincipal.CurrentPrincipal).Person.Id == person.Id.GetValueOrDefault(Guid.Empty))
			{
				afUnionCollection.AddRange(PrincipalAuthorization.Current().GrantedFunctions());
			}
			else
			{
				using (IUnitOfWork unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
				{
					IRepositoryFactory repositoryFactory = new RepositoryFactory();
					IPerson thePerson = repositoryFactory.CreatePersonRepository(unitOfWork).Load(person.Id.GetValueOrDefault(Guid.Empty));

					IList<IApplicationRole> applicationRoleCollection =
						thePerson.PermissionInformation.ApplicationRoleCollection;

					foreach (IApplicationRole role in applicationRoleCollection)
					{
						ICollection<IApplicationFunction> applicationFunctionCollection = role.ApplicationFunctionCollection;

						foreach (IApplicationFunction function in applicationFunctionCollection)
						{
							if (!afUnionCollection.Contains(function))
							{
								afUnionCollection.Add(function);
							}
						}
					}
				}
			}

			foreach (IApplicationFunction function in afUnionCollection)
			{
				applicationFunctionCollectionToRetrun.Add(new ApplicationFunctionDto
					{
						ForeignId = function.ForeignId,
						ForeignSource = function.ForeignSource,
						FunctionCode = function.FunctionCode,
						FunctionPath = function.FunctionPath,
						IsPreliminary = function.IsPreliminary,
						FunctionDescription = function.FunctionDescription
					});
			}

			return applicationFunctionCollectionToRetrun;
		}

		/// <summary>
		/// Gets all persons.
		/// </summary>
		/// <param name="excludeLoggedOnPerson">if set to <c>true</c> [exclude logged on person].</param>
		/// <returns>
		/// A collection of <see cref="PersonDto"/>.
		/// </returns>
		public ICollection<PersonDto> GetPersons(bool excludeLoggedOnPerson)
		{
			using (IUnitOfWork unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				using (unitOfWork.DisableFilter(QueryFilter.Deleted))
				{
					var personDtoFactory = _lifetimeScope.Resolve<PersonDtoFactory>();
					return personDtoFactory.GetPersons(excludeLoggedOnPerson);

				}
			}
		}

		/// <summary>
		/// Gets all the people with a specific query specified.
		/// </summary>
		/// <param name="queryDto">A query type for a specific property of PersonDto.</param>
		/// <returns>A list of found persons.</returns>
		public ICollection<PersonDto> GetPersonsByQuery(QueryDto queryDto)
		{
			var invoker = _lifetimeScope.Resolve<IInvokeQuery<ICollection<PersonDto>>>();
			return invoker.Invoke(queryDto);
		}

		/// <summary>
		/// Get person periods depending on the loadOptionDto
		/// </summary>
		/// <param name="loadOptionDto"></param>
		/// <param name="startDate"></param>
		/// <param name="endDate"></param>
		/// <returns></returns>
		public ICollection<PersonPeriodDetailDto> GetPersonPeriods(PersonPeriodLoadOptionDto loadOptionDto, DateOnlyDto startDate, DateOnlyDto endDate)
		{
			if (loadOptionDto == null)
				throw new ArgumentNullException(nameof(loadOptionDto));

			if (startDate == null)
				throw new ArgumentNullException(nameof(startDate));

			if (endDate == null)
				throw new ArgumentNullException(nameof(endDate));

			return
#pragma warning disable 618
				GetPersonPeriodsByQuery(new GetAllPersonPeriodsQueryDto
#pragma warning restore 618
				{
					Range = new DateOnlyPeriodDto {StartDate = startDate, EndDate = endDate}
				});
		}

		#endregion

		#region Methods - Instance Methods - Message Broker Contracts

		/// <summary>
		/// Gets the message broker configuration.
		/// </summary>
		/// <returns>A <see cref="PersonDto"/>.</returns>
		public MessageBrokerDto GetMessageBrokerConfiguration()
		{
			return MessageBrokerFactory.RetrieveMessageBrokerConfigurations();
		}

		#endregion

		public PushMessageDialogueDto GetPushMessageDialogue(PushMessageDialogueDto pushMessageDialogueDto)
		{
			PushMessageDialogueDto returnPushMessageDialogueDto = null;
			using (IUnitOfWork unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				IRepositoryFactory repositoryFactory = new RepositoryFactory();
				IPushMessageDialogueRepository repository = repositoryFactory.CreatePushMessageDialogueRepository(unitOfWork);
				IPushMessageDialogue pushMessageDialogue = repository.Get(pushMessageDialogueDto.Id.GetValueOrDefault());
				if (pushMessageDialogue != null)
				{
					var assembler = _factoryProvider.CreatePushMessageDialogueAssembler();
					returnPushMessageDialogueDto = assembler.DomainEntityToDto(pushMessageDialogue);
				}
			}
			return returnPushMessageDialogueDto;
		}

		public ICollection<PushMessageDialogueDto> GetPushMessageDialoguesNotRepliedTo(PersonDto person)
		{
			using (IUnitOfWork unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				IRepositoryFactory repositoryFactory = new RepositoryFactory();
				IPushMessageDialogueRepository repository = repositoryFactory.CreatePushMessageDialogueRepository(unitOfWork);
				var personAssembler = _factoryProvider.CreatePersonAssembler();
				IList<IPushMessageDialogue> pushMessageDialogues =
					repository.FindAllPersonMessagesNotRepliedTo(personAssembler.DtoToDomainEntity(person));

				var assembler = _factoryProvider.CreatePushMessageDialogueAssembler();
				return assembler.DomainEntitiesToDtos(pushMessageDialogues).ToList();
			}
		}

		public void SavePushMessageDialogue(PushMessageDialogueDto pushMessageDialogueDto)
		{
			using (IUnitOfWork unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				IRepositoryFactory repositoryFactory = new RepositoryFactory();
				IPushMessageDialogueRepository repository =
					repositoryFactory.CreatePushMessageDialogueRepository(unitOfWork);
				IPushMessageDialogue pushMessageDialogue = repository.Load(pushMessageDialogueDto.Id.GetValueOrDefault());
				if (pushMessageDialogue != null)
				{
					Console.WriteLine(pushMessageDialogue.ToString());
				}

				unitOfWork.PersistAll();
			}
		}

		/// <summary>
		/// Sets the dialouge reply for push messages.
		/// </summary>
		/// <param name="pushMessageDialogueDto">The push message dialogue dto.</param>
		/// <param name="dialogueReply">The dialouge reply.</param>
		/// <param name="sender">The sender.</param>
		public void SetDialogueReply(PushMessageDialogueDto pushMessageDialogueDto, string dialogueReply, PersonDto sender)
		{
				using (IUnitOfWork unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
				{
					IRepositoryFactory repositoryFactory = new RepositoryFactory();
					IPushMessageDialogueRepository pushMessageDialogueRepository =
						 repositoryFactory.CreatePushMessageDialogueRepository(unitOfWork);
					IPushMessageDialogue pushMessageDialogue = pushMessageDialogueRepository.Load(pushMessageDialogueDto.Id.Value);
					var personAssembler = _factoryProvider.CreatePersonAssembler();
					pushMessageDialogue.DialogueReply(dialogueReply, personAssembler.DtoToDomainEntity(sender));
					unitOfWork.PersistAll();
				}
		}

		/// <summary>
		/// Sets the reply for answer messages.
		/// </summary>
		/// <param name="pushMessageDialogueDto">The push message dialogue dto.</param>
		/// <param name="reply">The reply.</param>
		public void SetReply(PushMessageDialogueDto pushMessageDialogueDto, string reply)
		{
			using (IUnitOfWork unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				IRepositoryFactory repositoryFactory = new RepositoryFactory();
				IPushMessageDialogueRepository pushMessageDialogueRepository =
					repositoryFactory.CreatePushMessageDialogueRepository(unitOfWork);
				IPushMessageDialogue pushMessageDialogue =
					pushMessageDialogueRepository.Load(pushMessageDialogueDto.Id.Value);
				pushMessageDialogue.SetReply(reply);
				unitOfWork.PersistAll();
			}
		}

		public ICollection<GroupPageDto> GroupPagesByQuery(QueryDto queryDto)
		{
			var invoker = _lifetimeScope.Resolve<IInvokeQuery<ICollection<GroupPageDto>>>();
			return invoker.Invoke(queryDto);
		}

		public ICollection<GroupPageGroupDto> GroupPageGroupsByQuery(QueryDto queryDto)
		{
			var invoker = _lifetimeScope.Resolve<IInvokeQuery<ICollection<GroupPageGroupDto>>>();
			return invoker.Invoke(queryDto);
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
				Logger.Info("Disposing the service");
				//Release managed resources
				_lifetimeScope.Dispose();
			}
			//Release unmanaged resources
		}
	}
}
