using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Cascading;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Secrets.Furness;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests
{
	public class IntradayCascadingRequestProcessor : IIntradayRequestProcessor
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(IntradayRequestProcessor));
		private readonly ICommandDispatcher _commandDispatcher;
		private readonly ISkillCombinationResourceRepository _skillCombinationResourceRepository;
		private readonly IScheduleForecastSkillReadModelRepository _scheduleForecastSkillReadModelRepository;
		private readonly IPersonSkillProvider _personSkillProvider;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly ICurrentScenario _currentScenario;
		private readonly ISkillRepository _skillRepository;
		private readonly IActivityRepository _activityRepository;
		private readonly AddResourcesToSubSkills _addResourcesToSubSkills;
		private readonly ReducePrimarySkillResources _reducePrimarySkillResources;
		private readonly PrimarySkillOverstaff _primarySkillOverstaff;


		private const double _quotient = 1d; // the outer quotient: default = 1
		private const int _maximumIteration = 100; // the maximum number of iterations

		public IntradayCascadingRequestProcessor(ICommandDispatcher commandDispatcher, 
			ISkillCombinationResourceRepository skillCombinationResourceRepository, IPersonSkillProvider personSkillProvider, 
			IScheduleStorage scheduleStorage, ICurrentScenario currentScenario, IScheduleForecastSkillReadModelRepository scheduleForecastSkillReadModelRepository,
			ISkillRepository skillRepository, IActivityRepository activityRepository, AddResourcesToSubSkills addResourcesToSubSkills, ReducePrimarySkillResources reducePrimarySkillResources, PrimarySkillOverstaff primarySkillOverstaff)
		{
			_commandDispatcher = commandDispatcher;
			_skillCombinationResourceRepository = skillCombinationResourceRepository;
			_personSkillProvider = personSkillProvider;
			_scheduleStorage = scheduleStorage;
			_currentScenario = currentScenario;
			_scheduleForecastSkillReadModelRepository = scheduleForecastSkillReadModelRepository;
			_skillRepository = skillRepository;
			_activityRepository = activityRepository;
			_addResourcesToSubSkills = addResourcesToSubSkills;
			_reducePrimarySkillResources = reducePrimarySkillResources;
			_primarySkillOverstaff = primarySkillOverstaff;
		}

		public void Process(IPersonRequest personRequest, DateTime startTime)
		{
			//Need to validate skillCombo Read model
			//if (!_scheduleForecastSkillReadModelValidator.Validate(_currentBusinessUnit.Current().Id.GetValueOrDefault()))
			//{
			//	sendDenyCommand(personRequest.Id.GetValueOrDefault(), Resources.DenyDueToTechnicalProblems);
			//	return;
			//}

			//what if the agent changes personPeriod in the middle of the request period?

			var combinationResources = _skillCombinationResourceRepository.LoadSkillCombinationResources(personRequest.Request.Period).ToArray();
			if (!combinationResources.Any()) return;

			var schedules = _scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(personRequest.Person, new ScheduleDictionaryLoadOptions(false, false), personRequest.Request.Period, _currentScenario.Current())[personRequest.Person];

			var dateOnlyPeriod = personRequest.Request.Period.ToDateOnlyPeriod(personRequest.Person.PermissionInformation.DefaultTimeZone());

			var scheduleDays = schedules.ScheduledDayCollection(dateOnlyPeriod);
			var allSkills = _skillRepository.LoadAll();
			var skillsByActivity = allSkills.ToLookup(s => s.Activity.Id.GetValueOrDefault());
			var skillStaffingIntervals = _scheduleForecastSkillReadModelRepository.GetBySkills(allSkills.Select(x => x.Id.GetValueOrDefault()).ToArray(), personRequest.Request.Period.StartDateTime, personRequest.Request.Period.EndDateTime);
			skillStaffingIntervals.ForEach(s => s.StaffingLevel = 0);

			var skillCombinationResource = combinationResources.FirstOrDefault();
			var skillInterval = (int)skillCombinationResource.EndDateTime.Subtract(skillCombinationResource.StartDateTime).TotalMinutes;
			foreach (var day in scheduleDays)
			{
				var projection = day.ProjectionService().CreateProjection().FilterLayers(personRequest.Request.Period);
				
				var layers = projection.ToResourceLayers(skillInterval);
				
				foreach (var layer in layers)
				{
					var activity = _activityRepository.Get(layer.PayloadId);
					var skillCombination = _personSkillProvider.SkillsOnPersonDate(personRequest.Person, dateOnlyPeriod.StartDate).ForActivity(layer.PayloadId);
					if (!skillCombination.Skills.Any()) continue;

					combinationResources.Single(x => x.SkillCombination.NonSequenceEquals(skillCombination.Skills.Select(y => y.Id.GetValueOrDefault()))
													 && x.StartDateTime == layer.Period.StartDateTime).Resource -= 1;

					var dividedActivity = new DividedActivityData();
					var elapsedToCalculate = layer.Period.ElapsedTime();

					var skillsForActivity = skillsByActivity[layer.PayloadId];
					foreach (ISkill skill in skillsForActivity)
					{
						double? targetDemandValue = skillStaffingIntervals.FirstOrDefault(s => s.SkillId == skill.Id.GetValueOrDefault() && s.StartDateTime == layer.Period.StartDateTime)?.Forecast;
						if (targetDemandValue.HasValue)
							dividedActivity.TargetDemands.Add(skill, targetDemandValue.Value);
					}

					var resources = combinationResources.Where(c => c.SkillCombination.Any(s => skillsForActivity.Any(x => x.Id.GetValueOrDefault() == s)) && c.StartDateTime == layer.Period.StartDateTime);
					foreach (var periodResource in resources)
					{
						if (periodResource.Resource == 0) continue;
						var skills = periodResource.SkillCombination;
						
						var personSkillEfficiencyRow = new Dictionary<ISkill, double>();
						var relativePersonSkillResourceRow = new Dictionary<ISkill, double>();
						var personSkillResourceRow = new Dictionary<ISkill, double>();

						foreach (var skill in skills)
						{
							var intervalPart = 1;
							var traff = periodResource.Resource * intervalPart;
							var realSkill = skillsForActivity.First(s => s.Id.GetValueOrDefault() == skill);
							
							double personSkillResourceValue = traff;
							double bitwiseSkillEfficiencyValue = 1d;
							double relativePersonSkillResourceValue = traff * bitwiseSkillEfficiencyValue;

							relativePersonSkillResourceRow.Add(realSkill, relativePersonSkillResourceValue);
							personSkillResourceRow.Add(realSkill, personSkillResourceValue);
							personSkillEfficiencyRow.Add(realSkill, 1);

							// add to sum also
							double currentRelativePersonSkillResourceValue;
							if (dividedActivity.RelativePersonSkillResourcesSum.TryGetValue(realSkill, out currentRelativePersonSkillResourceValue))
							{
								dividedActivity.RelativePersonSkillResourcesSum[realSkill] = currentRelativePersonSkillResourceValue + relativePersonSkillResourceValue;
							}
							else
							{
								dividedActivity.RelativePersonSkillResourcesSum.Add(realSkill, relativePersonSkillResourceValue);
							}
						}

						var key = string.Join("_", periodResource.SkillCombination);
						if (!dividedActivity.KeyedSkillResourceEfficiencies.ContainsKey(key))
						{
							dividedActivity.KeyedSkillResourceEfficiencies.Add(key, personSkillEfficiencyRow);
							dividedActivity.WeightedRelativeKeyedSkillResourceResources.Add(key, personSkillResourceRow);
							dividedActivity.RelativeKeyedSkillResourceResources.Add(key, relativePersonSkillResourceRow);
							dividedActivity.RelativePersonResources.Add(key, periodResource.Resource);

							double targetResourceValue = elapsedToCalculate.TotalMinutes * periodResource.Resource;
							dividedActivity.PersonResources.Add(key, targetResourceValue);
						}
					}
					
					var relevantSkillStaffPeriods = skillStaffingIntervals.Where(s => skillsForActivity.Any(x => x.Id.GetValueOrDefault() == s.SkillId) && s.StartDateTime == layer.Period.StartDateTime).Select(y =>
																																																				 {
																																																					 return new
																																																						 {Skill = skillsForActivity.First(s => s.Id.GetValueOrDefault() == y.SkillId), y};

																																																				 }).ToDictionary(k => k.Skill,v => (IResourceCalculationPeriod)v.y);

					

					if (relevantSkillStaffPeriods.Count > 0)
					{
						var furnessDataConverter = new FurnessDataConverter(dividedActivity);
						IFurnessData furnessData = furnessDataConverter.ConvertDividedActivityToFurnessData();

						IFurnessEvaluator furnessEvaluator = new FurnessEvaluator(furnessData);
						furnessEvaluator.Evaluate(_quotient, _maximumIteration, Calculation.Variances.StandardDeviation);
						IDividedActivityData optimizedActivityData = furnessDataConverter.ConvertFurnessDataBackToActivity();
						
						setFurnessResultsToSkillStaffPeriods(layer.Period, relevantSkillStaffPeriods, optimizedActivityData);
					}

					//Shovling
					var something = new SkillStaffIntervalHolder(relevantSkillStaffPeriods);
					var cascadingSkills = new CascadingSkills(allSkills);
					if (cascadingSkills.Any())
					{
						
						var cascadingSkillsForActivity = cascadingSkills.ForActivity(activity).ToArray();
						var cascadingSkillGroups = new List<CascadingSkillGroup>();
						
							var cascadingSkillsInSkillGroup = cascadingSkillsForActivity.Where(x => skillsForActivity.Contains(x)).ToArray();
							if (!cascadingSkillsInSkillGroup.Any())
								continue;

							var lowestCascadingIndex = cascadingSkillsInSkillGroup.Min(x => x.CascadingIndex.Value);
							var primarySkills = cascadingSkillsInSkillGroup.Where(x => x.CascadingIndex.Value == lowestCascadingIndex);
							var cascadingSubSkills = new List<SubSkillsWithSameIndex>();
							foreach (var skillInSameChainAsPrimarySkill in cascadingSkillsInSkillGroup.Where(x => !primarySkills.Contains(x)))
							{
								var last = cascadingSubSkills.LastOrDefault();
								if (last == null || !skillInSameChainAsPrimarySkill.CascadingIndex.Value.Equals(last.CascadingIndex))
								{
									var cascadingSkillGroupItem = new SubSkillsWithSameIndex();
									cascadingSkillGroupItem.AddSubSkill(skillInSameChainAsPrimarySkill);
									cascadingSubSkills.Add(cascadingSkillGroupItem);
								}
								else
								{
									last.AddSubSkill(skillInSameChainAsPrimarySkill);
								}
							}
							cascadingSkillGroups.Add(new CascadingSkillGroup(primarySkills, cascadingSubSkills, resources.Sum(x => x.Resource)));
						
						cascadingSkillGroups.Sort(new CascadingSkillGroupSorter());

						var orderedSkillGroups =  mergeSkillGroupsWithSameIndex(cascadingSkillGroups);


						var allSkillGroups = orderedSkillGroups.AllSkillGroups();
						foreach (var skillGroupsWithSameIndex in orderedSkillGroups)
						{
							var state = _primarySkillOverstaff.AvailableSum(something, allSkillGroups, skillGroupsWithSameIndex, layer.Period);
							_addResourcesToSubSkills.Execute(state, something, skillGroupsWithSameIndex, layer.Period);
							_reducePrimarySkillResources.Execute(state, something, layer.Period);
						}
					}
				}
			}
			
			var isDenied = false;
			foreach (var interval in skillStaffingIntervals)
			{
				if (interval.StaffingLevel < interval.Forecast)
				{
					sendDenyCommand(personRequest.Id.GetValueOrDefault(), "Understaffed");
					isDenied = true;
					break;
				}
			}
			if (!isDenied) sendApproveCommand(personRequest.Id.GetValueOrDefault());
		}

		

		private static OrderedSkillGroups mergeSkillGroupsWithSameIndex(IEnumerable<CascadingSkillGroup> cascadingSkillGroups)
		{
			var ret = new List<List<CascadingSkillGroup>>();
			foreach (var skillGroup in cascadingSkillGroups)
			{
				var retLast = ret.LastOrDefault();
				if (retLast == null || retLast.First().SkillGroupIndexHash() != skillGroup.SkillGroupIndexHash())
				{
					ret.Add(new List<CascadingSkillGroup> { skillGroup });
				}
				else
				{
					ret.Last().Add(skillGroup);
				}
			}
			return new OrderedSkillGroups(ret);
		}

		private static void setFurnessResultsToSkillStaffPeriods(DateTimePeriod completeIntervalPeriod, IDictionary<ISkill, IResourceCalculationPeriod> relevantSkillStaffPeriods, IDividedActivityData optimizedActivityData)
		{
			foreach (var skillPair in relevantSkillStaffPeriods)
			{
				IResourceCalculationPeriod staffPeriod = skillPair.Value;
				double resource;
				optimizedActivityData.WeightedRelativePersonSkillResourcesSum.TryGetValue(skillPair.Key, out resource);
				double calculatedresource = resource / completeIntervalPeriod.ElapsedTime().TotalMinutes;
				staffPeriod.SetCalculatedResource65(calculatedresource);
			}
		}

		private bool sendDenyCommand(Guid personRequestId, string denyReason)
		{
			var command = new DenyRequestCommand()
			{
				PersonRequestId = personRequestId,
				DenyReason = denyReason
			};
			_commandDispatcher.Execute(command);

			if (command.ErrorMessages.Any())
			{
				logger.Warn(command.ErrorMessages);
			}

			return !command.ErrorMessages.Any();
		}

		private bool sendApproveCommand(Guid personRequestId)
		{
			var command = new ApproveRequestCommand()
			{
				PersonRequestId = personRequestId,
				IsAutoGrant = true
			};
			_commandDispatcher.Execute(command);

			if (command.ErrorMessages.Any())
			{
				logger.Warn(command.ErrorMessages);
			}

			return !command.ErrorMessages.Any();
		}

	}
}
