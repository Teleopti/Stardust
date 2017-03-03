using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo.ImportAgent
{
	public interface IImportAgentDataProvider
	{
		void Init();
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

		private IList<IApplicationRole> _applicationRoles;
		private IList<IContract> _contracts;
		private IList<IContractSchedule> _contractSchedules;
		private IList<IPartTimePercentage> _partTimePercentages;
		private IList<IRuleSetBag> _ruleSetBags;
		private IList<ISkill> _skills;
		private IList<ISite> _sites;
		private IList<ITeam> _teams;
		private IList<IExternalLogOn> _externalLogOns;

		public ImportAgentDataProvider(IApplicationRoleRepository applicationRoleRepository, IContractRepository contractRepository, IContractScheduleRepository contractScheduleRepository, IPartTimePercentageRepository partTimePercentageRepository, IRuleSetBagRepository ruleSetBagRepository, ISkillRepository skillRepository, ISiteRepository siteRepository, ITeamRepository teamRepository, IExternalLogOnRepository externalLogOnRepository)
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
		}

		public void Init()
		{
			_applicationRoles = _applicationRoleRepository.LoadAll();
			_contracts = _contractRepository.LoadAll();
			_contractSchedules = _contractScheduleRepository.LoadAll();
			_partTimePercentages = _partTimePercentageRepository.LoadAll();
			_ruleSetBags = _ruleSetBagRepository.LoadAll();
			_skills = _skillRepository.LoadAll();
			_sites = _siteRepository.LoadAll();
			_teams = _teamRepository.LoadAll();
			_externalLogOns = _externalLogOnRepository.LoadAll();
		}

		public ImportAgentSettingsDataModel GetImportAgentSettingsData()
		{
			var settingData = new ImportAgentSettingsDataModel()
			{
				Roles = _applicationRoles.ToList(),
				StartDate = new DateOnly(),
				Teams = _teams.ToList(),
				Skills = _skills.ToList(),
				ExternalLogons = _externalLogOns.ToList(),
				Contracts = _contracts.ToList(),
				ContractSchedules = _contractSchedules.ToList(),
				PartTimePercentages = _partTimePercentages.ToList(),
				ShiftBags = _ruleSetBags.ToList(),
				SchedulePeriodTypes = Enum.GetValues(typeof(SchedulePeriodType)).Cast<SchedulePeriodType>().ToList(),
				SchedulePeriodLength = 1
			};
			return settingData;
		}

		public IExternalLogOn FindExternalLogOn(string externalLogonName)
		{
			return _externalLogOns.FirstOrDefault(externalLogon => externalLogon.AcdLogOnName == externalLogonName);
		}

		public IApplicationRole FindRole(string roleName)
		{			
			return _applicationRoles.FirstOrDefault(role => role.Name == roleName);
		}

		public IContract FindContract(string contractName)
		{			
			return _contracts.FirstOrDefault(
					contract => contract.Description.Name == contractName || contract.Description.ShortName == contractName);
		}

		public IContractSchedule FindContractSchedule(string contractScheduleName)
		{
			return _contractSchedules.FirstOrDefault(
				contractSchedule =>
					contractSchedule.Description.Name == contractScheduleName ||
					contractSchedule.Description.ShortName == contractScheduleName);
		}

		public IPartTimePercentage FindPartTimePercentage(string partTimePercentageName)
		{
			return	_partTimePercentages.FirstOrDefault(
					partTimePercentage =>
						partTimePercentage.Description.Name == partTimePercentageName ||
						partTimePercentage.Description.ShortName == partTimePercentageName);
		}

		public IRuleSetBag FindRuleSetBag(string ruleSetBagName)
		{
			return
				_ruleSetBags.FirstOrDefault(
					ruleSetBag => ruleSetBag.Description.Name == ruleSetBagName || ruleSetBag.Description.ShortName == ruleSetBagName);
		}

		public ISite FindSite(string siteName)
		{
			return _sites.FirstOrDefault(site => site.Description.Name == siteName);
		}

		public ITeam FindTeam(ISite site, string teamName)
		{
			return _teams.FirstOrDefault(team => team.Description.Name == teamName && team.Site.Id == site.Id);
		}

		public ISkill FindSkill(string skillName)
		{
			return _skills.FirstOrDefault(skill => skill.Name == skillName);
		}

	}
}