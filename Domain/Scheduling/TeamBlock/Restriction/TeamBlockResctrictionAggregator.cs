using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.Restriction
{
	public interface ITeamBlockRestrictionAggregator
	{
		IEffectiveRestriction Aggregate(IScheduleDictionary scheduleDictionary, DateOnly datePointer, IPerson person, ITeamBlockInfo teamBlockInfo, SchedulingOptions schedulingOptions, ShiftProjectionCache roleModel = null);
	}

	public class TeamBlockRestrictionAggregator : ITeamBlockRestrictionAggregator
	{
		private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;
		private readonly IScheduleDayEquator _scheduleDayEquator;
		private readonly IAssignmentPeriodRule _nightlyRestRule;
		private readonly ITeamBlockSchedulingOptions _teamBlockSchedulingOptions;

		public TeamBlockRestrictionAggregator(
			IEffectiveRestrictionCreator effectiveRestrictionCreator,
			IScheduleDayEquator scheduleDayEquator,
			IAssignmentPeriodRule nightlyRestRule,
			ITeamBlockSchedulingOptions teamBlockSchedulingOptions)
		{
			_effectiveRestrictionCreator = effectiveRestrictionCreator;
			_scheduleDayEquator = scheduleDayEquator;
			_nightlyRestRule = nightlyRestRule;
			_teamBlockSchedulingOptions = teamBlockSchedulingOptions;
		}
		
		public IEffectiveRestriction Aggregate(IScheduleDictionary scheduleDictionary, DateOnly datePointer, IPerson person, ITeamBlockInfo teamBlockInfo, SchedulingOptions schedulingOptions,
		                                       ShiftProjectionCache roleModel = null)
		{
			var dateOnlyList = teamBlockInfo?.BlockInfo.BlockPeriod.DayCollection();
			if (dateOnlyList == null) return null;

			var groupMembers = teamBlockInfo.TeamInfo.GroupMembers.ToList();
			var matrixList = teamBlockInfo.TeamInfo.MatrixesForGroup().ToList();
			var timeZone = groupMembers[0].PermissionInformation.DefaultTimeZone();
			var matrixesForPerson = teamBlockInfo.TeamInfo.MatrixesForMemberAndPeriod(person, teamBlockInfo.BlockInfo.BlockPeriod).ToList();

			IEffectiveRestriction effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(),
			                                                                      new EndTimeLimitation(),
			                                                                      new WorkTimeLimitation(), null, null, null,
			                                                                      new List<IActivityRestriction>());

			effectiveRestriction = combineRestriction(new TeamBlockEffectiveRestriction(_effectiveRestrictionCreator, person, schedulingOptions,
													  scheduleDictionary), dateOnlyList, matrixList, effectiveRestriction);

			effectiveRestriction = combineRestriction(new TeamBlockEffectiveRestriction(_effectiveRestrictionCreator, groupMembers, schedulingOptions,
													  scheduleDictionary), datePointer, matrixList, effectiveRestriction);
			

			if (_teamBlockSchedulingOptions.IsBlockSchedulingWithSameStartTime(schedulingOptions) ||
				_teamBlockSchedulingOptions.IsBlockSameStartTimeInTeamBlock(schedulingOptions))
			{
				effectiveRestriction = combineRestriction(new SameStartTimeRestriction(timeZone), dateOnlyList, matrixesForPerson, effectiveRestriction);
			}
			if (_teamBlockSchedulingOptions.IsTeamSchedulingWithSameStartTime(schedulingOptions) ||
				_teamBlockSchedulingOptions.IsTeamSameStartTimeInTeamBlock(schedulingOptions))
			{
				effectiveRestriction = combineRestriction(new SameStartTimeRestriction(timeZone), datePointer, matrixList, effectiveRestriction);
			}
			if (_teamBlockSchedulingOptions.IsBlockSchedulingWithSameShift(schedulingOptions) ||
				_teamBlockSchedulingOptions.IsBlockSameShiftInTeamBlock(schedulingOptions))
			{
				effectiveRestriction = combineRestriction(new SameShiftRestriction(_scheduleDayEquator), dateOnlyList, matrixesForPerson, effectiveRestriction);
			}
			if (_teamBlockSchedulingOptions.IsBlockSchedulingWithSameShiftCategory(schedulingOptions) ||
				_teamBlockSchedulingOptions.IsBlockSameShiftCategoryInTeamBlock(schedulingOptions))
			{
				effectiveRestriction = combineRestriction(new SameShiftCategoryRestriction(), dateOnlyList, matrixesForPerson, effectiveRestriction);
			}
			if (_teamBlockSchedulingOptions.IsTeamSchedulingWithSameShiftCategory(schedulingOptions) ||
				_teamBlockSchedulingOptions.IsTeamSameShiftCategoryInTeamBlock(schedulingOptions))
			{
				effectiveRestriction = combineRestriction(new SameShiftCategoryRestriction(), datePointer, matrixList, effectiveRestriction);
			}
			if (_teamBlockSchedulingOptions.IsTeamSchedulingWithSameEndTime(schedulingOptions) ||
				_teamBlockSchedulingOptions.IsTeamSameEndTimeInTeamBlock(schedulingOptions))
			{
				effectiveRestriction = combineRestriction(new SameEndTimeRestriction(timeZone), datePointer, matrixList, effectiveRestriction);
			}
			if (_teamBlockSchedulingOptions.IsTeamSchedulingWithSameActivity(schedulingOptions) ||
			    _teamBlockSchedulingOptions.IsTeamSameActivityInTeamBlock(schedulingOptions))
			{
				effectiveRestriction = combineRestriction(new SameActivityRestriction(schedulingOptions.CommonActivity), datePointer,
				                                          matrixList,
				                                          effectiveRestriction);
			}
			if (roleModel != null)
			{
				effectiveRestriction = combineRestriction(new ResctrictionFromRoleModelRestriction(roleModel, _teamBlockSchedulingOptions, schedulingOptions), dateOnlyList,
						matrixList, effectiveRestriction);
				if(effectiveRestriction != null) effectiveRestriction.CommonMainShift = null;
				if (schedulingOptions.UseBlock && schedulingOptions.BlockSameShift)
				{
					effectiveRestriction = combineRestriction(new SameShiftRestriction(_scheduleDayEquator), dateOnlyList, matrixesForPerson, effectiveRestriction);
				}
			}

			effectiveRestriction = combineRestriction(new NightlyRestRestriction(_nightlyRestRule, schedulingOptions), dateOnlyList, matrixList,
			                                          effectiveRestriction);

			return effectiveRestriction;
		}

		private static IEffectiveRestriction combineRestriction(IScheduleRestrictionStrategy strategy,
																IList<DateOnly> dateOnlyList,
																IList<IScheduleMatrixPro> matrixList,
																IEffectiveRestriction effectiveRestriction)
		{
			if (effectiveRestriction == null) return null;
			var restriction = strategy.ExtractRestriction(dateOnlyList, matrixList);
			return effectiveRestriction.Combine(restriction);
		}
	
		private static IEffectiveRestriction combineRestriction(IScheduleRestrictionStrategy strategy,
																DateOnly dateOnly,
																IList<IScheduleMatrixPro> matrixList,
																IEffectiveRestriction effectiveRestriction)
		{
			return combineRestriction(strategy, new List<DateOnly> {dateOnly}, matrixList, effectiveRestriction);
		}
	}
}
