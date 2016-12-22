using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
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

		public IntradayCascadingRequestProcessor(ICommandDispatcher commandDispatcher, 
			ISkillCombinationResourceRepository skillCombinationResourceRepository, IPersonSkillProvider personSkillProvider, 
			IScheduleStorage scheduleStorage, ICurrentScenario currentScenario, IScheduleForecastSkillReadModelRepository scheduleForecastSkillReadModelRepository)
		{
			_commandDispatcher = commandDispatcher;
			_skillCombinationResourceRepository = skillCombinationResourceRepository;
			_personSkillProvider = personSkillProvider;
			_scheduleStorage = scheduleStorage;
			_currentScenario = currentScenario;
			_scheduleForecastSkillReadModelRepository = scheduleForecastSkillReadModelRepository;
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
			var skillStaffingIntervals = new List<SkillStaffingInterval>();
			var skillInterval = (int)combinationResources.FirstOrDefault().EndDateTime.Subtract(combinationResources.FirstOrDefault().StartDateTime).TotalMinutes;
			foreach (var day in scheduleDays)
			{
				var projection = day.ProjectionService().CreateProjection().FilterLayers(personRequest.Request.Period);
				
				var layers = projection.ToResourceLayers(skillInterval);
				
				foreach (var layer in layers)
				{
					var skillCombination = _personSkillProvider.SkillsOnPersonDate(personRequest.Person, dateOnlyPeriod.StartDate).ForActivity(layer.PayloadId);
					if (!skillCombination.Skills.Any()) continue;

					combinationResources.Single(x => x.SkillCombination.NonSequenceEquals(skillCombination.Skills.Select(y => y.Id.GetValueOrDefault()))
													 && x.StartDateTime == layer.Period.StartDateTime).Resource -= 1;
					foreach (var skill in skillCombination.Skills)
					{
						var skillStaffIntervals = _scheduleForecastSkillReadModelRepository.GetBySkill(skill.Id.GetValueOrDefault(), layer.Period.StartDateTime, layer.Period.EndDateTime);
						if (skillStaffIntervals != null)
						{
							foreach (var interval in skillStaffIntervals)
							{
								var resources = combinationResources.Where(x => x.SkillCombination.Contains(skill.Id.GetValueOrDefault()) && x.StartDateTime == interval.StartDateTime)
									.Sum(skillCombinationResource => skillCombinationResource.Resource / skillCombinationResource.SkillCombination.Count());
								interval.StaffingLevel = resources;
								skillStaffingIntervals.Add(interval);
							}
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
