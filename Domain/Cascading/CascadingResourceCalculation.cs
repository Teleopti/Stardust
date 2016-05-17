using System;
using System.Linq;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class CascadingResourceCalculation
	{
		private readonly IResourceOptimizationHelper _resourceOptimizationHelper;
		private readonly Func<ISchedulerStateHolder> _stateHolder;
		private readonly VirtualSkillContext _virtualSkillContext;

		public CascadingResourceCalculation(IResourceOptimizationHelper resourceOptimizationHelper,
																Func<ISchedulerStateHolder> stateHolder,
																VirtualSkillContext virtualSkillContext)
		{
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_stateHolder = stateHolder;
			_virtualSkillContext = virtualSkillContext;
		}

		public void ForDay(DateOnly date)
		{
			using (_virtualSkillContext.Create(new DateOnlyPeriod(date, date)))
			{
				//TODO - we don't want to do this for every day, need a DateOnlyPeriod method as well?
				using (new ResourceCalculationContextFactory(_stateHolder, () => new CascadingPersonSkillProvider()).Create())
				{
					_resourceOptimizationHelper.ResourceCalculateDate(date, false, false); //ska vara true, true - fixa och lägg på test senare

					//just hack for now
					var stateHolder = _stateHolder();
					var skills = stateHolder.SchedulingResultState.Skills;
					var cascadingSkills = skills.Where(x => x.IsCascading()).OrderBy(x => x.CascadingIndex); //lägg nån annanstans
					//TODO: hantera deletade skills?? (kanske inte behövs här)
					foreach (var skillToMoveFrom in cascadingSkills)
					{
						var datetimePeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDate(date, date.AddDays(1), skillToMoveFrom.TimeZone);
						var intervals = datetimePeriod.Intervals(TimeSpan.FromMinutes(skillToMoveFrom.DefaultResolution));
						var skillStaffPeriodFromDic = stateHolder.SchedulingResultState.SkillStaffPeriodHolder.SkillSkillStaffPeriodDictionary[skillToMoveFrom];
						foreach (var interval in intervals)
						{
							ISkillStaffPeriod skillStaffPeriodFrom;
							if (skillStaffPeriodFromDic.TryGetValue(interval, out skillStaffPeriodFrom))
							{
								var skillToMoveFromAbsoluteDifference = skillStaffPeriodFrom.AbsoluteDifference;
								if (skillToMoveFromAbsoluteDifference > 0)
								{
									var remainingOverstaff = skillToMoveFromAbsoluteDifference;
									foreach (var skillToMoveTo in cascadingSkills.Where(x => x.Activity.Equals(skillToMoveFrom.Activity)))
									{
										var skillsInSameGroup = VirtualSkillContext.VirtualSkillGroupResult.SkillsInSameGroupAs(skillToMoveFrom);
										if(!skillsInSameGroup.Contains(skillToMoveTo))
											continue;
									

										ISkillStaffPeriod skillStaffPeriodTo;
										var skillStaffPeriodToDic = stateHolder.SchedulingResultState.SkillStaffPeriodHolder.SkillSkillStaffPeriodDictionary[skillToMoveTo];
										if (skillStaffPeriodToDic.TryGetValue(interval, out skillStaffPeriodTo))
										{
											var skillToMoveToAbsoluteDifference = skillStaffPeriodTo.AbsoluteDifference;
											if (skillToMoveToAbsoluteDifference < 0)
											{
											
												var maxResourceToMove = Math.Min(Math.Abs(skillToMoveToAbsoluteDifference), remainingOverstaff);
												remainingOverstaff -= maxResourceToMove;

												skillStaffPeriodTo.SetCalculatedResource65(skillStaffPeriodTo.CalculatedResource + maxResourceToMove);
												skillStaffPeriodFrom.SetCalculatedResource65(skillStaffPeriodFrom.CalculatedResource - maxResourceToMove);

												if (Math.Abs(remainingOverstaff) < 0.0000000000000001d)
													break;
											}
										}
									}
								}
							}
						}
					}
				}
			
			}
		}
	}
}