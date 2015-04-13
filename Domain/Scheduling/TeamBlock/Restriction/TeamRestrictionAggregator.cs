﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.Restriction
{
	public interface ITeamRestrictionAggregator
	{
		IEffectiveRestriction Aggregate(DateOnly dateOnly, ITeamBlockInfo teamBlockInfo, ISchedulingOptions schedulingOptions, IShiftProjectionCache roleModel);
	}

	public class TeamRestrictionAggregator : ITeamRestrictionAggregator
	{
		private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;
		private readonly Func<ISchedulingResultStateHolder> _schedulingResultStateHolder;
		private readonly ITeamBlockSchedulingOptions _teamBlockSchedulingOptions;

		public TeamRestrictionAggregator(IEffectiveRestrictionCreator effectiveRestrictionCreator,
			Func<ISchedulingResultStateHolder> schedulingResultStateHolder,
			ITeamBlockSchedulingOptions teamBlockSchedulingOptions)
		{
			_effectiveRestrictionCreator = effectiveRestrictionCreator;
			_schedulingResultStateHolder = schedulingResultStateHolder;
			_teamBlockSchedulingOptions = teamBlockSchedulingOptions;
		}

		public IEffectiveRestriction Aggregate(DateOnly dateOnly,  ITeamBlockInfo teamBlockInfo, ISchedulingOptions schedulingOptions, IShiftProjectionCache roleModel)
		{
			var scheduleDictionary = _schedulingResultStateHolder().Schedules;
			var groupMembers = teamBlockInfo.TeamInfo.GroupMembers.ToList();
			var matrixList = teamBlockInfo.TeamInfo.MatrixesForGroup().ToList();
			var timeZone = groupMembers[0].PermissionInformation.DefaultTimeZone();

			IEffectiveRestriction effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(),
																			  new EndTimeLimitation(),
																			  new WorkTimeLimitation(), null, null, null,
																			  new List<IActivityRestriction>());

			effectiveRestriction = combineRestriction(new TeamBlockEffectiveRestrcition(_effectiveRestrictionCreator, groupMembers, schedulingOptions,
																	 scheduleDictionary), dateOnly, matrixList, effectiveRestriction);
			
			if (_teamBlockSchedulingOptions.IsTeamSchedulingWithSameStartTime(schedulingOptions))
			{
				effectiveRestriction = combineRestriction(new SameStartTimeRestriction(timeZone), dateOnly, matrixList,
													  effectiveRestriction);
			}
			if (_teamBlockSchedulingOptions.IsTeamSchedulingWithSameEndTime(schedulingOptions))
			{
				effectiveRestriction = combineRestriction(new SameEndTimeRestriction(timeZone), dateOnly, matrixList,
														  effectiveRestriction);
			}
			if (_teamBlockSchedulingOptions.IsTeamSchedulingWithSameShiftCategory(schedulingOptions))
			{
				effectiveRestriction = combineRestriction(new SameShiftCategoryRestriction(), dateOnly, matrixList,
													  effectiveRestriction);
			}
			if (_teamBlockSchedulingOptions.IsTeamSchedulingWithSameActivity(schedulingOptions))
			{
				effectiveRestriction = combineRestriction(new SameActivityRestriction(schedulingOptions.CommonActivity), dateOnly, matrixList,
				                                          effectiveRestriction);
			}
			if (roleModel != null)
			{
				effectiveRestriction = combineRestriction(new ResctrictionFromRoleModelRestriction(roleModel, _teamBlockSchedulingOptions, schedulingOptions), dateOnly,
						matrixList, effectiveRestriction);
			}

			return effectiveRestriction;
		}

		private static IEffectiveRestriction combineRestriction(IScheduleRestrictionStrategy strategy,
		                                                        DateOnly dateOnly,
		                                                        IList<IScheduleMatrixPro> matrixList,
		                                                        IEffectiveRestriction effectiveRestriction)
		{
			if (effectiveRestriction == null) return null;
			var restriction = strategy.ExtractRestriction(new List<DateOnly> {dateOnly}, matrixList);
			return effectiveRestriction.Combine(restriction);
		}
	}

}
