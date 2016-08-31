using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	//REMOVE THIS FAKE!!! Should not be used in DomainTest
	public class FakeGroupScheduleGroupPageDataProvider : IGroupScheduleGroupPageDataProvider
	{
		private readonly Func<ISchedulerStateHolder> _stateHolder;
		private readonly IList<IRuleSetBag> _ruleSetBags;

		public FakeGroupScheduleGroupPageDataProvider(Func<ISchedulerStateHolder> stateHolder)
		{
			_stateHolder = stateHolder;
			_ruleSetBags = new List<IRuleSetBag>();
		}

		public IEnumerable<IPerson> PersonCollection { get { return _stateHolder().AllPermittedPersons; } }
		public IEnumerable<IContract> ContractCollection { get { return Enumerable.Empty<IContract>(); } }
		public IEnumerable<IContractSchedule> ContractScheduleCollection { get { return Enumerable.Empty<IContractSchedule>(); } }
		public IEnumerable<IPartTimePercentage> PartTimePercentageCollection { get { return Enumerable.Empty<IPartTimePercentage>(); } }
		public IEnumerable<IRuleSetBag> RuleSetBagCollection { get { return _ruleSetBags; } }
		public IEnumerable<IGroupPage> UserDefinedGroupings { get { return Enumerable.Empty<IGroupPage>(); } }
		public IEnumerable<IBusinessUnit> BusinessUnitCollection { get { return new [] {BusinessUnitFactory.BusinessUnitUsedInTest}; } }
		public DateOnlyPeriod SelectedPeriod { get { return new DateOnlyPeriod(); } }
		public IList<ISkill> SkillCollection { get { return new List<ISkill>(); } }
		public IEnumerable<IPerson> AllLoadedPersons { get { return _stateHolder().AllPermittedPersons; } }

		public void AddRuleSetBag(IRuleSetBag ruleSetBag)
		{
			_ruleSetBags.Add(ruleSetBag);
		}
	}
}