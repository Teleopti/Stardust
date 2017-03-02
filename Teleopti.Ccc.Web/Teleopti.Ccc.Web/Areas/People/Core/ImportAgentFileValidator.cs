using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Ajax.Utilities;
using NPOI.SS.UserModel;
using Teleopti.Ccc.Domain.AgentInfo.ImportAgent;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Util;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.People.Core.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.People.Core
{
	public class ImportAgentFileValidator:IImportAgentFileValidator
	{
		private const int maxNameLength = 25;
		private const int maxWindowUserLength = 100;
		private const int maxApplicationUserIdLength = 50;

		private readonly IImportAgentDataProvider _importAgentDataProvider;

		public ImportAgentFileValidator(IImportAgentDataProvider importAgentDataProvider)
		{
			_importAgentDataProvider = importAgentDataProvider;
		}

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

		public List<string> ExtractColumnNames(IWorkbook workbook)
		{
			var sheet = workbook.GetSheetAt(0);
			var headerRow = sheet.GetRow(0);
			return headerRow.Cells.Select(x => x.StringCellValue).ToList();
		}

		public List<AgentExtractionResult> ExtractAgentInfoValues(IWorkbook workbook)
		{
			_importAgentDataProvider.Init();
			var sheet = workbook.GetSheetAt(0);

			var result = new List<AgentExtractionResult>();

			for(int i = 1;i <= sheet.LastRowNum;i++)
			{
				var row = sheet.GetRow(i);
				result.Add(extractAgentInfo(row));
			}

			return result;
		}

		public List<string> ValidateColumnNames(List<string> names)
		{
			var columnNames = names.ToArray();
			var errorMessages = new List<string>();
			for(var i = 0;i < expectedColumnNames.Length;i++)
			{
				var expectedColumnName = expectedColumnNames[i];
				if(i >= columnNames.Length)
				{
					errorMessages.Add(expectedColumnName);
					continue;
				}
				var columnName = columnNames[i];
				if(string.Compare(columnName,expectedColumnName,true,CultureInfo.CurrentCulture) != 0)
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

			if(cells.Length < expectedColumnNames.Length)
			{
				result.ErrorMessages.Add(Resources.MissingColumn);
				return result;
			}

			var raw = new RawAgent();
			var agentInfo = new AgentDataModel();

			setRawData(raw,cells,result);

			result.Raw = raw;

			agentInfo.Firstname = raw.Firstname;
			agentInfo.Lastname = raw.Lastname;

			result.ErrorMessages.AddRange(parseFirstnameAndLastname(raw.Firstname, raw.Lastname, agentInfo));
			result.ErrorMessages.AddRange(parseWindowsUserAndApplicationLogonId(raw.WindowsUser, raw.ApplicationUserId, agentInfo));	
			result.ErrorMessages.AddRange(parsePassword(raw.Password,agentInfo));			
			result.ErrorMessages.AddRange(parseRole(raw.Role,agentInfo));

			if(raw.StartDate.HasValue)
			{
				agentInfo.StartDate = new DateOnly(raw.StartDate.Value);
			}

			result.ErrorMessages.AddRange(parseOrganization(raw.Organization,agentInfo));
			result.ErrorMessages.AddRange(parseSkill(raw.Skill,agentInfo));
			result.ErrorMessages.AddRange(parseExternalLogon(raw.ExternalLogon,agentInfo));
			result.ErrorMessages.AddRange(parseContract(raw.Contract,agentInfo));
			result.ErrorMessages.AddRange(parseContractSchedule(raw.ContractSchedule,agentInfo));
			result.ErrorMessages.AddRange(parsePartTimePercentage(raw.PartTimePercentage,agentInfo));
			result.ErrorMessages.AddRange(parseRuleSetBag(raw.ShiftBag,agentInfo));
			result.ErrorMessages.AddRange(parseSchedulePeriodType(raw.SchedulePeriodType,agentInfo));

			if(raw.SchedulePeriodLength.HasValue)
			{
				agentInfo.SchedulePeriodLength = (int)raw.SchedulePeriodLength.Value;
			}

			if(!result.ErrorMessages.Any())
			{
				result.Agent = agentInfo;
			}
			return result;
		}

		private IEnumerable<string> parseFirstnameAndLastname(string rawFirstname, string rawLastname, AgentDataModel agentInfo)
		{
			var errorMessages = new List<string>();

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
				return errorMessages;
			}

			agentInfo.Firstname = rawFirstname;
			agentInfo.Lastname = rawLastname;

			return errorMessages;
		}

		private IEnumerable<string> parseWindowsUserAndApplicationLogonId(string rawWindowsUser, string rawApplicationUserId, AgentDataModel agentInfo)
		{
			var errorMessages = new List<string>();

			if (rawWindowsUser.IsNullOrWhiteSpace() && rawApplicationUserId.IsNullOrWhiteSpace())
			{
				errorMessages.Add(Resources.NoLogonAccountErrorMsgSemicolon);
			}

			if(rawWindowsUser.Length > maxWindowUserLength)
			{
				errorMessages.Add(Resources.TooLongWindowsUserErrorMsgSemicolon);
			}

			if (rawApplicationUserId.Length > maxApplicationUserIdLength)
			{
				errorMessages.Add(Resources.TooLongApplicationUserIdErrorMsgSemicolon);
			}

			if (errorMessages.Any())
			{
				return errorMessages;
			}

			agentInfo.WindowsUser = rawWindowsUser;
			agentInfo.ApplicationUserId = rawApplicationUserId;
			return new List<string>();
		}

		private static void setRawData(RawAgent raw,ICell[] cells,AgentExtractionResult result)
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
			catch(Exception)
			{
				raw.StartDate = null;
				result.ErrorMessages.Add(string.Format(Resources.InvalidColumn, "StartDate",cells[6].StringCellValue));
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
			catch(Exception)
			{
				raw.SchedulePeriodLength = null;
				result.ErrorMessages.Add(string.Format(Resources.InvalidColumn, "SchedulePeriodLength", cells[15].StringCellValue));
			}
		}

		private IEnumerable<string> parsePassword(string rawPassword,AgentDataModel agentInfo)
		{
			if (rawPassword.IsNullOrWhiteSpace())
			{
				return new List<string> { Resources.EmptyPasswordErrorMsgSemicolon };
			}
			agentInfo.Password = rawPassword;
			return new List<string>();
		}

		

		private IEnumerable<string> parseSchedulePeriodType(string rawSchedulePeriodType,AgentDataModel agentInfo)
		{
			var schedulePeriodTypeString = rawSchedulePeriodType;
			SchedulePeriodType schedulePeriodType;
			if(Enum.TryParse(schedulePeriodTypeString,out schedulePeriodType))
			{
				agentInfo.SchedulePeriodType = schedulePeriodType;
				return new List<string>();
			}

			return new List<string> {string.Format(Resources.InvalidColumn, "SchedulePeriodType", rawSchedulePeriodType)};
		}

		private IEnumerable<string> parseExternalLogon(string rawExternalLogon,AgentDataModel agentInfo)
		{
			var externalLogon = _importAgentDataProvider.FindExternalLogOn(rawExternalLogon);

			if (externalLogon == null)
			{
				return new List<string> { string.Format(Resources.InvalidColumn, "ExternalLogOn",rawExternalLogon) };
			}

			agentInfo.ExternalLogon = externalLogon;
			return new List<string>();
		}

		private IEnumerable<string> parseRuleSetBag(string rawShiftBag,AgentDataModel agentInfo)
		{
			var ruleSetBag = _importAgentDataProvider.FindRuleSetBag(rawShiftBag);
			if(ruleSetBag == null)
			{
				return new List<string> { string.Format(Resources.InvalidColumn, "ShiftBags",rawShiftBag) };				
			}
			
			agentInfo.RuleSetBag = ruleSetBag;
			return new List<string>();
		}

		private IEnumerable<string> parsePartTimePercentage(string rawPartTimePercentage,AgentDataModel agentInfo)
		{
			var partTimePercentage = _importAgentDataProvider.FindPartTimePercentage(rawPartTimePercentage);
			if(partTimePercentage == null)
			{
				return new List<string> { string.Format(Resources.InvalidColumn, "PartTimePercentage",rawPartTimePercentage) };
			}
			
			agentInfo.PartTimePercentage = partTimePercentage;
			return new List<string>();
		}

		private IEnumerable<string> parseContractSchedule(string rawContractSchedule,AgentDataModel agentInfo)
		{
			var contractSchedule = _importAgentDataProvider.FindContractSchedule(rawContractSchedule);
			if(contractSchedule == null)
			{
				return new List<string> { string.Format(Resources.InvalidColumn, "ContractSchedule",rawContractSchedule) };
			}
			
			agentInfo.ContractSchedule = contractSchedule;
			return new List<string>();
		}

		private IEnumerable<string> parseContract(string rawContract,AgentDataModel agentInfo)
		{
			var contract = _importAgentDataProvider.FindContract(rawContract);
			if(contract == null)
			{
				return new List<string> { string.Format(Resources.InvalidColumn, "Contract",rawContract) };			
			}
			
			agentInfo.Contract = contract;
			return new List<string>();
		}

		private IEnumerable<string> parseRole(string rawRoleString, AgentDataModel agent)
		{

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

			return invalidRoles.Any()
				? new List<string> {string.Format(Resources.InvalidColumn,  "Role", string.Join(",", invalidRoles))}
				: new List<string>();
		}

		private IEnumerable<string> parseOrganization(string rawOrganizationString,AgentDataModel agent)
		{
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
			
			if(team == null)
			{
				return new List<string> {string.Format(Resources.InvalidColumn,  "Organization", rawOrganizationString)};
			}

			agent.Team = team;
			return new List<string>();
		}

		private IEnumerable<string> parseSkill(string rawSkillString,AgentDataModel agent)
		{
			var skillNames = StringHelper.SplitStringList(rawSkillString);
			agent.Skills = new List<ISkill>();

			var invalidSkills = new List<string>();
			foreach(var skillName in skillNames)
			{
				var skill = _importAgentDataProvider.FindSkill(skillName);
				if(skill == null)
				{
					invalidSkills.Add(skillName);
				}
				else
				{
					agent.Skills.Add(skill);
				}
			}

			return invalidSkills.Any()
				? new List<string> { string.Format(Resources.InvalidColumn, "Skill",string.Join(",",invalidSkills)) }
				: new List<string>();
		}
	}
}