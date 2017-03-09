using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo.ImportAgent;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.People.Core.Providers
{
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

		public ImportAgentsFieldOptionsModel FieldOptions()
		{
			return new ImportAgentsFieldOptionsModel
			{
				Roles = _applicationRoleRepository.LoadAll().Select(r => r.Name).ToList(),
				Teams =
					_teamRepository.LoadAll()
						.Where(
							t => _permissionProvider.HasTeamPermission(DefinedRaptorApplicationFunctionPaths.WebPeople, DateOnly.Today, t))
						.ToDictionary(t => t.Id.GetValueOrDefault(), t => t.SiteAndTeam),
				Contracts = _contractRepository.LoadAll().ToDictionary(c => c.Id.GetValueOrDefault(), c => c.Description.Name),
				ContractSchedules =
					_contractScheduleRepository.LoadAll().ToDictionary(c => c.Id.GetValueOrDefault(), c => c.Description.Name),
				ShiftBags = _ruleSetBagRepository.LoadAll().ToDictionary(r => r.Id.GetValueOrDefault(), r => r.Description.Name),
				Skills = _skillRepository.LoadAll().ToDictionary(s => s.Id.GetValueOrDefault(), s => s.Name),
				SchedulePeriodTypes =
					Enum.GetValues(typeof(SchedulePeriodType)).Cast<SchedulePeriodType>().ToDictionary(t => (int) t, t => t.ToString())
			};
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

		public IExternalLogOn FindExternalLogOn(Guid id)
		{
			return _externalLogOnRepository.Get(id);
		}

		public IExternalLogOn FindExternalLogOn(string externalLogonName)
		{
			return _externalLogOnRepository.LoadAll().FirstOrDefault(externalLogon => externalLogon.AcdLogOnName == externalLogonName);
		}

		public IApplicationRole FindRole(Guid id)
		{
			return _applicationRoleRepository.Get(id);
		}


		public IApplicationRole FindRole(string roleName)
		{			
			return _applicationRoleRepository.LoadAll().FirstOrDefault(role => role.Name == roleName);
		}

		public IContract FindContract(Guid id)
		{
			return _contractRepository.Get(id);
		}

		public IContract FindContract(string contractName)
		{			
			return _contractRepository.LoadAll().FirstOrDefault(
					contract => contract.Description.Name == contractName || contract.Description.ShortName == contractName);
		}

		public IContractSchedule FindContractSchedule(Guid id)
		{
			return _contractScheduleRepository.Get(id);
		}

		public IContractSchedule FindContractSchedule(string contractScheduleName)
		{
			return _contractScheduleRepository.LoadAll().FirstOrDefault(
				contractSchedule =>
					contractSchedule.Description.Name == contractScheduleName ||
					contractSchedule.Description.ShortName == contractScheduleName);
		}

		public IPartTimePercentage FindPartTimePercentage(Guid id)
		{
			return _partTimePercentageRepository.Get(id);
		}

		public IPartTimePercentage FindPartTimePercentage(string partTimePercentageName)
		{
			return	_partTimePercentageRepository.LoadAll().FirstOrDefault(
					partTimePercentage =>
						partTimePercentage.Description.Name == partTimePercentageName ||
						partTimePercentage.Description.ShortName == partTimePercentageName);
		}

		public IRuleSetBag FindRuleSetBag(Guid id)
		{
			return _ruleSetBagRepository.Get(id);
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

		public ITeam FindTeam(Guid id)
		{
			return _teamRepository.Get(id);
		}

		public ITeam FindTeam(ISite site, string teamName)
		{
			return _teamRepository.LoadAll().FirstOrDefault(team => team.Description.Name == teamName && team.Site.Id == site.Id);
		}

		public ISkill FindSkill(Guid id)
		{
			return _skillRepository.Get(id);
		}

		public ISkill FindSkill(string skillName)
		{
			return _skillRepository.LoadAll().FirstOrDefault(skill => skill.Name == skillName);
		}

	}
}