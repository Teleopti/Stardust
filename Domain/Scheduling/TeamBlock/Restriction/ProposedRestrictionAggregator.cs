﻿using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.Restriction
{
	public interface IProposedRestrictionAggregator
	{
		IEffectiveRestriction Aggregate(ISchedulingOptions schedulingOptions, ITeamBlockInfo teamBlockInfo,
														DateOnly dateOnly, IPerson person, IShiftProjectionCache roleModel);
	}

	public class ProposedRestrictionAggregator : IProposedRestrictionAggregator
	{
		private readonly ITeamRestrictionAggregator _teamRestrictionAggregator;
		private readonly IBlockRestrictionAggregator _blockRestrictionAggregator;
		private readonly ITeamBlockRestrictionAggregator _teamBlockRestrictionAggregator;
		private readonly ITeamBlockSchedulingOptions _teamBlockSchedulingOptions;
		private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;
		private readonly Func<ISchedulingResultStateHolder> _schedulingResultStateHolder;

		public ProposedRestrictionAggregator(ITeamRestrictionAggregator teamRestrictionAggregator,
			IBlockRestrictionAggregator blockRestrictionAggregator,
			ITeamBlockRestrictionAggregator teamBlockRestrictionAggregator,
			ITeamBlockSchedulingOptions teamBlockSchedulingOptions,
			IEffectiveRestrictionCreator effectiveRestrictionCreator,
			Func<ISchedulingResultStateHolder> schedulingResultStateHolder)
		{
			_teamRestrictionAggregator = teamRestrictionAggregator;
			_blockRestrictionAggregator = blockRestrictionAggregator;
			_teamBlockRestrictionAggregator = teamBlockRestrictionAggregator;
			_teamBlockSchedulingOptions = teamBlockSchedulingOptions;
			_effectiveRestrictionCreator = effectiveRestrictionCreator;
			_schedulingResultStateHolder = schedulingResultStateHolder;
		}

		public IEffectiveRestriction Aggregate(ISchedulingOptions schedulingOptions, ITeamBlockInfo teamBlockInfo,
											   DateOnly dateOnly, IPerson person, IShiftProjectionCache roleModel)
		{
			if (_teamBlockSchedulingOptions.IsTeamScheduling(schedulingOptions))
				return _teamRestrictionAggregator.Aggregate(dateOnly, teamBlockInfo, schedulingOptions, roleModel);

			if (_teamBlockSchedulingOptions.IsBlockScheduling(schedulingOptions))
				return _blockRestrictionAggregator.Aggregate(person, teamBlockInfo, schedulingOptions, roleModel);

			if (_teamBlockSchedulingOptions.IsTeamBlockScheduling(schedulingOptions))
				return _teamBlockRestrictionAggregator.Aggregate(dateOnly, person, teamBlockInfo, schedulingOptions, roleModel);

			return _effectiveRestrictionCreator.GetEffectiveRestrictionForSinglePerson(person, dateOnly, schedulingOptions,
				_schedulingResultStateHolder().Schedules);
		}
	}
}
