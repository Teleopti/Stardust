using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Ajax.Utilities;
using NPOI.SS.UserModel;
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

		private readonly string[] expectedColumnNames = new[]
		{
			"Firstname",
			"Lastname",
			"WindowsUser",
			"ApplicationUserId",
			"Password",
			"Role",
			"StartDate",
			"Organization",
			"Skill",
			"ExternalLogon",
			"Contract",
			"ContractSchedule",
			"PartTimePercentage",
			"ShiftBag",
			"SchedulePeriodType",
			"SchedulePeriodLength"
		};

		private defaultAgentDataModel _defaultValues = new defaultAgentDataModel();

		private readonly IImportAgentDataProvider _importAgentDataProvider;

		public ImportAgentFileValidator(IImportAgentDataProvider importAgentDataProvider)
		{
			_importAgentDataProvider = importAgentDataProvider;
		}

		public List<string> ExtractColumnNames(IWorkbook workbook)
		{
			if (workbook.NumberOfSheets == 0)
				return new List<string>();
			var sheet = workbook.GetSheetAt(0);
			var headerRow = sheet.GetRow(0);
			return headerRow.Cells.Select(x => x.StringCellValue).ToList();
		}

		public List<AgentExtractionResult> ExtractAgentInfoValues(IWorkbook workbook)
		{
			var sheet = workbook.GetSheetAt(0);

			var result = new List<AgentExtractionResult>();

			for (int i = 1; i <= sheet.LastRowNum; i++)
			{
				var row = sheet.GetRow(i);
				result.Add(extractAgentInfo(row));
			}

			return result;
		}

		public void SetDefaultValues(ImportAgentFormData defaultValues)
		{
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
						if(skill != null)
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

			if(!defaultValues.PartTimePercentageId.IsNullOrWhiteSpace())
			{
				Guid partTimePercentageId;
				if(Guid.TryParse(defaultValues.PartTimePercentageId,out partTimePercentageId))
				{
					var partTimePercentage = _importAgentDataProvider.FindPartTimePercentage(partTimePercentageId);
					if(partTimePercentage != null)
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

		public List<string> ValidateColumnNames(List<string> names)
		{
			var columnNames = names.ToArray();
			var errorMessages = new List<string>();
			for (var i = 0; i < expectedColumnNames.Length; i++)
			{
				var expectedColumnName = expectedColumnNames[i];
				if (i >= columnNames.Length)
				{
					errorMessages.Add(expectedColumnName);
					continue;
				}
				var columnName = columnNames[i];
				if (string.Compare(columnName, expectedColumnName, true, CultureInfo.CurrentCulture) != 0)
				{
					errorMessages.Add(expectedColumnName);
				}
			}

			return errorMessages;
		}

		private AgentExtractionResult extractAgentInfo(IRow row)
		{
			var cells = row.Cells.ToArray();
			var result = new AgentExtractionResult();

			if (cells.Length < expectedColumnNames.Length)
			{
				result.Feedback.ErrorMessages.Add(Resources.MissingColumn);
				return result;
			}

			var raw = new RawAgent();
			var agentInfo = new AgentDataModel();

			setRawData(raw, cells, result);

			result.Raw = raw;

			agentInfo.Firstname = raw.Firstname;
			agentInfo.Lastname = raw.Lastname;

			result.Feedback.Merge(parseFirstnameAndLastname(raw.Firstname, raw.Lastname, agentInfo));
			result.Feedback.Merge(parseWindowsUserAndApplicationLogonId(raw.WindowsUser, raw.ApplicationUserId, agentInfo));
			result.Feedback.Merge(parsePassword(raw.Password, agentInfo));
			result.Feedback.Merge(parseRole(raw.Role, agentInfo));

			if (raw.StartDate.HasValue)
			{
				agentInfo.StartDate = new DateOnly(raw.StartDate.Value);
			}

			result.Feedback.Merge(parseOrganization(raw.Organization, agentInfo));
			result.Feedback.Merge(parseSkill(raw.Skill, agentInfo));
			result.Feedback.Merge(parseExternalLogon(raw.ExternalLogon, agentInfo));
			result.Feedback.Merge(parseContract(raw.Contract, agentInfo));
			result.Feedback.Merge(parseContractSchedule(raw.ContractSchedule, agentInfo));
			result.Feedback.Merge(parsePartTimePercentage(raw.PartTimePercentage, agentInfo));
			result.Feedback.Merge(parseRuleSetBag(raw.ShiftBag, agentInfo));
			result.Feedback.Merge(parseSchedulePeriodType(raw.SchedulePeriodType, agentInfo));

			if (raw.SchedulePeriodLength.HasValue)
			{
				agentInfo.SchedulePeriodLength = (int) raw.SchedulePeriodLength.Value;
			}

			if (!result.Feedback.ErrorMessages.Any())
			{
				result.Agent = agentInfo;
			}
			return result;
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

			if (rawFirstname.Length > maxNameLength)
			{
				errorMessages.Add(Resources.TooLongFirstnameErrorMsgSemicolon);
			}
			if (rawLastname.Length > maxNameLength)
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

			if (rawWindowsUser.Length > maxWindowUserLength)
			{
				errorMessages.Add(Resources.TooLongWindowsUserErrorMsgSemicolon);
			}

			if (rawApplicationUserId.Length > maxApplicationUserIdLength)
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

		private static void setRawData(RawAgent raw, ICell[] cells, AgentExtractionResult result)
		{
			raw.Firstname = cells[0].StringCellValue;
			raw.Lastname = cells[1].StringCellValue;
			raw.WindowsUser = cells[2].StringCellValue;
			raw.ApplicationUserId = cells[3].StringCellValue;
			raw.Password = cells[4].StringCellValue;
			raw.Role = cells[5].StringCellValue;
			try
			{
				raw.StartDate = cells[6].DateCellValue;
			}
			catch (Exception)
			{
				raw.StartDate = null;
				result.Feedback.ErrorMessages.Add(string.Format(Resources.InvalidColumn, "StartDate", cells[6].StringCellValue));
			}

			raw.Organization = cells[7].StringCellValue;
			raw.Skill = cells[8].StringCellValue;
			raw.ExternalLogon = cells[9].StringCellValue;
			raw.Contract = cells[10].StringCellValue;
			raw.ContractSchedule = cells[11].StringCellValue;
			raw.PartTimePercentage = cells[12].StringCellValue;
			raw.ShiftBag = cells[13].StringCellValue;
			raw.SchedulePeriodType = cells[14].StringCellValue;
			try
			{
				raw.SchedulePeriodLength = cells[15].NumericCellValue;
			}
			catch (Exception)
			{
				raw.SchedulePeriodLength = null;
				result.Feedback.ErrorMessages.Add(string.Format(Resources.InvalidColumn, "SchedulePeriodLength", cells[15].StringCellValue));
			}
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

			if (_defaultValues.SchedulePeriodType.HasValue)
			{
				agentInfo.SchedulePeriodType = _defaultValues.SchedulePeriodType.Value;
				// [ToDo] Add message to resource
				feedback.WarningMessages.Add("Fixed by default");
				return feedback;
			}

			feedback.ErrorMessages.Add(string.Format(Resources.InvalidColumn,"SchedulePeriodType",rawSchedulePeriodType));
			return feedback;			
		}

		private Feedback parseExternalLogon(string rawExternalLogon, AgentDataModel agentInfo)
		{
			var feedback = new Feedback();
			var externalLogon = _importAgentDataProvider.FindExternalLogOn(rawExternalLogon);

			if (externalLogon == null)
			{
				if(_defaultValues.ExternalLogon != null)
				{
					agentInfo.ExternalLogon = _defaultValues.ExternalLogon;
					// [ToDo] Add message to resource
					feedback.WarningMessages.Add("Fixed by default");
					return feedback;
				}

				feedback.ErrorMessages.Add(string.Format(Resources.InvalidColumn,"ExternalLogOn",rawExternalLogon));
				return feedback;				
			}

			agentInfo.ExternalLogon = externalLogon;
			return feedback;
		}

		private Feedback parseRuleSetBag(string rawShiftBag, AgentDataModel agentInfo)
		{
			var feedback = new Feedback();
			var ruleSetBag = _importAgentDataProvider.FindRuleSetBag(rawShiftBag);
			if (ruleSetBag == null)
			{
				if(_defaultValues.RuleSetBag != null)
				{
					agentInfo.RuleSetBag = _defaultValues.RuleSetBag;
					// [ToDo] Add message to resource
					feedback.WarningMessages.Add("Fixed by default");
					return feedback;
				}

				feedback.ErrorMessages.Add(string.Format(Resources.InvalidColumn,"ShiftBags",rawShiftBag));
				return feedback;;
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
				if(_defaultValues.PartTimePercentage != null)
				{
					agentInfo.PartTimePercentage = _defaultValues.PartTimePercentage;
					// [ToDo] Add message to resource
					feedback.WarningMessages.Add("Fixed by default");
					return feedback;
				}

				feedback.ErrorMessages.Add(string.Format(Resources.InvalidColumn,"PartTimePercentage",rawPartTimePercentage));
				return feedback;;
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
				if(_defaultValues.ContractSchedule != null)
				{
					agentInfo.ContractSchedule = _defaultValues.ContractSchedule;
					// [ToDo] Add message to resource
					feedback.WarningMessages.Add("Fixed by default");
					return feedback;
				}

				feedback.ErrorMessages.Add(string.Format(Resources.InvalidColumn,"ContractSchedule",rawContractSchedule));
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
				if(_defaultValues.Contract != null)
				{
					agentInfo.Contract = _defaultValues.Contract;
					// [ToDo] Add message to resource
					feedback.WarningMessages.Add("Fixed by default");
					return feedback;
				}

				feedback.ErrorMessages.Add(string.Format(Resources.InvalidColumn,"Contract",rawContract));
				return feedback;
			}

			agentInfo.Contract = contract;
			return feedback;
		}

		private Feedback parseRole(string rawRoleString, AgentDataModel agent)
		{
			var feedback = new Feedback();
			var roleNames = StringHelper.SplitStringList(rawRoleString).ToList();
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
				if(_defaultValues.Roles.Any())
				{
					agent.Roles = _defaultValues.Roles;
					// [ToDo] Add message to resource
					feedback.WarningMessages.Add("Fixed by default");
					return feedback;
				}

				feedback.ErrorMessages.Add(string.Format(Resources.InvalidColumn,"Role",string.Join(",",invalidRoles)));
				return feedback;
			}

			if (invalidRoles.Any())
			{
				feedback.WarningMessages.Add(string.Format(Resources.InvalidColumn,"Role",string.Join(",",invalidRoles)));
			}

			return feedback;			
		}

		private Feedback parseOrganization(string rawOrganizationString, AgentDataModel agent)
		{
			var feedback = new Feedback();
			var organizationParts = rawOrganizationString.Split('/');

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
				if(_defaultValues.Team != null)
				{
					agent.Team = _defaultValues.Team;
					// [ToDo] Add message to resource
					feedback.WarningMessages.Add("Fixed by default");
					return feedback;
				}

				feedback.ErrorMessages.Add(string.Format(Resources.InvalidColumn,"Organization",rawOrganizationString));
				return feedback;
			}

			agent.Team = team;
			return feedback;
		}

		private Feedback parseSkill(string rawSkillString, AgentDataModel agent)
		{
			var feedback = new Feedback();
			var skillNames = StringHelper.SplitStringList(rawSkillString);
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
				if(_defaultValues.Skills.Any())
				{
					agent.Skills = _defaultValues.Skills;
					// [ToDo] Add message to resource
					feedback.WarningMessages.Add("Fixed by default");
					return feedback;
				}

				feedback.ErrorMessages.Add(string.Format(Resources.InvalidColumn,"Skill",string.Join(",",invalidSkills)));
				return feedback;
			}

			if (invalidSkills.Any())
			{
				feedback.WarningMessages.Add(string.Format(Resources.InvalidColumn,"Skill",string.Join(",",invalidSkills)));				
			}

			return feedback;		
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