using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.Intraday;
using Teleopti.Ccc.Domain.Cascading;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests
{
	public class IntradayCascadingRequestProcessor : IIntradayRequestProcessor
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(IntradayCascadingRequestProcessor));
		private readonly IActivityRepository _activityRepository;
		private readonly AddResourcesToSubSkills _addResourcesToSubSkills;
		private readonly ICommandDispatcher _commandDispatcher;
		private readonly ICurrentScenario _currentScenario;
		private readonly IPersonSkillProvider _personSkillProvider;
		private readonly PrimarySkillOverstaff _primarySkillOverstaff;
		private readonly ReducePrimarySkillResources _reducePrimarySkillResources;
		private readonly IScheduleForecastSkillReadModelRepository _scheduleForecastSkillReadModelRepository;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly ISkillCombinationResourceRepository _skillCombinationResourceRepository;
		private readonly ISkillRepository _skillRepository;
		private readonly ISkillCombinationResourceReadModelValidator _skillCombinationResourceReadModelValidator;
		private readonly SkillGroupPerActivityProvider _skillGroupPerActivityProvider;
		private readonly IAbsenceRequestValidatorProvider _absenceRequestValidatorProvider;

		public IntradayCascadingRequestProcessor(ICommandDispatcher commandDispatcher,
												 ISkillCombinationResourceRepository skillCombinationResourceRepository, IPersonSkillProvider personSkillProvider,
												 IScheduleStorage scheduleStorage, ICurrentScenario currentScenario, IScheduleForecastSkillReadModelRepository scheduleForecastSkillReadModelRepository,
												 ISkillRepository skillRepository, IActivityRepository activityRepository, AddResourcesToSubSkills addResourcesToSubSkills,
			ReducePrimarySkillResources reducePrimarySkillResources, PrimarySkillOverstaff primarySkillOverstaff, ISkillCombinationResourceReadModelValidator skillCombinationResourceReadModelValidator, 
			SkillGroupPerActivityProvider skillGroupPerActivityProvider, IAbsenceRequestValidatorProvider absenceRequestValidatorProvider)
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
			_skillCombinationResourceReadModelValidator = skillCombinationResourceReadModelValidator;
			_skillGroupPerActivityProvider = skillGroupPerActivityProvider;
			_absenceRequestValidatorProvider = absenceRequestValidatorProvider;
		}

		public void Process(IPersonRequest personRequest, DateTime startTime)
		{
			try
			{
				if (!_skillCombinationResourceReadModelValidator.Validate())
				{
					logger.Warn(Resources.DenyDueToTechnicalProblems + "Read model is not up to date");
					sendDenyCommand(personRequest.Id.GetValueOrDefault(), Resources.DenyDueToTechnicalProblems);
					return;
				}

				//what if the agent changes personPeriod in the middle of the request period?
				//what if the request is 8:00-8:05, only a third of a resource should be removed

				var combinationResources = _skillCombinationResourceRepository.LoadSkillCombinationResources(personRequest.Request.Period);
				if (!combinationResources.Any())
				{
					logger.Warn(Resources.DenyDueToTechnicalProblems + " Can not find any skillcombinations.");
					sendDenyCommand(personRequest.Id.GetValueOrDefault(), Resources.DenyDueToTechnicalProblems);
					return;
				}

				var schedules = _scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(personRequest.Person, new ScheduleDictionaryLoadOptions(false, false), personRequest.Request.Period, _currentScenario.Current())[personRequest.Person];
				//night shift?
				var dateOnlyPeriod = personRequest.Request.Period.ToDateOnlyPeriod(personRequest.Person.PermissionInformation.DefaultTimeZone());

				var scheduleDays = schedules.ScheduledDayCollection(dateOnlyPeriod);
				var allSkills = _skillRepository.LoadAll();
				var skillStaffingIntervals = _scheduleForecastSkillReadModelRepository.GetBySkills(allSkills.Select(x => x.Id.GetValueOrDefault()).ToArray(), personRequest.Request.Period.StartDateTime, personRequest.Request.Period.EndDateTime);
				skillStaffingIntervals.ForEach(s => s.StaffingLevel = 0);

				var skillIds = new HashSet<Guid>();
				foreach (var skillCombinationResource in combinationResources)
				{
					foreach (var skillId in skillCombinationResource.SkillCombination)
					{
						skillIds.Add(skillId);
					}
				}
				var skillInterval = allSkills.Where(x => skillIds.Contains(x.Id.Value)).Min(x => x.DefaultResolution);

				var relevantSkillStaffPeriods =
					skillStaffingIntervals.GroupBy(s => allSkills.First(a => a.Id.GetValueOrDefault() == s.SkillId))
						.ToDictionary(k => k.Key,
							v =>
								(IResourceCalculationPeriodDictionary)
								new ResourceCalculationPeriodDictionary(v.ToDictionary(d => d.DateTimePeriod,
									s => (IResourceCalculationPeriod) s)));
				var resourcesForCalculation = new ResourcesExtractorCalculation(combinationResources, allSkills, skillInterval);
				var resourcesForShovel = new ResourcesExtractorShovel(combinationResources, allSkills, skillInterval);

				var skillCombinationResourcesForAgent = new List<SkillCombinationResource>();
				foreach (var day in scheduleDays)
				{
					var projection = day.ProjectionService().CreateProjection().FilterLayers(personRequest.Request.Period);

					var layers = projection.ToResourceLayers(skillInterval);

					foreach (var layer in layers)
					{
						var activity = _activityRepository.Get(layer.PayloadId);
						if (nonSkillActivityForWholeInterval(activity, layer))
						{
							zeroDemandForIntervalsCoveredByNonSkillActivity(skillStaffingIntervals.ToList(), layer);
							continue;
						}
						var skillCombination =
							_personSkillProvider.SkillsOnPersonDate(personRequest.Person, dateOnlyPeriod.StartDate)
								.ForActivity(layer.PayloadId);
						if (!skillCombination.Skills.Any()) continue;
						
						var skillCombinationResourceByAgentAndLayer = distributeResourceSmartly(combinationResources, skillCombination, layer);
						skillCombinationResourcesForAgent.Add(skillCombinationResourceByAgentAndLayer);
						
						var scheduleResourceOptimizer = new ScheduleResourceOptimizer(resourcesForCalculation, new SlimSkillResourceCalculationPeriodWrapper(relevantSkillStaffPeriods),new AffectedPersonSkillService(allSkills), false, new ActivityDivider());
						scheduleResourceOptimizer.Optimize(layer.Period);
						
						//Shovling
						var skillStaffIntervalHolder = new SkillStaffIntervalHolder(relevantSkillStaffPeriods.Select(s =>
						{
							IResourceCalculationPeriod period;
							s.Value.TryGetValue(layer.Period, out period);
							return new {s.Key, period};
						}).ToDictionary(k => k.Key, v => v.period));
						var cascadingSkills = new CascadingSkills(allSkills);
						if (!cascadingSkills.Any()) continue;

						var orderedSkillGroups = _skillGroupPerActivityProvider.FetchOrdered(cascadingSkills,
							resourcesForShovel, activity, layer.Period);
						var allSkillGroups = orderedSkillGroups.AllSkillGroups();
						foreach (var skillGroupsWithSameIndex in orderedSkillGroups)
						{
							var state = _primarySkillOverstaff.AvailableSum(skillStaffIntervalHolder, allSkillGroups, skillGroupsWithSameIndex, layer.Period);
							_addResourcesToSubSkills.Execute(state, skillStaffIntervalHolder, skillGroupsWithSameIndex, layer.Period);
							_reducePrimarySkillResources.Execute(state, skillStaffIntervalHolder, layer.Period);
						}
					}
				}

				var mergedPeriod = personRequest.Request.Person.WorkflowControlSet.GetMergedAbsenceRequestOpenPeriod((IAbsenceRequest)personRequest.Request);
				var validators = _absenceRequestValidatorProvider.GetValidatorList(mergedPeriod);

				var staffingThresholdValidator = validators.OfType<StaffingThresholdValidator>().FirstOrDefault();
				if (staffingThresholdValidator != null)
				{
					var validatedRequest = staffingThresholdValidator.ValidateLight((IAbsenceRequest) personRequest.Request, skillStaffingIntervals);
					if (validatedRequest.IsValid)
					{
						var result = sendApproveCommand(personRequest.Id.GetValueOrDefault());
						if (result)
						{
							foreach (var combinationResource in skillCombinationResourcesForAgent)
							{
								_skillCombinationResourceRepository.PersistChange(combinationResource);
							}
						}
						else
						{
							sendDenyCommand(personRequest.Id.GetValueOrDefault(), validatedRequest.ValidationErrors);
						}
					}
					else
					{
						sendDenyCommand(personRequest.Id.GetValueOrDefault(), validatedRequest.ValidationErrors);
					}
				}
				else
				{
					logger.Error(Resources.DenyDueToTechnicalProblems + " Can not find any staffingThresholdValidator.");
					sendDenyCommand(personRequest.Id.GetValueOrDefault(), Resources.DenyDueToTechnicalProblems);
				}
			}
			catch (Exception exp)
			{
				logger.Error(Resources.DenyDueToTechnicalProblems + exp);
				sendDenyCommand(personRequest.Id.GetValueOrDefault(), Resources.DenyDueToTechnicalProblems);
			}
		}

		private static SkillCombinationResource distributeResourceSmartly(IEnumerable<SkillCombinationResource> combinationResources,
			SkillCombination skillCombination, ResourceLayer layer)
		{
			var skillCombinationResourceByAgentAndLayer =
				combinationResources.Single(
					x => x.SkillCombination.NonSequenceEquals(skillCombination.Skills.Select(y => y.Id.GetValueOrDefault()))
						 && (layer.Period.StartDateTime >= x.StartDateTime && layer.Period.EndDateTime <= x.EndDateTime) );

			var part = layer.Period.ElapsedTime().TotalMinutes / skillCombinationResourceByAgentAndLayer.GetTimeSpan().TotalMinutes;
			skillCombinationResourceByAgentAndLayer.Resource -= 1*part;
			return skillCombinationResourceByAgentAndLayer;
		}

		private static bool nonSkillActivityForWholeInterval(IActivity activity, ResourceLayer layer)
		{
			return !activity.RequiresSkill && !layer.FractionPeriod.HasValue;
		}

		private static void zeroDemandForIntervalsCoveredByNonSkillActivity(List<SkillStaffingInterval> skillStaffingIntervals, ResourceLayer layer)
		{
			var relevantStaffingIntervals = skillStaffingIntervals.Where(s => s.StartDateTime == layer.Period.StartDateTime);
			foreach (var relevantStaffingInterval in relevantStaffingIntervals)
			{
				relevantStaffingInterval.ForecastWithShrinkage = 0;
				relevantStaffingInterval.FStaff = 0;
			}
		}

		private bool sendDenyCommand(Guid personRequestId, string denyReason)
		{
			var command = new DenyRequestCommand
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
			var command = new ApproveRequestCommand
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