using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeGroupScheduleGroupPageDataProvider : IGroupScheduleGroupPageDataProvider
	{
		public IEnumerable<IPerson> PersonCollection { get; private set; }
		public IEnumerable<IContract> ContractCollection { get; private set; }
		public IEnumerable<IContractSchedule> ContractScheduleCollection { get; private set; }
		public IEnumerable<IPartTimePercentage> PartTimePercentageCollection { get; private set; }
		public IEnumerable<IRuleSetBag> RuleSetBagCollection { get; private set; }
		public IEnumerable<IGroupPage> UserDefinedGroupings { get; private set; }
		public IEnumerable<IBusinessUnit> BusinessUnitCollection { get; private set; }
		public DateOnlyPeriod SelectedPeriod { get; private set; }
		public IList<ISkill> SkillCollection { get; private set; }
		public IEnumerable<IPerson> AllLoadedPersons { get; private set; }
	}
}