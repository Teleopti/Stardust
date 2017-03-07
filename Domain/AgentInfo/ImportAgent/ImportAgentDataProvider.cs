using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo.ImportAgent
{
	public interface IImportAgentDataProvider
	{
		ImportAgentSettingsDataModel GetImportAgentSettingsData();
		IApplicationRole FindRole(string roleName);
		IContract FindContract(string contractName);
		IContractSchedule FindContractSchedule(string contractScheduleName);
		IPartTimePercentage FindPartTimePercentage(string partTimePercentageName);
		IRuleSetBag FindRuleSetBag(string ruleSetBagName);
		ISkill FindSkill(string skillName);
		ISite FindSite(string siteName);
		ITeam FindTeam(ISite site, string teamName);
		IExternalLogOn FindExternalLogOn(string externalLogonName);
	}

	public class ImportAgentDataProvider : IImportAgentDataProvider
	{
		private readonly IApplicationRoleRepository _applicationRoleRepository;
		private readonly IContractRepository _contractRepository;
		private readonly IContractScheduleRepository _contractScheduleRepository;
		private readonly IPartTimePercentageRepository _partTimePercentageRepository;
		private readonly IRuleSetBagRepository _ruleSetBagRepository;
		private readonly ISkillRepository _skillRepository;
		private readonly ISiteRepository _siteRepository;
		private readonly ITeamRepository _teamRepository;
		private readonly IExternalLogOnRepository _externalLogOnRepository;

		private readonly IPermissionProvider _permissionProvider;

		public ImportAgentDataProvider(IApplicationRoleRepository applicationRoleRepository, IContractRepository contractRepository, IContractScheduleRepository contractScheduleRepository, 
			IPartTimePercentageRepository partTimePercentageRepository, IRuleSetBagRepository ruleSetBagRepository, ISkillRepository skillRepository, 
			ISiteRepository siteRepository, ITeamRepository teamRepository, IExternalLogOnRepository externalLogOnRepository, IPermissionProvider permissionProvider)
		{
			_applicationRoleRepository = applicationRoleRepository;
			_contractRepository = contractRepository;
			_contractScheduleRepository = contractScheduleRepository;
			_partTimePercentageRepository = partTimePercentageRepository;
			_ruleSetBagRepository = ruleSetBagRepository;
			_skillRepository = skillRepository;
			_siteRepository = siteRepository;
			_teamRepository = teamRepository;
			_externalLogOnRepository = externalLogOnRepository;
			_permissionProvider = permissionProvider;
		}
		public ImportAgentSettingsDataModel GetImportAgentSettingsData()
		{
			var applicationRoles = _applicationRoleRepository.LoadAll();
			var contracts = _contractRepository.LoadAll();
			var contractSchedules = _contractScheduleRepository.LoadAll();
			var partTimePercentages = _partTimePercentageRepository.LoadAll();
			var ruleSetBags = _ruleSetBagRepository.LoadAll();
			var skills = _skillRepository.LoadAll();
			var externalLogOns = _externalLogOnRepository.LoadAll();
			var settingData = new ImportAgentSettingsDataModel()
			{
				Roles = applicationRoles.ToList(),
				Teams = GetPermittedTeams(),
				Skills = skills.ToList(),
				ExternalLogons = externalLogOns.ToList(),
				Contracts = contracts.ToList(),
				ContractSchedules = contractSchedules.ToList(),
				PartTimePercentages = partTimePercentages.ToList(),
				ShiftBags = ruleSetBags.ToList(),
				SchedulePeriodTypes = Enum.GetValues(typeof(SchedulePeriodType)).Cast<SchedulePeriodType>().ToList(),
				SchedulePeriodLength = 1
			};
			return settingData;
		}

		public List<TeamViewModel> GetPermittedTeams()
		{
			var teams = _teamRepository.LoadAll();
			var permittedTeamList = new List<TeamViewModel>();

			foreach (var team in teams)
			{
				if (_permissionProvider.HasTeamPermission(DefinedRaptorApplicationFunctionPaths.WebPeople, DateOnly.Today, team))
				{
					permittedTeamList.Add(new TeamViewModel
					{
						Id = team.Id.Value.ToString(),
						SiteAndTeam = team.SiteAndTeam
					});
				}
			}

			return permittedTeamList.OrderBy(t => t.SiteAndTeam).ToList();
		}

		public IExternalLogOn FindExternalLogOn(string externalLogonName)
		{
			return _externalLogOnRepository.LoadAll().FirstOrDefault(externalLogon => externalLogon.AcdLogOnName == externalLogonName);
		}

		public IApplicationRole FindRole(string roleName)
		{			
			return _applicationRoleRepository.LoadAll().FirstOrDefault(role => role.Name == roleName);
		}

		public IContract FindContract(string contractName)
		{			
			return _contractRepository.LoadAll().FirstOrDefault(
					contract => contract.Description.Name == contractName || contract.Description.ShortName == contractName);
		}

		public IContractSchedule FindContractSchedule(string contractScheduleName)
		{
			return _contractScheduleRepository.LoadAll().FirstOrDefault(
				contractSchedule =>
					contractSchedule.Description.Name == contractScheduleName ||
					contractSchedule.Description.ShortName == contractScheduleName);
		}

		public IPartTimePercentage FindPartTimePercentage(string partTimePercentageName)
		{
			return	_partTimePercentageRepository.LoadAll().FirstOrDefault(
					partTimePercentage =>
						partTimePercentage.Description.Name == partTimePercentageName ||
						partTimePercentage.Description.ShortName == partTimePercentageName);
		}

		public IRuleSetBag FindRuleSetBag(string ruleSetBagName)
		{
			return
				_ruleSetBagRepository.LoadAll().FirstOrDefault(
					ruleSetBag => ruleSetBag.Description.Name == ruleSetBagName || ruleSetBag.Description.ShortName == ruleSetBagName);
		}

		public ISite FindSite(string siteName)
		{
			return _siteRepository.LoadAll().FirstOrDefault(site => site.Description.Name == siteName);
		}

		public ITeam FindTeam(ISite site, string teamName)
		{
			return _teamRepository.LoadAll().FirstOrDefault(team => team.Description.Name == teamName && team.Site.Id == site.Id);
		}

		public ISkill FindSkill(string skillName)
		{
			return _skillRepository.LoadAll().FirstOrDefault(skill => skill.Name == skillName);
		}

	}
}