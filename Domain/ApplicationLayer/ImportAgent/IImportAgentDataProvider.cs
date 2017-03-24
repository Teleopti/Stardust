using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ImportAgent
{
	public interface IImportAgentDataProvider
	{
		ImportAgentsFieldOptionsModel FieldOptions();
		IApplicationRole FindRole(string roleName);
		IContract FindContract(string contractName);
		IContractSchedule FindContractSchedule(string contractScheduleName);
		IPartTimePercentage FindPartTimePercentage(string partTimePercentageName);
		IRuleSetBag FindRuleSetBag(string ruleSetBagName);
		ISkill FindSkill(string skillName);
		ISite FindSite(string siteName);
		ITeam FindTeam(ISite site, string teamName);
		IExternalLogOn FindExternalLogOn(string externalLogonName);
		IExternalLogOn FindExternalLogOn(Guid id);
		IApplicationRole FindRole(Guid id);
		IContract FindContract(Guid id);
		IContractSchedule FindContractSchedule(Guid id);
		IPartTimePercentage FindPartTimePercentage(Guid id);
		IRuleSetBag FindRuleSetBag(Guid id);
		ITeam FindTeam(Guid id);
		ISkill FindSkill(Guid id);
	}
}