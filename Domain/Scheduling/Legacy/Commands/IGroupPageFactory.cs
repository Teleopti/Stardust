using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public interface IGroupPageFactory
	{
		IGroupPageCreator<IBusinessUnit> GetPersonsGroupPageCreator();
		IGroupPageCreator<IContract> GetContractsGroupPageCreator();
		IGroupPageCreator<IContractSchedule> GetContractSchedulesGroupPageCreator();
		IGroupPageCreator<IPartTimePercentage> GetPartTimePercentagesGroupPageCreator();
		IGroupPageCreator<IPerson> GetNotesGroupPageCreator();
		IGroupPageCreator<IRuleSetBag> GetRuleSetBagsGroupPageCreator();
		IGroupPageCreator<IPerson> GetSingleAgentTeamCreator();
	}
}