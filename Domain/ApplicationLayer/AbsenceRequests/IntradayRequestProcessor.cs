using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.Intraday;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests
{
	public class IntradayRequestProcessor : IIntradayRequestProcessor
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(IntradayRequestProcessor));
		private readonly ICommandDispatcher _commandDispatcher;
		private readonly ICurrentScenario _currentScenario;
		private readonly IPersonSkillProvider _personSkillProvider;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly ISkillCombinationResourceRepository _skillCombinationResourceRepository;
		private readonly ISkillRepository _skillRepository;
		private readonly ISkillCombinationResourceReadModelValidator _skillCombinationResourceReadModelValidator;
		private readonly IAbsenceRequestValidatorProvider _absenceRequestValidatorProvider;
		private readonly ISkillStaffingIntervalProvider _skillStaffingIntervalProvider;

		public IntradayRequestProcessor(ICommandDispatcher commandDispatcher,
												 ISkillCombinationResourceRepository skillCombinationResourceRepository, IPersonSkillProvider personSkillProvider,
												 IScheduleStorage scheduleStorage, ICurrentScenario currentScenario,
												 ISkillRepository skillRepository, ISkillCombinationResourceReadModelValidator skillCombinationResourceReadModelValidator, 
												 IAbsenceRequestValidatorProvider absenceRequestValidatorProvider, ISkillStaffingIntervalProvider skillStaffingIntervalProvider)
		{
			_commandDispatcher = commandDispatcher;
			_skillCombinationResourceRepository = skillCombinationResourceRepository;
			_personSkillProvider = personSkillProvider;
			_scheduleStorage = scheduleStorage;
			_currentScenario = currentScenario;
			_skillRepository = skillRepository;
			_skillCombinationResourceReadModelValidator = skillCombinationResourceReadModelValidator;
			_absenceRequestValidatorProvider = absenceRequestValidatorProvider;
			_skillStaffingIntervalProvider = skillStaffingIntervalProvider;
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

				var combinationResources = _skillCombinationResourceRepository.LoadSkillCombinationResources(personRequest.Request.Period).ToArray();
				if (!combinationResources.Any())
				{
					logger.Warn(Resources.DenyDueToTechnicalProblems + " Can not find any skillcombinations.");
					sendDenyCommand(personRequest.Id.GetValueOrDefault(), Resources.DenyDueToTechnicalProblems);
					return;
				}

				var schedules = _scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(personRequest.Person, new ScheduleDictionaryLoadOptions(false, false), personRequest.Request.Period, _currentScenario.Current())[personRequest.Person];

				var dateOnlyPeriod = personRequest.Request.Period.ToDateOnlyPeriod(personRequest.Person.PermissionInformation.DefaultTimeZone());

				var scheduleDays = schedules.ScheduledDayCollection(dateOnlyPeriod);
				var allSkills = _skillRepository.LoadAll();

				var skillIds = new HashSet<Guid>();
				foreach (var skillCombinationResource in combinationResources)
				{
					foreach (var skillId in skillCombinationResource.SkillCombination)
					{
						skillIds.Add(skillId);
					}
				}
				var skillInterval = allSkills.Where(x => skillIds.Contains(x.Id.Value)).Min(x => x.DefaultResolution);

				var deltaResourcesForAgent = new List<SkillCombinationResource>();
				foreach (var day in scheduleDays)
				{
					var projection = day.ProjectionService().CreateProjection().FilterLayers(personRequest.Request.Period);

					var layers = projection.ToResourceLayers(skillInterval);
					if (!layers.Any())
					{
						logger.Info($"Absence request {personRequest.Id.GetValueOrDefault()} is approved as the agent is not scheduled.");
						sendApproveCommand(personRequest.Id.GetValueOrDefault());
						return;
					}

					deltaResourcesForAgent.AddRange(
						from layer in layers let skillCombination = _personSkillProvider.SkillsOnPersonDate(personRequest.Person, dateOnlyPeriod.StartDate)
							.ForActivity(layer.PayloadId)
						where skillCombination.Skills.Any()
						select distributeResourceSmartly(combinationResources, skillCombination, layer));
				}

				var skillStaffingIntervals = _skillStaffingIntervalProvider.GetSkillStaffIntervalsAllSkills(personRequest.Request.Period, combinationResources.ToList());
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
							foreach (var delta in deltaResourcesForAgent)
							{
								_skillCombinationResourceRepository.PersistChange(delta);
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
			var skillCombinationChange = new SkillCombinationResource
			{
				StartDateTime = skillCombinationResourceByAgentAndLayer.StartDateTime,
				EndDateTime = skillCombinationResourceByAgentAndLayer.EndDateTime,
				SkillCombination = skillCombinationResourceByAgentAndLayer.SkillCombination,
				Resource = -1 * part
			};
			return skillCombinationChange;
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