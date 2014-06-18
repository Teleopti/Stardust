using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
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
		private readonly IBrokenWeekOutsideSelectionSpecification _brokenWeekOutsideSelectionSpecification;
		private bool _cancelMe;
		private readonly IAllTeamMembersInSelectionSpecification _allTeamMembersInSelectionSpecification;
		private readonly IPersonWeekVoilatingWeeklyRestSpecification  _personWeekVoilatingWeeklyRestSpecification;
		public event EventHandler<ResourceOptimizerProgressEventArgs> ResolvingWeek;

		public WeeklyRestSolverService(IWeeksFromScheduleDaysExtractor weeksFromScheduleDaysExtractor,
			IEnsureWeeklyRestRule ensureWeeklyRestRule, IContractWeeklyRestForPersonWeek contractWeeklyRestForPersonWeek,
			IDayOffToTimeSpanExtractor dayOffToTimeSpanExtractor, IShiftNudgeManager shiftNudgeManager,
			IdentifyDayOffWithHighestSpan identifyDayOffWithHighestSpan,
			IDeleteScheduleDayFromUnsolvedPersonWeek deleteScheduleDayFromUnsolvedPersonWeek,
			IBrokenWeekOutsideSelectionSpecification brokenWeekOutsideSelectionSpecification,
			IAllTeamMembersInSelectionSpecification allTeamMembersInSelectionSpecification, IPersonWeekVoilatingWeeklyRestSpecification personWeekVoilatingWeeklyRestSpecification)
		{
			_weeksFromScheduleDaysExtractor = weeksFromScheduleDaysExtractor;
			_ensureWeeklyRestRule = ensureWeeklyRestRule;
			_contractWeeklyRestForPersonWeek = contractWeeklyRestForPersonWeek;
			_dayOffToTimeSpanExtractor = dayOffToTimeSpanExtractor;
			_shiftNudgeManager = shiftNudgeManager;
			_identifyDayOffWithHighestSpan = identifyDayOffWithHighestSpan;
			_deleteScheduleDayFromUnsolvedPersonWeek = deleteScheduleDayFromUnsolvedPersonWeek;
			_brokenWeekOutsideSelectionSpecification = brokenWeekOutsideSelectionSpecification;
			_allTeamMembersInSelectionSpecification = allTeamMembersInSelectionSpecification;
			_personWeekVoilatingWeeklyRestSpecification = personWeekVoilatingWeeklyRestSpecification;
		}

		protected virtual void OnDayScheduled(ResourceOptimizerProgressEventArgs resourceOptimizerProgressEventArgs)
		{
			EventHandler<ResourceOptimizerProgressEventArgs> temp = ResolvingWeek;
			if (temp != null)
			{
				temp(this, resourceOptimizerProgressEventArgs);
			}
			_cancelMe = resourceOptimizerProgressEventArgs.Cancel;
		}

		public void Execute(IList<IPerson> selectedPersons, DateOnlyPeriod selectedPeriod,
			ITeamBlockGenerator teamBlockGenerator, ISchedulePartModifyAndRollbackService rollbackService,
			IResourceCalculateDelayer resourceCalculateDelayer, ISchedulingResultStateHolder schedulingResultStateHolder,
			IList<IScheduleMatrixPro> allPersonMatrixList, IOptimizationPreferences optimizationPreferences,
			ISchedulingOptions schedulingOptions)
		{
			foreach (var person in selectedPersons)
			{
				var personMatrix = allPersonMatrixList.FirstOrDefault(s => s.Person == person);
				var weeklyRestInPersonWeek = new Dictionary<PersonWeek, TimeSpan>();
				if (personMatrix != null)
				{
					var personScheduleRange = schedulingResultStateHolder.Schedules[person];
					var selectedPeriodScheduleDays = personScheduleRange.ScheduledDayCollection(selectedPeriod);
					var selctedPersonWeeks =
						_weeksFromScheduleDaysExtractor.CreateWeeksFromScheduleDaysExtractor(selectedPeriodScheduleDays,
							false).ToList();
					var personWeeksVoilatingWeeklyRest = new List<PersonWeek>();
					foreach (var personWeek in selctedPersonWeeks)
					{
						var weeklyRest = _contractWeeklyRestForPersonWeek.GetWeeklyRestFromContract(personWeek);
						if (!weeklyRestInPersonWeek.ContainsKey(personWeek))
							weeklyRestInPersonWeek.Add(personWeek, weeklyRest);
						
						if (!_personWeekVoilatingWeeklyRestSpecification.IsSatisfyBy(personScheduleRange, personWeek, weeklyRest))
							personWeeksVoilatingWeeklyRest.Add(personWeek);
					}
					var selctedPersonWeeksWithNeighbouringWeeks =
						_weeksFromScheduleDaysExtractor.CreateWeeksFromScheduleDaysExtractor(selectedPeriodScheduleDays,
							true).ToList();
					foreach (var personWeek in personWeeksVoilatingWeeklyRest)
					{
						if (_ensureWeeklyRestRule.HasMinWeeklyRest(personWeek, personScheduleRange, weeklyRestInPersonWeek[personWeek]))
							continue;
						var possiblePositionsToFix = _dayOffToTimeSpanExtractor.GetDayOffWithTimeSpanAmongAWeek(personWeek.Week,
							personScheduleRange);
						var fisrtDayOfElement = DateOnly.MinValue;
						if (possiblePositionsToFix.Count > 0)
							fisrtDayOfElement = possiblePositionsToFix.FirstOrDefault().Key;
						bool success = true;
						if (!possiblePositionsToFix.Any())
						{
							var endDayOfWeek = personWeek.Week.EndDate.AddDays(1);
							while (
								!_ensureWeeklyRestRule.HasMinWeeklyRest(personWeek, personScheduleRange, weeklyRestInPersonWeek[personWeek]))
							{
								_deleteScheduleDayFromUnsolvedPersonWeek.DeleteAppropiateScheduleDay(personScheduleRange, endDayOfWeek
												, rollbackService, selectedPeriod);
								endDayOfWeek = endDayOfWeek.AddDays(-1);
							}
							
						}
						else
						{
							while (possiblePositionsToFix.Count() != 0)
							{
								OnDayScheduled(new ResourceOptimizerProgressEventArgs(0, 0,
									string.Format(UserTexts.Resources.ResolvingWeeklyRestFor, personWeek.Person.Name.FirstName,
										personWeek.Person.Name.LastName)));
								if (_cancelMe)
									return;
								var highProbablePosition = _identifyDayOffWithHighestSpan.GetHighProbableDayOffPosition(possiblePositionsToFix);
								success = _shiftNudgeManager.TrySolveForDayOff(personWeek, highProbablePosition,
									teamBlockGenerator,
									allPersonMatrixList, rollbackService, resourceCalculateDelayer,
									schedulingResultStateHolder, selectedPeriod, selectedPersons, optimizationPreferences, schedulingOptions);

								if (success)
								{
									var foundProblemWithThisWeek = _brokenWeekOutsideSelectionSpecification.IsSatisfy(personWeek,
										selctedPersonWeeksWithNeighbouringWeeks, weeklyRestInPersonWeek, personScheduleRange);
									if (foundProblemWithThisWeek)
									{
										//rollback this week 
										_shiftNudgeManager.RollbackLastScheduledWeek(rollbackService, resourceCalculateDelayer);
										if (isFullTeamSelected(selectedPersons, personWeek.Person, teamBlockGenerator, schedulingOptions,
											allPersonMatrixList, personWeek.Week))
											_deleteScheduleDayFromUnsolvedPersonWeek.DeleteAppropiateScheduleDay(personScheduleRange,
												possiblePositionsToFix.First().Key, rollbackService, selectedPeriod);
									}
									break;
								}

								possiblePositionsToFix.Remove(highProbablePosition);
							}
							if (!success && fisrtDayOfElement != DateOnly.MinValue)
							{
								if (isFullTeamSelected(selectedPersons, personWeek.Person, teamBlockGenerator, schedulingOptions,
									allPersonMatrixList, personWeek.Week))
									_deleteScheduleDayFromUnsolvedPersonWeek.DeleteAppropiateScheduleDay(personScheduleRange, fisrtDayOfElement,
										rollbackService, selectedPeriod);
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
