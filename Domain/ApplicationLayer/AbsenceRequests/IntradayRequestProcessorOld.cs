using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Cascading;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests
{
	public class IntradayRequestProcessorOld : IIntradayRequestProcessor
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(IntradayRequestProcessorOld));
		private readonly IActivityRepository _activityRepository;
		private readonly IAddResourcesToSubSkills _addResourcesToSubSkills;
		private readonly ICommandDispatcher _commandDispatcher;
		private readonly ICurrentScenario _currentScenario;
		private readonly IPersonSkillProvider _personSkillProvider;
		private readonly PrimarySkillOverstaff _primarySkillOverstaff;
		private readonly ReducePrimarySkillResources _reducePrimarySkillResources;
		private readonly IScheduleForecastSkillReadModelRepository _scheduleForecastSkillReadModelRepository;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly ISkillCombinationResourceRepository _skillCombinationResourceRepository;
		private readonly ISkillRepository _skillRepository;
		private readonly SkillCombinationResourceReadModelValidator _skillCombinationResourceReadModelValidator;
		private readonly SkillGroupPerActivityProvider _skillGroupPerActivityProvider;
		private readonly IAbsenceRequestValidatorProvider _absenceRequestValidatorProvider;

		public IntradayRequestProcessorOld(ICommandDispatcher commandDispatcher,
										   ISkillCombinationResourceRepository skillCombinationResourceRepository, IPersonSkillProvider personSkillProvider,
										   IScheduleStorage scheduleStorage, ICurrentScenario currentScenario, IScheduleForecastSkillReadModelRepository scheduleForecastSkillReadModelRepository,
										   ISkillRepository skillRepository, IActivityRepository activityRepository, IAddResourcesToSubSkills addResourcesToSubSkills,
										   ReducePrimarySkillResources reducePrimarySkillResources, PrimarySkillOverstaff primarySkillOverstaff,
										   SkillGroupPerActivityProvider skillGroupPerActivityProvider, IAbsenceRequestValidatorProvider absenceRequestValidatorProvider,
										   SkillCombinationResourceReadModelValidator skillCombinationResourceReadModelValidator)
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
			_skillGroupPerActivityProvider = skillGroupPerActivityProvider;
			_absenceRequestValidatorProvider = absenceRequestValidatorProvider;
			_skillCombinationResourceReadModelValidator = skillCombinationResourceReadModelValidator;
		}

		public void Process(IPersonRequest personRequest, DateTime startTime)
		{
			try
			{
				if (!_skillCombinationResourceReadModelValidator.Validate())
				{
					logger.Warn(Resources.DenyReasonTechnicalIssues + "Read model is not up to date");
					sendDenyCommand(personRequest.Id.GetValueOrDefault(), Resources.DenyReasonTechnicalIssues);
					return;
				}

				//what if the agent changes personPeriod in the middle of the request period?
				//what if the request is 8:00-8:05, only a third of a resource should be removed

				var combinationResources = _skillCombinationResourceRepository.LoadSkillCombinationResources(personRequest.Request.Period).ToArray();
				if (!combinationResources.Any())
				{
					logger.Warn(Resources.DenyReasonTechnicalIssues + " Can not find any skillcombinations.");
					sendDenyCommand(personRequest.Id.GetValueOrDefault(), Resources.DenyReasonTechnicalIssues);
					return;
				}

				var schedules = _scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(personRequest.Person, new ScheduleDictionaryLoadOptions(false, false), personRequest.Request.Period, _currentScenario.Current())[personRequest.Person];

				//night shift?
				var dateOnlyPeriod = personRequest.Request.Period.ToDateOnlyPeriod(personRequest.Person.PermissionInformation.DefaultTimeZone());

				var scheduleDays = schedules.ScheduledDayCollection(dateOnlyPeriod);
				var allSkills = _skillRepository.LoadAll();
				var skillStaffingIntervals = _scheduleForecastSkillReadModelRepository.GetBySkills(allSkills.Select(x => x.Id.GetValueOrDefault()).ToArray(), personRequest.Request.Period.StartDateTime, personRequest.Request.Period.EndDateTime).ToList();
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
									s => (IResourceCalculationPeriod)s)));
				var resourcesForCalculation = new ResourcesExtractorCalculation(combinationResources, allSkills, skillInterval);
				var resourcesForShovel = new ResourcesExtractorShovel(combinationResources, allSkills, skillInterval);

				var mergedPeriod = personRequest.Request.Person.WorkflowControlSet.GetMergedAbsenceRequestOpenPeriod((IAbsenceRequest)personRequest.Request);
				var validators = _absenceRequestValidatorProvider.GetValidatorList(mergedPeriod);

				if (validators.Any(x => x.GetType() == typeof(StaffingThresholdWithShrinkageValidator)))
					foreach (var skillStaffingInterval in skillStaffingIntervals)
					{
						skillStaffingInterval.FStaff = skillStaffingInterval.ForecastWithShrinkage;
					}
				//this looks strange but is how it works. Pending = no autogrant, Grant = autogrant
				var autoGrant = mergedPeriod.AbsenceRequestProcess.GetType() != typeof(PendingAbsenceRequest);

				var skillCombinationResourcesForAgent = new List<SkillCombinationResource>();
				var earliestProjectionStartDateTime = DateTime.MaxValue;
				var latestProjectionEndDateTime = DateTime.MinValue;
				foreach (var day in scheduleDays)
				{
					var projection = day.ProjectionService().CreateProjection().FilterLayers(personRequest.Request.Period);
					var layers = projection.ToResourceLayers(skillInterval).ToList();

					if (!layers.Any())
					{
						continue;
					}

					if (projection.OriginalProjectionPeriod.Value.StartDateTime < earliestProjectionStartDateTime)
					{
						earliestProjectionStartDateTime = projection.OriginalProjectionPeriod.Value.StartDateTime;
					}
					if (projection.OriginalProjectionPeriod.Value.EndDateTime > latestProjectionEndDateTime)
					{
						latestProjectionEndDateTime = projection.OriginalProjectionPeriod.Value.EndDateTime;
					}

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

						var scheduleResourceOptimizer = new ScheduleResourceOptimizer(resourcesForCalculation, new SlimSkillResourceCalculationPeriodWrapper(relevantSkillStaffPeriods), new AffectedPersonSkillService(allSkills), false, new ActivityDivider());
						scheduleResourceOptimizer.Optimize(layer.Period);

						//Shovling
						var skillStaffIntervalHolder = new SkillStaffIntervalHolder(relevantSkillStaffPeriods.Select(s =>
						{
							IResourceCalculationPeriod period;
							s.Value.TryGetValue(layer.Period, out period);
							return new { s.Key, period };
						}).ToDictionary(k => k.Key, v => v.period));
						var cascadingSkills = new CascadingSkills(allSkills);
						if (!cascadingSkills.Any()) continue;

						var orderedSkillGroups = _skillGroupPerActivityProvider.FetchOrdered(cascadingSkills,
							resourcesForShovel, activity, layer.Period);
						var allSkillGroups = orderedSkillGroups.AllSkillGroups();
						foreach (var skillGroupsWithSameIndex in orderedSkillGroups)
						{
							var state = _primarySkillOverstaff.AvailableSum(skillStaffIntervalHolder, allSkillGroups, skillGroupsWithSameIndex, layer.Period);
							_addResourcesToSubSkills.Execute(state, skillStaffIntervalHolder, skillGroupsWithSameIndex, layer.Period, new NoShovelingCallback());
							_reducePrimarySkillResources.Execute(state, skillStaffIntervalHolder, layer.Period, null, new NoShovelingCallback());
						}
					}
				}

				var staffingThresholdValidator = validators.OfType<StaffingThresholdValidator>().FirstOrDefault();
				if (staffingThresholdValidator != null)
				{
					var validatedRequest = staffingThresholdValidator.ValidateLight((IAbsenceRequest) personRequest.Request, skillStaffingIntervals.Where(x => x.StartDateTime >= earliestProjectionStartDateTime && x.StartDateTime < latestProjectionEndDateTime));
					if (validatedRequest.IsValid)
					{
						if (!autoGrant) return;
						var result = sendApproveCommand(personRequest.Id.GetValueOrDefault());
						if (result)
						{
							_skillCombinationResourceRepository.PersistChanges(skillCombinationResourcesForAgent);

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
					logger.Error(Resources.DenyReasonTechnicalIssues + " Can not find any staffingThresholdValidator.");
					sendDenyCommand(personRequest.Id.GetValueOrDefault(), Resources.DenyReasonTechnicalIssues);
				}
			}
			catch (Exception exp)
			{
				logger.Error(Resources.DenyReasonTechnicalIssues + exp);
				sendDenyCommand(personRequest.Id.GetValueOrDefault(), Resources.DenyReasonTechnicalIssues);
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
			var skillCombinationChange = new SkillCombinationResource
			{
				StartDateTime = skillCombinationResourceByAgentAndLayer.StartDateTime,
				EndDateTime = skillCombinationResourceByAgentAndLayer.EndDateTime,
				SkillCombination = skillCombinationResourceByAgentAndLayer.SkillCombination,
				Resource = -1 * part
			};
			return skillCombinationChange;
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
				relevantStaffingInterval.FStaff = 0;
			}
		}

		private void sendDenyCommand(Guid personRequestId, string denyReason)
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