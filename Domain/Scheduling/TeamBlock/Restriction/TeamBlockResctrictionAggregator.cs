using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.Restriction
{
	public interface ITeamBlockRestrictionAggregator
	{
		IEffectiveRestriction Aggregate(ITeamBlockInfo teamBlockInfo, ISchedulingOptions schedulingOptions, IShiftProjectionCache roleModel);
		IEffectiveRestriction Aggregate(ITeamBlockInfo teamBlockInfo, ISchedulingOptions schedulingOptions);
	}

	public class TeamBlockRestrictionAggregator : ITeamBlockRestrictionAggregator
	{
		private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;
		private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
		private readonly IScheduleDayEquator _scheduleDayEquator;
		private readonly IAssignmentPeriodRule _nightlyRestRule;
		private readonly ITeamBlockSchedulingOptions _teamBlockSchedulingOptions;

		public TeamBlockRestrictionAggregator(
			IEffectiveRestrictionCreator effectiveRestrictionCreator,
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
		
		public IEffectiveRestriction Aggregate(ITeamBlockInfo teamBlockInfo, ISchedulingOptions schedulingOptions)
		{
			return Aggregate(teamBlockInfo, schedulingOptions, null);
		}

		public IEffectiveRestriction Aggregate(ITeamBlockInfo teamBlockInfo, ISchedulingOptions schedulingOptions,
		                                       IShiftProjectionCache roleModel)
		{
			if (teamBlockInfo == null)
				return null;
			var dateOnlyList = teamBlockInfo.BlockInfo.BlockPeriod.DayCollection();
			if (dateOnlyList == null) return null;

			var groupPerson = teamBlockInfo.TeamInfo.GroupPerson;
			var matrixList = teamBlockInfo.TeamInfo.MatrixesForGroup().ToList();
			var scheduleDictionary = _schedulingResultStateHolder.Schedules;
			var timeZone = TeleoptiPrincipal.Current.Regional.TimeZone;

			IEffectiveRestriction effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(),
			                                                                      new EndTimeLimitation(),
			                                                                      new WorkTimeLimitation(), null, null, null,
			                                                                      new List<IActivityRestriction>());

			effectiveRestriction = combineRestriction(new TeamBlockEffectiveRestrcition(_effectiveRestrictionCreator, groupPerson.GroupMembers, schedulingOptions,
					                                  scheduleDictionary), dateOnlyList, matrixList, effectiveRestriction);

			if (_teamBlockSchedulingOptions.IsBlockSchedulingWithSameStartTime(schedulingOptions) ||
			    _teamBlockSchedulingOptions.IsTeamSchedulingWithSameStartTime(schedulingOptions))
			{
				effectiveRestriction = combineRestriction(new SameStartTimeRestriction(timeZone), dateOnlyList,
				                                          matrixList, effectiveRestriction);
			}
			if (_teamBlockSchedulingOptions.IsTeamSchedulingWithSameEndTime(schedulingOptions))
			{
				effectiveRestriction = combineRestriction(new SameEndTimeRestriction(timeZone), dateOnlyList, matrixList,
				                                          effectiveRestriction);
			}
			if (_teamBlockSchedulingOptions.IsBlockSchedulingWithSameShift(schedulingOptions))
			{
				effectiveRestriction = combineRestriction(new SameShiftRestriction(_scheduleDayEquator), dateOnlyList, matrixList,
				                                          effectiveRestriction);
			}
			if (_teamBlockSchedulingOptions.IsBlockSchedulingWithSameShiftCategory(schedulingOptions) ||
			    _teamBlockSchedulingOptions.IsTeamSchedulingWithSameShiftCategory(schedulingOptions))
			{
				effectiveRestriction = combineRestriction(new SameShiftCategoryRestriction(), dateOnlyList, matrixList,
				                                          effectiveRestriction);
			}
			if (roleModel != null)
			{
				effectiveRestriction = combineRestriction(new ResctrictionFromRoleModelRestriction(roleModel, _teamBlockSchedulingOptions, schedulingOptions), dateOnlyList,
						matrixList, effectiveRestriction);
			}
			effectiveRestriction = combineRestriction(new NightlyRestRestrcition(_nightlyRestRule), dateOnlyList, matrixList,
			                                          effectiveRestriction);

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
