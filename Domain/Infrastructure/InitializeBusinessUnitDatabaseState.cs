using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Kpi;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.Infrastructure
{
	public class InitializeBusinessUnitDatabaseState
	{
		private readonly IPersonRepository _personRepository;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IApplicationRoleRepository _applicationRoleRepository;
		private readonly IAvailableDataRepository _availableDataRepository;
		private readonly IKpiRepository _kpiRepository;
		private readonly ISkillTypeRepository _skillTypeRepository;
		private readonly ICurrentUnitOfWork _currentUnitOfWork;

		public InitializeBusinessUnitDatabaseState(IPersonRepository personRepository,
			IScenarioRepository scenarioRepository,
			IApplicationRoleRepository applicationRoleRepository,
			IAvailableDataRepository availableDataRepository,
			IKpiRepository kpiRepository,
			ISkillTypeRepository skillTypeRepository,
			ICurrentUnitOfWork currentUnitOfWork)
		{
			_personRepository = personRepository;
			_scenarioRepository = scenarioRepository;
			_applicationRoleRepository = applicationRoleRepository;
			_availableDataRepository = availableDataRepository;
			_kpiRepository = kpiRepository;
			_skillTypeRepository = skillTypeRepository;
			_currentUnitOfWork = currentUnitOfWork;
		}
		
		public void Execute(IBusinessUnit newBusinessUnit, IPerson systemUser)
		{
			createMissingKpis(_kpiRepository);

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
			_scenarioRepository.Add(buildScenario());
			_applicationRoleRepository.AddRange(new[] { administratorRole, businessUnitAdministratorRole, siteManagerRole, teamLeaderRole, agentRole });
			_availableDataRepository.AddRange(new[] { administratorData, businessUnitAdministratorData, siteManagerData, teamLeaderData, agentData });

			createMissingSkillTypes(_skillTypeRepository);

			if (systemUser != null)
			{
				_currentUnitOfWork.Current().Reassociate(systemUser);
				systemUser.PermissionInformation.AddApplicationRole(administratorRole);
			}
			_personRepository.Add(systemUser);
		}
		
		private static void createMissingSkillTypes(ISkillTypeRepository skillTypeRepository)
		{
			var existingSkillTypes = skillTypeRepository.LoadAll();
			foreach (var skillType in buildSkillTypes())
			{
				if (existingSkillTypes.Any(x => x.Description.Name == skillType.Description.Name))
					continue;
				skillTypeRepository.Add(skillType);
			}
		}

		private static void createMissingKpis(IKpiRepository kpiRepository)
		{
			var existingKpis = kpiRepository.LoadAll();
			foreach (var kpi in buildKpis())
			{
				if (existingKpis.Any(x => x.ResourceKey == kpi.ResourceKey))
					continue;
				kpiRepository.Add(kpi);
			}
		}

		private static IScenario buildScenario()
		{
			return new Scenario("Default")
			{
				DefaultScenario = true,
				EnableReporting = true,
				Restricted = false
			};
		}

		private static IEnumerable<ISkillType> buildSkillTypes()
		{
			return new List<ISkillType>
			{
				createSkillType("SkillTypeInboundTelephony", ForecastSource.InboundTelephony),
				createSkillType("SkillTypeTime", ForecastSource.Time),
				createSkillType("SkillTypeEmail", ForecastSource.Email),
				createSkillType("SkillTypeBackoffice", ForecastSource.Backoffice),
				createSkillType("SkillTypeProject", ForecastSource.Time),
				createSkillType("SkillTypeFax", ForecastSource.Facsimile)
			};
		}

		private static ISkillType createSkillType(string desc, ForecastSource forecastSource)
		{
			var description = new Description(desc);
			var skillType = (ISkillType) new SkillTypeEmail(description, forecastSource);
			if (ForecastSource.InboundTelephony == forecastSource)
			{
				skillType = new SkillTypePhone(description, forecastSource);
			}
			
			return skillType;
		}

		private static IEnumerable<IKeyPerformanceIndicator> buildKpis()
		{
			var yellow = Color.FromArgb(-256);
			var red = Color.FromArgb(-65536);
			var green = Color.FromArgb(-16744448);
			const EnumTargetValueType number = EnumTargetValueType.TargetValueTypeNumber;
			const EnumTargetValueType percent = EnumTargetValueType.TargetValueTypePercent;
			return new List<IKeyPerformanceIndicator>
			{
				new KeyPerformanceIndicator("Readiness (%)", nameof(Resources.KpiReadiness), percent, 80, 75, 80, yellow, red, green),
				new KeyPerformanceIndicator("Average After Call Work (s)",  nameof(Resources.KpiAverageAfterCallWork), number, 20, 0, 40, green, green, red),
				new KeyPerformanceIndicator("Average Handle Time (s)",  nameof(Resources.KpiAverageHandleTime), number, 140, 30, 180, green, yellow, red),
				new KeyPerformanceIndicator("Answered Calls per Scheduled Phone Hour",  nameof(Resources.KpiAnsweredCallsPerScheduledPhoneHour), number, 25, 0, 10, yellow, red, green),
				new KeyPerformanceIndicator("Adherence (%)",  nameof(Resources.KpiAdherence), percent, 80, 75, 80, yellow, red, green),
				new KeyPerformanceIndicator("Average Talk Time (s)",  nameof(Resources.KpiAverageTalkTime), number, 120, 30, 160, green, yellow, red),
				new KeyPerformanceIndicator("Absenteeism (%)", nameof(Resources.KpiAbsenteeism), percent, 5, 4, 6, yellow, green, red)
			};
		}

		private static IApplicationRole buildRole(string name, string description, bool builtIn, IBusinessUnit businessUnit)
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

		private static IAvailableData buildAvailableData(IApplicationRole role, AvailableDataRangeOption dataRangeOption)
		{
			return new AvailableData
			{
				ApplicationRole = role,
				AvailableDataRange = dataRangeOption
			};
		}
	}
}