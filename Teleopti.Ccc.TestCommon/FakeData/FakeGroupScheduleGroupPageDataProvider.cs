using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	//would be better to not fake this interface at all, but too messy currently...
	public class FakeGroupScheduleGroupPageDataProvider : IGroupScheduleGroupPageDataProvider
	{
		private readonly Func<ISchedulerStateHolder> _stateHolder;

		public FakeGroupScheduleGroupPageDataProvider(Func<ISchedulerStateHolder> stateHolder)
		{
			_stateHolder = stateHolder;
		}

		public void SetBusinessUnit(IBusinessUnit businessUnit)
		{
			BusinessUnitCollection = new[] {businessUnit};
		}

		public IEnumerable<IPerson> PersonCollection { get { return _stateHolder().AllPermittedPersons; } }
		public IEnumerable<IContract> ContractCollection { get { return Enumerable.Empty<IContract>(); } }
		public IEnumerable<IContractSchedule> ContractScheduleCollection { get { return Enumerable.Empty<IContractSchedule>(); } }
		public IEnumerable<IPartTimePercentage> PartTimePercentageCollection { get { return Enumerable.Empty<IPartTimePercentage>(); } }
		public IEnumerable<IRuleSetBag> RuleSetBagCollection { get { return Enumerable.Empty<IRuleSetBag>(); } }
		public IEnumerable<IGroupPage> UserDefinedGroupings { get { return Enumerable.Empty<IGroupPage>(); } }
		public IEnumerable<IBusinessUnit> BusinessUnitCollection { get; private set; }
		public DateOnlyPeriod SelectedPeriod { get { return new DateOnlyPeriod(); } }
		public IList<ISkill> SkillCollection { get { return new List<ISkill>(); } }
		public IEnumerable<IPerson> AllLoadedPersons { get { return _stateHolder().AllPermittedPersons; } }
	}
}