using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Kpi;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Wfm.Administration.Core
{
	public class CreateBusinessUnit
	{
		private readonly IDataSourceScope _dataSourceScope;
		private readonly IDataSourcesFactory _dataSourcesFactory;
		private readonly IUpdatedByScope _updatedByScope;
		private readonly IBusinessUnitScope _businessUnitScope;
		private readonly ITransactionHooksScope _transactionHooksScope;

		public CreateBusinessUnit(IDataSourceScope dataSourceScope, 
			IDataSourcesFactory dataSourcesFactory, 
			IUpdatedByScope updatedByScope, 
			IBusinessUnitScope businessUnitScope, 
			ITransactionHooksScope transactionHooksScope)
		{
			_dataSourceScope = dataSourceScope;
			_dataSourcesFactory = dataSourcesFactory;
			_updatedByScope = updatedByScope;
			_businessUnitScope = businessUnitScope;
			_transactionHooksScope = transactionHooksScope;
		}

		public void Create(Tenant tenant, string businessUnitName)
		{
			var dataSource = _dataSourcesFactory.Create(tenant.Name, tenant.DataSourceConfiguration.ApplicationConnectionString,
				tenant.DataSourceConfiguration.AnalyticsConnectionString);

			var newBusinessUnit = new BusinessUnit(businessUnitName);
			IPerson systemUser = null;
			withGlobalScope(dataSource, uow =>
			{
				var businessUnitRepository = new BusinessUnitRepository(uow);
				var personRepository = new PersonRepository(uow);
				businessUnitRepository.Add(newBusinessUnit);
				systemUser = personRepository.Get(SystemUser.Id); // This requires that the system user already exists!
				//var initializeLazyLoadedCollection = systemUser.PermissionInformation.ApplicationRoleCollection; // Load lazy loaded collection needs to be loaded within session
			});

			withBusinessUnitScope(dataSource, newBusinessUnit, uow =>
			{
				var scenarioRepository = new ScenarioRepository(uow);
				var applicationRoleRepository = new ApplicationRoleRepository(uow);
				var availableDataRepository = new AvailableDataRepository(uow);
				var kpiRepository = new KpiRepository(uow);
				var skillTypeRepository = new SkillTypeRepository(uow);
				var personRepository = new PersonRepository(uow);

				var scenario = new Scenario("Default")
				{
					DefaultScenario = true,
					EnableReporting = true,
					Restricted = false
				};
				var existingKpis = kpiRepository.LoadAll();
				var existingSkillTypes = skillTypeRepository.LoadAll();
				foreach (var kpi in buildKpis())
				{
					if (existingKpis.Any(x => x.ResourceKey == kpi.ResourceKey))
						continue;
					kpiRepository.Add(kpi);
				}
				
				var administratorRole = buildRole(ShippedApplicationRoleNames.AdministratorRole, ShippedCustomRoles.xxBuiltInAdministratorRole.ToString(), false, newBusinessUnit);
				var businessUnitAdministratorRole = buildRole(ShippedApplicationRoleNames.BusinessUnitAdministratorRole, ShippedCustomRoles.xxBuiltInBusinessUnitAdministratorRole.ToString(), false, newBusinessUnit);
				var siteManagerRole = buildRole(ShippedApplicationRoleNames.SiteManagerRole, ShippedCustomRoles.xxBuiltInSiteManagerRole.ToString(), false, newBusinessUnit);
				var teamLeaderRole = buildRole(ShippedApplicationRoleNames.TeamLeaderRole, ShippedCustomRoles.xxBuiltInTeamLeaderRole.ToString(), false, newBusinessUnit);
				var agentRole = buildRole(ShippedApplicationRoleNames.AgentRole, ShippedCustomRoles.xxBuildInStandardAgentRole.ToString(), false, newBusinessUnit);
				var administratorData = buildAvailableData(administratorRole, AvailableDataRangeOption.Everyone);
				var businessUnitAdministratorData = buildAvailableData(businessUnitAdministratorRole, AvailableDataRangeOption.MyBusinessUnit);
				var siteManagerData = buildAvailableData(siteManagerRole, AvailableDataRangeOption.MySite);
				var teamLeaderData = buildAvailableData(teamLeaderRole, AvailableDataRangeOption.MyOwn);
				var agentData = buildAvailableData(agentRole, AvailableDataRangeOption.MyTeam);
				scenarioRepository.Add(scenario);
				applicationRoleRepository.AddRange(new []{ administratorRole, businessUnitAdministratorRole, siteManagerRole, teamLeaderRole, agentRole });
				availableDataRepository.AddRange(new []{ administratorData , businessUnitAdministratorData , siteManagerData, teamLeaderData, agentData });
				foreach (var skillType in buildSkillTypes())
				{
					if (existingSkillTypes.Any(x => x.Description.Name == skillType.Description.Name))
						continue;
					skillTypeRepository.Add(skillType);
				}
				if (systemUser != null)
				{
					uow.Current().Reassociate(systemUser);
					systemUser.PermissionInformation.AddApplicationRole(administratorRole);
				}
				personRepository.Add(systemUser);
			});
		}

		private static IEnumerable<ISkillType> buildSkillTypes()
		{
			return new List<ISkillType>
			{
				createSkillType(new Description("SkillTypeInboundTelephony"), ForecastSource.InboundTelephony),
				createSkillType(new Description("SkillTypeTime"), ForecastSource.Time),
				createSkillType(new Description("SkillTypeEmail"), ForecastSource.Email),
				createSkillType(new Description("SkillTypeBackoffice"), ForecastSource.Backoffice),
				createSkillType(new Description("SkillTypeProject"), ForecastSource.Time),
				createSkillType(new Description("SkillTypeFax"), ForecastSource.Facsimile)
			};
		}
		private static ISkillType createSkillType(Description description, ForecastSource forecastSource)
		{
			var skillType = ForecastSource.InboundTelephony == forecastSource
				? (ISkillType) new SkillTypePhone(description, forecastSource)
				: new SkillTypeEmail(description, forecastSource);
			return skillType;
		}

		private static IEnumerable<IKeyPerformanceIndicator> buildKpis()
		{
			return new List<IKeyPerformanceIndicator>
			{
				new KeyPerformanceIndicator("Readiness (%)", "KpiReadiness", EnumTargetValueType.TargetValueTypePercent, 80, 75, 80,
					Color.FromArgb(-256), Color.FromArgb(-65536), Color.FromArgb(-16744448)),
				new KeyPerformanceIndicator("Average After Call Work (s)", "KpiAverageAfterCallWork", EnumTargetValueType.TargetValueTypeNumber, 20, 0, 40, Color.FromArgb(-16744448), Color.FromArgb(-16744448), Color.FromArgb(-65536)),
				new KeyPerformanceIndicator("Average Handle Time (s)", "KpiAverageHandleTime", EnumTargetValueType.TargetValueTypeNumber, 140, 30, 180, Color.FromArgb(-16744448), Color.FromArgb(-256), Color.FromArgb(-65536)),
				new KeyPerformanceIndicator("Answered Calls per Scheduled Phone Hour", "KpiAnsweredCallsPerScheduledPhoneHour", EnumTargetValueType.TargetValueTypeNumber, 25, 0, 10, Color.FromArgb(-256), Color.FromArgb(-65536), Color.FromArgb(-16744448)),
				new KeyPerformanceIndicator("Adherence (%)", "KpiAdherence", EnumTargetValueType.TargetValueTypePercent, 80, 75, 80, Color.FromArgb(-256), Color.FromArgb(-65536), Color.FromArgb(-16744448)),
				new KeyPerformanceIndicator("Average Talk Time (s)", "KpiAverageTalkTime", EnumTargetValueType.TargetValueTypeNumber, 120, 30, 160, Color.FromArgb(-16744448), Color.FromArgb(-256), Color.FromArgb(-65536)),
				new KeyPerformanceIndicator("Absenteeism (%)", "KpiAbsenteeism", EnumTargetValueType.TargetValueTypePercent, 5, 4, 6, Color.FromArgb(-256), Color.FromArgb(-16744448), Color.FromArgb(-65536))
			};
		}

		private static ApplicationRole buildRole(string name, string description, bool builtIn, BusinessUnit businessUnit)
		{
			var applicationRole = new ApplicationRole
			{
				Name = name,
				DescriptionText = description,
				BuiltIn = builtIn
			};
			applicationRole.SetBusinessUnit(businessUnit);
			return applicationRole;
		}

		private static AvailableData buildAvailableData(IApplicationRole role, AvailableDataRangeOption dataRangeOption)
		{
			return new AvailableData
			{
				ApplicationRole = role,
				AvailableDataRange = dataRangeOption
			};
		}

		private void withGlobalScope(IDataSource dataSource, Action<ICurrentUnitOfWork> action)
		{
			using (_dataSourceScope.OnThisThreadUse(dataSource))
			using (_transactionHooksScope.OnThisThreadExclude<MessageBrokerSender>())
			{
				var updatedBy = new Person();
				updatedBy.SetId(SystemUser.Id);
				_updatedByScope.OnThisThreadUse(updatedBy);

				using (var uow = CurrentUnitOfWorkFactory.Make().Current().CreateAndOpenUnitOfWork())
				{
					action(new ThisUnitOfWork(uow));
					uow.PersistAll();
					uow.Flush();
					uow.Clear();
				}
				_updatedByScope.OnThisThreadUse(null);
			}
		}

		private void withBusinessUnitScope(IDataSource dataSource, IBusinessUnit businessUnit, Action<ICurrentUnitOfWork> action)
		{
			_businessUnitScope.OnThisThreadUse(businessUnit);
			withGlobalScope(dataSource, action);
			_businessUnitScope.OnThisThreadUse(null);
		}
	}
}