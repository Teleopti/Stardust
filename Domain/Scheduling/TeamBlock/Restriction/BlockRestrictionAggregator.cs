using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.Restriction
{
	public interface IBlockRestrictionAggregator
	{
		IEffectiveRestriction Aggregate(IScheduleDictionary schedules, IPerson person, ITeamBlockInfo teamBlockInfo, SchedulingOptions schedulingOptions, ShiftProjectionCache roleModel, DateOnly dateOnly);
	}
	public class BlockRestrictionAggregator : IBlockRestrictionAggregator
	{
		private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;
		private readonly IScheduleDayEquator _scheduleDayEquator;
		private readonly IAssignmentPeriodRule _nightlyRestRule;
		private readonly ITeamBlockSchedulingOptions _teamBlockSchedulingOptions;

		public BlockRestrictionAggregator(IEffectiveRestrictionCreator effectiveRestrictionCreator,
			IScheduleDayEquator scheduleDayEquator,
			IAssignmentPeriodRule nightlyRestRule,
			ITeamBlockSchedulingOptions teamBlockSchedulingOptions)
		{
			_effectiveRestrictionCreator = effectiveRestrictionCreator;
			_scheduleDayEquator = scheduleDayEquator;
			_nightlyRestRule = nightlyRestRule;
			_teamBlockSchedulingOptions = teamBlockSchedulingOptions;
		}

		public IEffectiveRestriction Aggregate(IScheduleDictionary schedules, IPerson person, ITeamBlockInfo teamBlockInfo, SchedulingOptions schedulingOptions, ShiftProjectionCache roleModel, DateOnly dateOnly)
		{
			var scheduleDictionary = schedules;
			var timeZone = person.PermissionInformation.DefaultTimeZone();
			var matrixes = teamBlockInfo.TeamInfo.MatrixesForMemberAndPeriod(person, teamBlockInfo.BlockInfo.BlockPeriod).ToList();

			IEffectiveRestriction effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(),
																			  new EndTimeLimitation(),
																			  new WorkTimeLimitation(), null, null, null,
																			  new List<IActivityRestriction>());

			effectiveRestriction = combineRestriction(new TeamBlockEffectiveRestriction(_effectiveRestrictionCreator, new List<IPerson>{person}, schedulingOptions,
																	 scheduleDictionary), dateOnly, matrixes, effectiveRestriction);
			
			if (_teamBlockSchedulingOptions.IsBlockSchedulingWithSameStartTime(schedulingOptions))
			{
				effectiveRestriction = combineRestriction(new SameStartTimeRestriction(timeZone), dateOnly,
													  matrixes, effectiveRestriction);
			}
			if (_teamBlockSchedulingOptions.IsBlockSchedulingWithSameShift(schedulingOptions))
			{
				effectiveRestriction = combineRestriction(new SameShiftRestriction(_scheduleDayEquator), dateOnly, matrixes,
													  effectiveRestriction);
			}
			if (_teamBlockSchedulingOptions.IsBlockSchedulingWithSameShiftCategory(schedulingOptions))
			{
					effectiveRestriction = combineRestriction(new SameShiftCategoryRestriction(), dateOnly, matrixes,
													  effectiveRestriction);
			}
		
			effectiveRestriction = combineRestriction(new NightlyRestRestriction(_nightlyRestRule, schedulingOptions), dateOnly, matrixes,
													  effectiveRestriction);

			if (roleModel != null)
			{
				effectiveRestriction = combineRestriction(new ResctrictionFromRoleModelRestriction(roleModel, _teamBlockSchedulingOptions, schedulingOptions), dateOnly,
						matrixes, effectiveRestriction);
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
