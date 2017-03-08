using Teleopti.Ccc.Domain.AgentInfo.ImportAgent;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Web.Areas.People.Core.Providers
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
	}
}