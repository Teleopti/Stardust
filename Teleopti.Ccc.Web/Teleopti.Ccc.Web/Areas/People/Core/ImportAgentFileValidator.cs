using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Ajax.Utilities;
using NPOI.HPSF;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Util;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.People.Core.Models;
using Teleopti.Ccc.Web.Areas.People.Core.Providers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.People.Core
{
	public class ImportAgentFileValidator : IImportAgentFileValidator
	{
		private const int maxNameLength = 25;
		private const int maxWindowUserLength = 100;
		private const int maxApplicationUserIdLength = 50;

		private defaultAgentDataModel _defaultValues;

		private readonly IImportAgentDataProvider _importAgentDataProvider;

		public ImportAgentFileValidator(IImportAgentDataProvider importAgentDataProvider)
		{
			_importAgentDataProvider = importAgentDataProvider;
		}

		public void SetDefaultValues(ImportAgentFormData defaultValues)
		{
			if(defaultValues == null) return;

			_defaultValues = new defaultAgentDataModel();
			if (!defaultValues.RoleIds.IsNullOrWhiteSpace())
			{
				var roleIds = StringHelper.SplitStringList(defaultValues.RoleIds);
				foreach (var roleIdString in roleIds)
				{
					Guid roleId;
					if (!Guid.TryParse(roleIdString, out roleId)) continue;
					var role = _importAgentDataProvider.FindRole(roleId);
					if (role != null)
					{
						_defaultValues.Roles.Add(role);
					}
				}
			}

			if (!defaultValues.StartDate.IsNullOrWhiteSpace())
			{
				DateTime startDate;
				if (DateTime.TryParse(defaultValues.StartDate, out startDate))
				{
					_defaultValues.StartDate = new DateOnly(startDate);
				}
			}

			if (!defaultValues.TeamId.IsNullOrWhiteSpace())
			{
				Guid teamId;
				if (Guid.TryParse(defaultValues.TeamId, out teamId))
				{
					var team = _importAgentDataProvider.FindTeam(teamId);
					_defaultValues.Team = team;
				}
			}

			if (!defaultValues.SkillIds.IsNullOrWhiteSpace())
			{
				var skillIds = StringHelper.SplitStringList(defaultValues.SkillIds);

				foreach (var skillIdString in skillIds)
				{
					Guid skillId;
					if (Guid.TryParse(skillIdString, out skillId))
					{
						var skill = _importAgentDataProvider.FindSkill(skillId);
						if (skill != null)
						{
							_defaultValues.Skills.Add(skill);
						}
					}
				}
			}

			if (!defaultValues.ExternalLogonId.IsNullOrWhiteSpace())
			{
				Guid externalLogonId;
				if (Guid.TryParse(defaultValues.ExternalLogonId, out externalLogonId))
				{
					var externalLogon = _importAgentDataProvider.FindExternalLogOn(externalLogonId);
					if (externalLogon != null)
					{
						_defaultValues.ExternalLogon = externalLogon;
					}
				}
			}

			if (!defaultValues.PartTimePercentageId.IsNullOrWhiteSpace())
			{
				Guid partTimePercentageId;
				if (Guid.TryParse(defaultValues.PartTimePercentageId, out partTimePercentageId))
				{
					var partTimePercentage = _importAgentDataProvider.FindPartTimePercentage(partTimePercentageId);
					if (partTimePercentage != null)
					{
						_defaultValues.PartTimePercentage = partTimePercentage;
					}
				}
			}

			if (!defaultValues.ContractId.IsNullOrWhiteSpace())
			{
				Guid contractId;
				if (Guid.TryParse(defaultValues.ContractId, out contractId))
				{
					var contract = _importAgentDataProvider.FindContract(contractId);
					if (contract != null)
					{
						_defaultValues.Contract = contract;
					}
				}
			}

			if (!defaultValues.ContractScheduleId.IsNullOrWhiteSpace())
			{
				Guid contractScheduleId;
				if (Guid.TryParse(defaultValues.ContractScheduleId, out contractScheduleId))
				{
					var contractSchedule = _importAgentDataProvider.FindContractSchedule(contractScheduleId);
					if (contractSchedule != null)
					{
						_defaultValues.ContractSchedule = contractSchedule;
					}
				}
			}

			if (!defaultValues.ShiftBagId.IsNullOrWhiteSpace())
			{
				Guid shiftBagId;
				if (Guid.TryParse(defaultValues.ShiftBagId, out shiftBagId))
				{
					var ruleSetBag = _importAgentDataProvider.FindRuleSetBag(shiftBagId);
					if (ruleSetBag != null)
					{
						_defaultValues.RuleSetBag = ruleSetBag;
					}
				}
			}

			if (!defaultValues.SchedulePeriodType.IsNullOrWhiteSpace())
			{
				SchedulePeriodType schedulePeriodType;
				if (Enum.TryParse(defaultValues.SchedulePeriodType, out schedulePeriodType))
				{
					_defaultValues.SchedulePeriodType = schedulePeriodType;
				}
			}

			if (!defaultValues.SchedulePeriodLength.IsNullOrWhiteSpace())
			{
				int schedulePeriodLength;
				if (int.TryParse(defaultValues.SchedulePeriodLength, out schedulePeriodLength))
				{
					_defaultValues.SchedulePeriodLength = schedulePeriodLength;
				}
			}
		}



		public AgentDataModel MapRawData(RawAgent raw, out Feedback feedback)
		{
			var agentInfo = new AgentDataModel();
			feedback = new Feedback();

			agentInfo.Firstname = raw.Firstname;
			agentInfo.Lastname = raw.Lastname;

			feedback.Merge(parseFirstnameAndLastname(raw.Firstname, raw.Lastname, agentInfo));
			feedback.Merge(parseWindowsUserAndApplicationLogonId(raw.WindowsUser, raw.ApplicationUserId, agentInfo));
			feedback.Merge(parsePassword(raw.Password, agentInfo));
			feedback.Merge(parseRole(raw.Role, agentInfo));
			feedback.Merge(parseStartDate(raw.StartDate, agentInfo));

			feedback.Merge(parseOrganization(raw.Organization, agentInfo));
			feedback.Merge(parseSkill(raw.Skill, agentInfo));
			feedback.Merge(parseExternalLogon(raw.ExternalLogon, agentInfo));
			feedback.Merge(parseContract(raw.Contract, agentInfo));
			feedback.Merge(parseContractSchedule(raw.ContractSchedule, agentInfo));
			feedback.Merge(parsePartTimePercentage(raw.PartTimePercentage, agentInfo));
			feedback.Merge(parseRuleSetBag(raw.ShiftBag, agentInfo));
			feedback.Merge(parseSchedulePeriodType(raw.SchedulePeriodType, agentInfo));
			feedback.Merge(parseSchedulePeriodLength((int)raw.SchedulePeriodLength, agentInfo));

			return agentInfo;
		}

		private Feedback parseStartDate(DateTime startDate, AgentDataModel agentInfo)
		{
			var feedback = new Feedback();
			if (startDate == DateTime.MinValue || startDate == DateTime.MaxValue)
			{
				if (_defaultValues != null && _defaultValues.StartDate.HasValue)
				{
					agentInfo.StartDate = _defaultValues.StartDate.Value;
					feedback.WarningMessages.Add(warningMessage(nameof(RawAgent.StartDate)));
					return feedback;
				}
				feedback.ErrorMessages.Add(string.Format(Resources.InvalidColumn, nameof(RawAgent.StartDate), ""));
			}
			else
			{
				agentInfo.StartDate = new DateOnly(startDate);
			}
			return feedback;
		}

		private Feedback parseFirstnameAndLastname(string rawFirstname, string rawLastname,
			AgentDataModel agentInfo)
		{
			var feedback = new Feedback();
			var errorMessages = feedback.ErrorMessages;

			if (rawFirstname.IsNullOrWhiteSpace() && rawLastname.IsNullOrWhiteSpace())
			{
				errorMessages.Add(Resources.BothFirstnameAndLastnameAreEmptyErrorMsgSemicolon);
			}

			if (rawFirstname != null && rawFirstname.Length > maxNameLength)
			{
				errorMessages.Add(Resources.TooLongFirstnameErrorMsgSemicolon);
			}
			if (rawLastname != null && rawLastname.Length > maxNameLength)
			{
				errorMessages.Add(Resources.TooLongLastnameErrorMsgSemicolon);
			}

			if (errorMessages.Any())
			{
				return feedback;
			}

			agentInfo.Firstname = rawFirstname;
			agentInfo.Lastname = rawLastname;

			return feedback;
		}

		private Feedback parseWindowsUserAndApplicationLogonId(string rawWindowsUser, string rawApplicationUserId,
			AgentDataModel agentInfo)
		{
			var feedback = new Feedback();
			var errorMessages = feedback.ErrorMessages;

			if (rawWindowsUser.IsNullOrWhiteSpace() && rawApplicationUserId.IsNullOrWhiteSpace())
			{
				errorMessages.Add(Resources.NoLogonAccountErrorMsgSemicolon);
			}

			if (!rawWindowsUser.IsNullOrWhiteSpace() && rawWindowsUser.Length > maxWindowUserLength)
			{
				errorMessages.Add(Resources.TooLongWindowsUserErrorMsgSemicolon);
			}

			if (!rawApplicationUserId.IsNullOrWhiteSpace() && rawApplicationUserId.Length > maxApplicationUserIdLength)
			{
				errorMessages.Add(Resources.TooLongApplicationUserIdErrorMsgSemicolon);
			}

			if (errorMessages.Any())
			{
				return feedback;
			}

			agentInfo.WindowsUser = rawWindowsUser;
			agentInfo.ApplicationUserId = rawApplicationUserId;
			return feedback;
		}

		private Feedback parsePassword(string rawPassword, AgentDataModel agentInfo)
		{
			var feedback = new Feedback();

			if (rawPassword.IsNullOrWhiteSpace())
			{
				feedback.ErrorMessages.Add(Resources.EmptyPasswordErrorMsgSemicolon);
				return feedback;
			}
			agentInfo.Password = rawPassword;
			return feedback;
		}

		private Feedback parseSchedulePeriodLength(int periodLength, AgentDataModel agentInfo)
		{
			var feedback = new Feedback();
			if (periodLength == 0)
			{
				if (_defaultValues?.SchedulePeriodLength != null)
				{
					agentInfo.SchedulePeriodLength = _defaultValues.SchedulePeriodLength.Value;
					feedback.WarningMessages.Add(warningMessage(nameof(RawAgent.SchedulePeriodLength)));
					return feedback;
				}
				feedback.ErrorMessages.Add(string.Format(Resources.InvalidColumn, nameof(RawAgent.SchedulePeriodLength), ""));
			}
			else
			{
				agentInfo.SchedulePeriodLength = periodLength;
			}

			return feedback;
		}

		private Feedback parseSchedulePeriodType(string rawSchedulePeriodType, AgentDataModel agentInfo)
		{
			var feedback = new Feedback();
			var schedulePeriodTypeString = rawSchedulePeriodType;
			SchedulePeriodType schedulePeriodType;
			if (Enum.TryParse(schedulePeriodTypeString, out schedulePeriodType))
			{
				agentInfo.SchedulePeriodType = schedulePeriodType;
				return feedback;
			}

			if (_defaultValues?.SchedulePeriodType != null)
			{
				agentInfo.SchedulePeriodType = _defaultValues.SchedulePeriodType.Value;
				feedback.WarningMessages.Add(warningMessage("SchedulePeriodType"));
				return feedback;
			}

			var avaliableSchedulePeriodTypeValues = String.Join(", ", EnumExtensions
															.GetValues(SchedulePeriodType.ChineseMonth)
															.Select(t => t.ToString()));
			feedback.ErrorMessages.Add(string.Format(Resources.InvalidColumn, "SchedulePeriodType", string.Format(Resources.OnlySupport, avaliableSchedulePeriodTypeValues)));
			return feedback;
		}

		private Feedback parseExternalLogon(string rawExternalLogons, AgentDataModel agentInfo)
		{
			var feedback = new Feedback();
			if (rawExternalLogons == null)
				return feedback;
			var externalLogons = StringHelper.SplitStringList(rawExternalLogons);
			agentInfo.ExternalLogons = new List<IExternalLogOn>();
			var invalidLogons = new List<string>(); 
			foreach (var logon in externalLogons)
			{
				var externalLogon = _importAgentDataProvider.FindExternalLogOn(logon);
				if (externalLogon == null)
				{
					invalidLogons.Add(logon);
				}
				else
				{
					agentInfo.ExternalLogons.Add(externalLogon);
					
				}
			}

			if (!agentInfo.ExternalLogons.Any())
			{
				if (_defaultValues != null)
				{
					if (_defaultValues.ExternalLogon != null)
						agentInfo.ExternalLogons.Add(_defaultValues.ExternalLogon);
					feedback.WarningMessages.Add(warningMessage("ExternalLogon"));
					return feedback;
				}

				feedback.ErrorMessages.Add(string.Format(Resources.InvalidColumn, "ExternalLogon", rawExternalLogons));
				return feedback;
			}

			if (invalidLogons.Any())
			{
				feedback.WarningMessages.Add(string.Format(Resources.InvalidColumn, nameof(RawAgent.ExternalLogon), string.Join(",", invalidLogons)));
			}
			return feedback;
		}

		private Feedback parseRuleSetBag(string rawShiftBag, AgentDataModel agentInfo)
		{
			var feedback = new Feedback();
			var ruleSetBag = _importAgentDataProvider.FindRuleSetBag(rawShiftBag);
			if (ruleSetBag == null)
			{
				if (_defaultValues?.RuleSetBag != null)
				{
					agentInfo.RuleSetBag = _defaultValues.RuleSetBag;
					feedback.WarningMessages.Add(warningMessage("ShiftBag"));
					return feedback;
				}

				feedback.ErrorMessages.Add(string.Format(Resources.InvalidColumn, "ShiftBags", rawShiftBag));
				return feedback;
				;
			}

			agentInfo.RuleSetBag = ruleSetBag;
			return feedback;
		}

		private Feedback parsePartTimePercentage(string rawPartTimePercentage, AgentDataModel agentInfo)
		{
			var feedback = new Feedback();
			var partTimePercentage = _importAgentDataProvider.FindPartTimePercentage(rawPartTimePercentage);
			if (partTimePercentage == null)
			{
				if (_defaultValues?.PartTimePercentage != null)
				{
					agentInfo.PartTimePercentage = _defaultValues.PartTimePercentage;
					feedback.WarningMessages.Add(warningMessage("PartTimePercentage"));
					return feedback;
				}

				feedback.ErrorMessages.Add(string.Format(Resources.InvalidColumn, "PartTimePercentage", rawPartTimePercentage));
				return feedback;
				;
			}

			agentInfo.PartTimePercentage = partTimePercentage;
			return feedback;
		}

		private Feedback parseContractSchedule(string rawContractSchedule, AgentDataModel agentInfo)
		{
			var feedback = new Feedback();
			var contractSchedule = _importAgentDataProvider.FindContractSchedule(rawContractSchedule);
			if (contractSchedule == null)
			{
				if (_defaultValues?.ContractSchedule != null)
				{
					agentInfo.ContractSchedule = _defaultValues.ContractSchedule;
					feedback.WarningMessages.Add(warningMessage("ContractSchedule"));
					return feedback;
				}

				feedback.ErrorMessages.Add(string.Format(Resources.InvalidColumn, "ContractSchedule", rawContractSchedule));
				return feedback;
			}

			agentInfo.ContractSchedule = contractSchedule;
			return feedback;
		}

		private Feedback parseContract(string rawContract, AgentDataModel agentInfo)
		{
			var feedback = new Feedback();
			var contract = _importAgentDataProvider.FindContract(rawContract);
			if (contract == null)
			{
				if (_defaultValues?.Contract != null)
				{
					agentInfo.Contract = _defaultValues.Contract;
					feedback.WarningMessages.Add(warningMessage("Contract"));
					return feedback;
				}

				feedback.ErrorMessages.Add(string.Format(Resources.InvalidColumn, "Contract", rawContract));
				return feedback;
			}

			agentInfo.Contract = contract;
			return feedback;
		}

		private Feedback parseRole(string rawRoleString, AgentDataModel agent)
		{
			var feedback = new Feedback();
			var roleNames = rawRoleString != null ?StringHelper.SplitStringList(rawRoleString).ToList() : new List<string>();
			agent.Roles = new List<IApplicationRole>();
			var invalidRoles = new List<string>();
			foreach (var roleName in roleNames)
			{
				var foundRole = _importAgentDataProvider.FindRole(roleName);
				if (foundRole == null)
				{
					invalidRoles.Add(roleName);
				}
				else
				{
					agent.Roles.Add(foundRole);
				}
			}

			if (!agent.Roles.Any())
			{
				if (_defaultValues != null && _defaultValues.Roles.Any())
				{
					agent.Roles = _defaultValues.Roles;
					feedback.WarningMessages.Add(warningMessage("Roles"));
					return feedback;
				}

				feedback.ErrorMessages.Add(string.Format(Resources.InvalidColumn, "Role", string.Join(",", invalidRoles)));
				return feedback;
			}

			if (invalidRoles.Any())
			{
				feedback.WarningMessages.Add(string.Format(Resources.InvalidColumn, "Role", string.Join(",", invalidRoles)));
			}

			return feedback;
		}

		private Feedback parseOrganization(string rawOrganizationString, AgentDataModel agent)
		{
			var feedback = new Feedback();
			var organizationParts = rawOrganizationString?.Split('/') ?? new string[] {};

			ITeam team = null;

			if (organizationParts.Length == 2)
			{
				var site = _importAgentDataProvider.FindSite(organizationParts[0]);
				if (site != null)
				{
					team = _importAgentDataProvider.FindTeam(site, organizationParts[1]);
				}
			}

			if (team == null)
			{
				if (_defaultValues?.Team != null)
				{
					agent.Team = _defaultValues.Team;
					feedback.WarningMessages.Add(warningMessage("Team"));
					return feedback;
				}

				feedback.ErrorMessages.Add(string.Format(Resources.InvalidColumn, "Organization", rawOrganizationString));
				return feedback;
			}

			agent.Team = team;
			return feedback;
		}

		private Feedback parseSkill(string rawSkillString, AgentDataModel agent)
		{
			var feedback = new Feedback();
			var skillNames = !rawSkillString.IsNullOrWhiteSpace()
				? StringHelper.SplitStringList(rawSkillString)
				: new List<string>();
			agent.Skills = new List<ISkill>();

			var invalidSkills = new List<string>();
			foreach (var skillName in skillNames)
			{
				var skill = _importAgentDataProvider.FindSkill(skillName);
				if (skill == null)
				{
					invalidSkills.Add(skillName);
				}
				else
				{
					agent.Skills.Add(skill);
				}
			}

			if (!agent.Skills.Any())
			{
				if (_defaultValues != null && _defaultValues.Skills.Any())
				{
					agent.Skills = _defaultValues.Skills;
					feedback.WarningMessages.Add(warningMessage("Skills"));
					return feedback;
				}

				feedback.ErrorMessages.Add(string.Format(Resources.InvalidColumn, "Skill", string.Join(",", invalidSkills)));
				return feedback;
			}

			if (invalidSkills.Any())
			{
				feedback.WarningMessages.Add(string.Format(Resources.InvalidColumn, "Skill", string.Join(",", invalidSkills)));
			}

			return feedback;
		}

		private static string warningMessage(string column)
		{
			return string.Format(Resources.ImportAgentsColumnFixedWithFallbackValue, column);
		}

		private class defaultAgentDataModel
		{
			public List<IApplicationRole> Roles { get; }
			public DateOnly? StartDate { get; set; }
			public ITeam Team { get; set; }
			public List<ISkill> Skills { get; }
			public IExternalLogOn ExternalLogon { get; set; }
			public IContract Contract { get; set; }
			public IContractSchedule ContractSchedule { get; set; }
			public IPartTimePercentage PartTimePercentage { get; set; }
			public IRuleSetBag RuleSetBag { get; set; }
			public SchedulePeriodType? SchedulePeriodType { get; set; }
			public int? SchedulePeriodLength { get; set; }

			public defaultAgentDataModel()
			{
				Roles = new List<IApplicationRole>();
				Skills = new List<ISkill>();
			}
		}
	}

}