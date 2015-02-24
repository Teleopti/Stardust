using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public class GroupPageFactory : IGroupPageFactory
	{
		public IGroupPageCreator<IBusinessUnit> GetPersonsGroupPageCreator()
		{
			return new PersonGroupPage();
		}

		public IGroupPageCreator<IContract> GetContractsGroupPageCreator()
		{
			return new ContractGroupPage();
		}

		public IGroupPageCreator<IContractSchedule> GetContractSchedulesGroupPageCreator()
		{
			return new ContractScheduleGroupPage();
		}

		public IGroupPageCreator<IPartTimePercentage> GetPartTimePercentagesGroupPageCreator()
		{
			return new PartTimePercentageGroupPage();
		}

		public IGroupPageCreator<IPerson> GetNotesGroupPageCreator()
		{
			return new PersonNoteGroupPage();
		}

		public IGroupPageCreator<IRuleSetBag> GetRuleSetBagsGroupPageCreator()
		{
			return new RuleSetBagGroupPage();
		}

		public IGroupPageCreator<IPerson> GetSingleAgentTeamCreator()
		{
			return new SingleAgentTeamGroupPage();
		}
	}
}