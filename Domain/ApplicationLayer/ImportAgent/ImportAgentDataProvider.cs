using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ImportAgent
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
				Roles = _applicationRoleRepository.LoadAllApplicationRolesSortedByName().Select(r => new FieldOptionViewModel
				{
					Id = r.Id.GetValueOrDefault(),
					Name = r.DescriptionText
				}).ToList(),

				Teams =
					_teamRepository.LoadAll()
						.Where(
							t => _permissionProvider.HasTeamPermission(DefinedRaptorApplicationFunctionPaths.WebPeople, DateOnly.Today, t))
						.ToDictionary(t => t.Id.GetValueOrDefault(), t => t.SiteAndTeam),

				Contracts = _contractRepository.LoadAll().Select(c => new FieldOptionViewModel
				{
					Id = c.Id.GetValueOrDefault(),
					Name = c.Description.Name
				}).ToList(),

				ContractSchedules = _contractScheduleRepository.LoadAll().Select(c => new FieldOptionViewModel
				{
					Id = c.Id.GetValueOrDefault(),
					Name = c.Description.Name
				}).ToList(),

				ShiftBags = _ruleSetBagRepository.LoadAll().Select(r => new FieldOptionViewModel
				{
					Id = r.Id.GetValueOrDefault(),
					Name = r.Description.Name
				}).ToList(),

				Skills = _skillRepository.FindAllWithoutMultisiteSkills().Select(s => new FieldOptionViewModel
				{
					Id = s.Id.GetValueOrDefault(),
					Name = s.Name
				}).ToList(),

				SchedulePeriodTypes = EnumExtensions.GetValues(SchedulePeriodType.ChineseMonth)
									.ToDictionary(t => (int)t, t => t.ToString()),

				PartTimePercentages = _partTimePercentageRepository.LoadAll().OrderBy(p => p.Percentage.Value).Select(p => new FieldOptionViewModel
				{
					Id = p.Id.GetValueOrDefault(),
					Name = p.Description.Name
				}).ToList(),

				ExternalLogons = _externalLogOnRepository.LoadAll().Select(e => new FieldOptionViewModel
				{
					Id = e.Id.GetValueOrDefault(),
					Name = e.AcdLogOnName
				}).ToList()
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
			if (roleName.IsNullOrEmpty())
			{
				return null;
			}
			return _applicationRoleRepository.LoadAll().FirstOrDefault(role => role.DescriptionText?.Trim().ToLowerInvariant() == roleName.Trim().ToLowerInvariant());
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
			return _partTimePercentageRepository.LoadAll().FirstOrDefault(
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
			return _skillRepository.LoadAll().FirstOrDefault(skill => skill.Name.ToLowerInvariant() == skillName.ToLowerInvariant());
		}

	}
}