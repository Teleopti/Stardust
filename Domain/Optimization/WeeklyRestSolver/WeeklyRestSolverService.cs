﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver
{
	public interface IWeeklyRestSolverService
	{
		void Execute(IList<IPerson> selectedPersons, DateOnlyPeriod selectedPeriod, ITeamBlockGenerator teamBlockGenerator,
			ISchedulePartModifyAndRollbackService rollbackService, IResourceCalculateDelayer resourceCalculateDelayer,
			ISchedulingResultStateHolder schedulingResultStateHolder, IList<IScheduleMatrixPro> allPersonMatrixList,
			IOptimizationPreferences optimizationPreferences, ISchedulingOptions schedulingOptions);

		event EventHandler<ResourceOptimizerProgressEventArgs> ResolvingWeek;
	}

	public class WeeklyRestSolverService : IWeeklyRestSolverService
	{
		private readonly IWeeksFromScheduleDaysExtractor _weeksFromScheduleDaysExtractor;
		private readonly IEnsureWeeklyRestRule _ensureWeeklyRestRule;
		private readonly IContractWeeklyRestForPersonWeek _contractWeeklyRestForPersonWeek;
		private readonly IDayOffToTimeSpanExtractor _dayOffToTimeSpanExtractor;
		private readonly IShiftNudgeManager _shiftNudgeManager;
		private readonly IdentifyDayOffWithHighestSpan _identifyDayOffWithHighestSpan;
		private readonly IDeleteScheduleDayFromUnsolvedPersonWeek _deleteScheduleDayFromUnsolvedPersonWeek;
		private readonly IAllTeamMembersInSelectionSpecification _allTeamMembersInSelectionSpecification;
		private readonly IPersonWeekViolatingWeeklyRestSpecification  _personWeekViolatingWeeklyRestSpecification;
		private readonly IBrokenWeekCounterForAPerson  _brokenWeekCounterForAPerson;
		private readonly ITeamBlockShiftCategoryLimitationValidator _teamBlockShiftCategoryLimitationValidator;
		public event EventHandler<ResourceOptimizerProgressEventArgs> ResolvingWeek;

		public WeeklyRestSolverService(IWeeksFromScheduleDaysExtractor weeksFromScheduleDaysExtractor,
			IEnsureWeeklyRestRule ensureWeeklyRestRule, IContractWeeklyRestForPersonWeek contractWeeklyRestForPersonWeek,
			IDayOffToTimeSpanExtractor dayOffToTimeSpanExtractor, IShiftNudgeManager shiftNudgeManager,
			IdentifyDayOffWithHighestSpan identifyDayOffWithHighestSpan,
			IDeleteScheduleDayFromUnsolvedPersonWeek deleteScheduleDayFromUnsolvedPersonWeek,
			IAllTeamMembersInSelectionSpecification allTeamMembersInSelectionSpecification, 
			IPersonWeekViolatingWeeklyRestSpecification personWeekViolatingWeeklyRestSpecification, 
			IBrokenWeekCounterForAPerson brokenWeekCounterForAPerson,
			ITeamBlockShiftCategoryLimitationValidator teamBlockShiftCategoryLimitationValidator)
		{
			_weeksFromScheduleDaysExtractor = weeksFromScheduleDaysExtractor;
			_ensureWeeklyRestRule = ensureWeeklyRestRule;
			_contractWeeklyRestForPersonWeek = contractWeeklyRestForPersonWeek;
			_dayOffToTimeSpanExtractor = dayOffToTimeSpanExtractor;
			_shiftNudgeManager = shiftNudgeManager;
			_identifyDayOffWithHighestSpan = identifyDayOffWithHighestSpan;
			_deleteScheduleDayFromUnsolvedPersonWeek = deleteScheduleDayFromUnsolvedPersonWeek;
			_allTeamMembersInSelectionSpecification = allTeamMembersInSelectionSpecification;
			_personWeekViolatingWeeklyRestSpecification = personWeekViolatingWeeklyRestSpecification;
			_brokenWeekCounterForAPerson = brokenWeekCounterForAPerson;
			_teamBlockShiftCategoryLimitationValidator = teamBlockShiftCategoryLimitationValidator;
		}

		private CancelSignal onDayScheduled(ResourceOptimizerProgressEventArgs args)
		{
			var handler = ResolvingWeek;
			if (handler != null)
			{
				handler(this, args);
				if (args.Cancel)
					return new CancelSignal{ShouldCancel = true};
			}
			return new CancelSignal();
		}

		public void Execute(IList<IPerson> selectedPersons, DateOnlyPeriod selectedPeriod,
			ITeamBlockGenerator teamBlockGenerator, ISchedulePartModifyAndRollbackService rollbackService,
			IResourceCalculateDelayer resourceCalculateDelayer, ISchedulingResultStateHolder schedulingResultStateHolder,
			IList<IScheduleMatrixPro> allPersonMatrixList, IOptimizationPreferences optimizationPreferences,
			ISchedulingOptions schedulingOptions)
		{
			var cancel = false;
			foreach (var person in selectedPersons)
			{
				var personMatrixes = allPersonMatrixList.Where(s => s.Person == person && s.SchedulePeriod.DateOnlyPeriod.Intersection(selectedPeriod).HasValue).ToList();

				foreach (var personMatrix in personMatrixes)
				{
					var weeklyRestInPersonWeek = new Dictionary<PersonWeek, TimeSpan>();
					var personScheduleRange = schedulingResultStateHolder.Schedules[person];
					var selectedPeriodScheduleDays = personScheduleRange.ScheduledDayCollection(selectedPeriod).ToArray();
					var selectedPersonWeeks = _weeksFromScheduleDaysExtractor.CreateWeeksFromScheduleDaysExtractor(selectedPeriodScheduleDays, false).ToList();
					var personWeeksViolatingWeeklyRest = new List<PersonWeek>();
					foreach (var personWeek in selectedPersonWeeks)
					{
						var weeklyRest = _contractWeeklyRestForPersonWeek.GetWeeklyRestFromContract(personWeek);
						if (!weeklyRestInPersonWeek.ContainsKey(personWeek))
							weeklyRestInPersonWeek.Add(personWeek, weeklyRest);

						if (!_personWeekViolatingWeeklyRestSpecification.IsSatisfyBy(personScheduleRange, personWeek, weeklyRest))
							personWeeksViolatingWeeklyRest.Add(personWeek);
					}
					var totalNumberOfBrokenWeek = _brokenWeekCounterForAPerson.CountBrokenWeek(selectedPeriodScheduleDays, personScheduleRange);
					foreach (var personWeek in personWeeksViolatingWeeklyRest)
					{
						if (cancel) return;
						if (_ensureWeeklyRestRule.HasMinWeeklyRest(personWeek, personScheduleRange, weeklyRestInPersonWeek[personWeek]))
							continue;
						var possiblePositionsToFix = _dayOffToTimeSpanExtractor.GetDayOffWithTimeSpanAmongAWeek(personWeek.Week,
							personScheduleRange);
						var firstDayOfElement = DateOnly.MinValue;
						if (possiblePositionsToFix.Count > 0)
							firstDayOfElement = possiblePositionsToFix.FirstOrDefault().Key;
						bool success = true;
						if (!possiblePositionsToFix.Any())
						{
							var endDayOfWeek = personWeek.Week.EndDate.AddDays(1);
							while(!_ensureWeeklyRestRule.HasMinWeeklyRest(personWeek, personScheduleRange, weeklyRestInPersonWeek[personWeek]))
							{
								_deleteScheduleDayFromUnsolvedPersonWeek.DeleteAppropiateScheduleDay(personScheduleRange, endDayOfWeek, rollbackService, selectedPeriod, personMatrix);
								endDayOfWeek = endDayOfWeek.AddDays(-1);
								if(endDayOfWeek < personWeek.Week.StartDate)
									break;
							}						
						}
						else
						{
							while(possiblePositionsToFix.Count() != 0)
							{
								var progressResult = onDayScheduled(new ResourceOptimizerProgressEventArgs(0, 0,
									string.Format(UserTexts.Resources.ResolvingWeeklyRestFor, personWeek.Person.Name.FirstName,
										personWeek.Person.Name.LastName),()=>cancel=true));
								if (cancel || progressResult.ShouldCancel) return;

								var highProbablePosition = _identifyDayOffWithHighestSpan.GetHighProbableDayOffPosition(possiblePositionsToFix);
								success = _shiftNudgeManager.TrySolveForDayOff(personWeek, highProbablePosition,
									teamBlockGenerator, allPersonMatrixList, 
									rollbackService, resourceCalculateDelayer,
									schedulingResultStateHolder, selectedPeriod, 
									selectedPersons, optimizationPreferences, 
									schedulingOptions);

								if (success)
								{
									var teamBlockInfo = teamBlockGenerator.Generate(allPersonMatrixList, personWeek.Week, new List<IPerson> { person }, schedulingOptions).First();
									var brokenShiftCategoryLimitations = !_teamBlockShiftCategoryLimitationValidator.Validate(teamBlockInfo, null, optimizationPreferences);
									
									var currentBrokenWeek = _brokenWeekCounterForAPerson.CountBrokenWeek(selectedPeriodScheduleDays,
									personScheduleRange);
									if (currentBrokenWeek >= totalNumberOfBrokenWeek || brokenShiftCategoryLimitations)
									{
										//rollback this week 
										_shiftNudgeManager.RollbackLastScheduledWeek(rollbackService, resourceCalculateDelayer);
										if (isFullTeamSelected(selectedPersons, personWeek.Person, teamBlockGenerator, schedulingOptions,
											allPersonMatrixList, personWeek.Week))
											_deleteScheduleDayFromUnsolvedPersonWeek.DeleteAppropiateScheduleDay(personScheduleRange,
												possiblePositionsToFix.First().Key, rollbackService, selectedPeriod, personMatrix);
									}
									break;
								}

								possiblePositionsToFix.Remove(highProbablePosition);
							}
							if (!success && firstDayOfElement != DateOnly.MinValue)
							{
								if (isFullTeamSelected(selectedPersons, personWeek.Person, teamBlockGenerator, schedulingOptions, allPersonMatrixList, personWeek.Week))
									_deleteScheduleDayFromUnsolvedPersonWeek.DeleteAppropiateScheduleDay(personScheduleRange, firstDayOfElement, rollbackService, selectedPeriod, personMatrix);
							}
						}
					}
				}
			}

		}

		private bool isFullTeamSelected(IList<IPerson> selectedPersons, IPerson person, ITeamBlockGenerator teamBlockGenerator,
			ISchedulingOptions schedulingOptions, IList<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod week)
		{
			var teamBlockInfo =
				teamBlockGenerator.Generate(allPersonMatrixList, week, new List<IPerson> {person}, schedulingOptions).First();
			return _allTeamMembersInSelectionSpecification.IsSatifyBy(teamBlockInfo.TeamInfo, selectedPersons);
		}
	}
}
