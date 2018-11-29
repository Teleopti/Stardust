using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.Restriction
{
	public class ProposedRestrictionAggregator
	{
		private readonly ITeamRestrictionAggregator _teamRestrictionAggregator;
		private readonly IBlockRestrictionAggregator _blockRestrictionAggregator;
		private readonly ITeamBlockRestrictionAggregator _teamBlockRestrictionAggregator;
		private readonly ITeamBlockSchedulingOptions _teamBlockSchedulingOptions;
		private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;

		public ProposedRestrictionAggregator(ITeamRestrictionAggregator teamRestrictionAggregator,
			IBlockRestrictionAggregator blockRestrictionAggregator,
			ITeamBlockRestrictionAggregator teamBlockRestrictionAggregator,
			ITeamBlockSchedulingOptions teamBlockSchedulingOptions,
			IEffectiveRestrictionCreator effectiveRestrictionCreator)
		{
			_teamRestrictionAggregator = teamRestrictionAggregator;
			_blockRestrictionAggregator = blockRestrictionAggregator;
			_teamBlockRestrictionAggregator = teamBlockRestrictionAggregator;
			_teamBlockSchedulingOptions = teamBlockSchedulingOptions;
			_effectiveRestrictionCreator = effectiveRestrictionCreator;
		}

		public IEffectiveRestriction Aggregate(IScheduleDictionary schedules, SchedulingOptions schedulingOptions, ITeamBlockInfo teamBlockInfo,
											   DateOnly dateOnly, IPerson person, ShiftProjectionCache roleModel)
		{
			if (_teamBlockSchedulingOptions.IsTeamScheduling(schedulingOptions))
				return _teamRestrictionAggregator.Aggregate(schedules, dateOnly, teamBlockInfo, schedulingOptions, roleModel);

			if (_teamBlockSchedulingOptions.IsBlockScheduling(schedulingOptions))
				return _blockRestrictionAggregator.Aggregate(schedules, person, teamBlockInfo, schedulingOptions, roleModel, dateOnly);

			if (_teamBlockSchedulingOptions.IsTeamBlockScheduling(schedulingOptions))
				return _teamBlockRestrictionAggregator.Aggregate(schedules, dateOnly, person, teamBlockInfo, schedulingOptions, roleModel);

			return _effectiveRestrictionCreator.GetEffectiveRestrictionForSinglePerson(person, dateOnly, schedulingOptions,schedules);
		}
	}
}
