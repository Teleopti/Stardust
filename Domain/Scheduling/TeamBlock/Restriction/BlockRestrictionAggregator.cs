using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.Restriction
{
	public interface IBlockRestrictionAggregator
	{
		IEffectiveRestriction Aggregate(IPerson person, ITeamBlockInfo teamBlockInfo, ISchedulingOptions schedulingOptions, IShiftProjectionCache roleModel);
	}
	public class BlockRestrictionAggregator : IBlockRestrictionAggregator
	{
		private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;
		private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
		private readonly IScheduleDayEquator _scheduleDayEquator;
		private readonly IAssignmentPeriodRule _nightlyRestRule;
		private readonly ITeamBlockSchedulingOptions _teamBlockSchedulingOptions;

		public BlockRestrictionAggregator(IEffectiveRestrictionCreator effectiveRestrictionCreator,
			ISchedulingResultStateHolder schedulingResultStateHolder,
			IScheduleDayEquator scheduleDayEquator,
			IAssignmentPeriodRule nightlyRestRule,
			ITeamBlockSchedulingOptions teamBlockSchedulingOptions)
		{
			_effectiveRestrictionCreator = effectiveRestrictionCreator;
			_schedulingResultStateHolder = schedulingResultStateHolder;
			_scheduleDayEquator = scheduleDayEquator;
			_nightlyRestRule = nightlyRestRule;
			_teamBlockSchedulingOptions = teamBlockSchedulingOptions;
		}

		public IEffectiveRestriction Aggregate(IPerson person, ITeamBlockInfo teamBlockInfo, ISchedulingOptions schedulingOptions, IShiftProjectionCache roleModel)
		{
			var scheduleDictionary = _schedulingResultStateHolder.Schedules;
            var timeZone = TimeZoneInfo.Utc;
			var matrixes = teamBlockInfo.TeamInfo.MatrixesForMemberAndPeriod(person, teamBlockInfo.BlockInfo.BlockPeriod).ToList();
			var dateOnlyList = teamBlockInfo.BlockInfo.BlockPeriod.DayCollection();
			if (dateOnlyList == null) return null;

			IEffectiveRestriction effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(),
																			  new EndTimeLimitation(),
																			  new WorkTimeLimitation(), null, null, null,
																			  new List<IActivityRestriction>());

			effectiveRestriction = combineRestriction(new TeamBlockEffectiveRestrcition(_effectiveRestrictionCreator, new List<IPerson>{person}, schedulingOptions,
																	 scheduleDictionary), dateOnlyList, matrixes, effectiveRestriction);

			if (_teamBlockSchedulingOptions.IsBlockSchedulingWithSameStartTime(schedulingOptions))
			{
				effectiveRestriction = combineRestriction(new SameStartTimeRestriction(timeZone), dateOnlyList,
													  matrixes, effectiveRestriction);
			}
			if (_teamBlockSchedulingOptions.IsBlockSchedulingWithSameShift(schedulingOptions))
			{
				effectiveRestriction = combineRestriction(new SameShiftRestriction(_scheduleDayEquator), dateOnlyList, matrixes,
													  effectiveRestriction);
			}
			if (_teamBlockSchedulingOptions.IsBlockSchedulingWithSameShiftCategory(schedulingOptions))
			{
					effectiveRestriction = combineRestriction(new SameShiftCategoryRestriction(), dateOnlyList, matrixes,
													  effectiveRestriction);
			}
		
			effectiveRestriction = combineRestriction(new NightlyRestRestrcition(_nightlyRestRule), dateOnlyList, matrixes,
													  effectiveRestriction);
			if (roleModel != null)
			{
				effectiveRestriction = combineRestriction(new ResctrictionFromRoleModelRestriction(roleModel, _teamBlockSchedulingOptions, schedulingOptions), dateOnlyList,
						matrixes, effectiveRestriction);
			}
			return effectiveRestriction;
		}

		private static IEffectiveRestriction combineRestriction(IScheduleRestrictionStrategy strategy,
																IList<DateOnly> dateOnlyList,
																IList<IScheduleMatrixPro> matrixList,
																IEffectiveRestriction effectiveRestriction)
		{
			var restriction = strategy.ExtractRestriction(dateOnlyList, matrixList);
			return effectiveRestriction.Combine(restriction);
		}
	}
}
