using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.AbsenceWaitlisting;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.Intraday;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests
{
	public class RequestProcessor : IRequestProcessor
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(RequestProcessor));
		private readonly ICommandDispatcher _commandDispatcher;
		private readonly ICurrentScenario _currentScenario;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly ISkillCombinationResourceRepository _skillCombinationResourceRepository;
		private readonly ISkillRepository _skillRepository;
		private readonly SkillCombinationResourceReadModelValidator _skillCombinationResourceReadModelValidator;
		private readonly IAbsenceRequestValidatorProvider _absenceRequestValidatorProvider;
		private readonly SkillStaffingIntervalProvider _skillStaffingIntervalProvider;
		private readonly IActivityRepository _activityRepository;
		private readonly IPersonRequestRepository _personRequestRepository;
		private readonly IAbsenceRequestSetting _absenceRequestSetting;
		private readonly IPersonSkillProvider _personSkillProvider;
		private readonly ArrangeRequestsByProcessOrder _arrangeRequestsByProcessOrder;
		private readonly ExtractSkillForecastIntervals _extractSkillForecastIntervals;
		private readonly IResourceCalculation _resourceCalculation;

		public RequestProcessor(ICommandDispatcher commandDispatcher,
			ISkillCombinationResourceRepository skillCombinationResourceRepository,
			IScheduleStorage scheduleStorage, ICurrentScenario currentScenario,
			ISkillRepository skillRepository,
			SkillCombinationResourceReadModelValidator skillCombinationResourceReadModelValidator,
			IAbsenceRequestValidatorProvider absenceRequestValidatorProvider,
			SkillStaffingIntervalProvider skillStaffingIntervalProvider,
			IActivityRepository activityRepository, IPersonRequestRepository personRequestRepository,
			IAbsenceRequestSetting absenceRequestSetting, IPersonSkillProvider personSkillProvider,
			ArrangeRequestsByProcessOrder arrangeRequestsByProcessOrder,
			ExtractSkillForecastIntervals extractSkillForecastIntervals, IResourceCalculation resourceCalculation)
		{
			_commandDispatcher = commandDispatcher;
			_skillCombinationResourceRepository = skillCombinationResourceRepository;
			_scheduleStorage = scheduleStorage;
			_currentScenario = currentScenario;
			_skillRepository = skillRepository;
			_skillCombinationResourceReadModelValidator = skillCombinationResourceReadModelValidator;
			_absenceRequestValidatorProvider = absenceRequestValidatorProvider;
			_skillStaffingIntervalProvider = skillStaffingIntervalProvider;
			_activityRepository = activityRepository;
			_personRequestRepository = personRequestRepository;
			_absenceRequestSetting = absenceRequestSetting;
			_personSkillProvider = personSkillProvider;
			_arrangeRequestsByProcessOrder = arrangeRequestsByProcessOrder;
			_extractSkillForecastIntervals = extractSkillForecastIntervals;
			_resourceCalculation = resourceCalculation;
		}

		public void Process(IPersonRequest personRequest, DateTime startTime)
		{
			try
			{
				if (!_skillCombinationResourceReadModelValidator.Validate())
				{
					logger.Error(Resources.DenyReasonTechnicalIssues + "Read model is not up to date");
					sendDenyCommand(personRequest, Resources.DenyReasonTechnicalIssues);
					return;
				}
				var loadSchedulesPeriodToCoverForMidnightShifts = personRequest.Request.Period.ChangeStartTime(TimeSpan.FromDays(-1));
				var waitlistedRequestsIds = _personRequestRepository.GetWaitlistRequests(loadSchedulesPeriodToCoverForMidnightShifts);
				var waitlistedRequests = _personRequestRepository.Find(waitlistedRequestsIds);
				var validPeriod = new DateTimePeriod(startTime, startTime.AddHours(_absenceRequestSetting.ImmediatePeriodInHours));
				waitlistedRequests =
					waitlistedRequests.Where(
						x =>
							x.Request.Period.StartDateTime >= validPeriod.StartDateTime &&
							x.Request.Period.EndDateTime <= validPeriod.EndDateTime).ToList();
				waitlistedRequests.Add(personRequest);
				var allRequests = _arrangeRequestsByProcessOrder.GetRequestsSortedBySeniority(waitlistedRequests).ToList();
				allRequests.AddRange(_arrangeRequestsByProcessOrder.GetRequestsSortedByDate(waitlistedRequests));

				var inflatedPeriod = new DateTimePeriod(allRequests.Min(x => x.Request.Period.StartDateTime), allRequests.Max(x => x.Request.Period.EndDateTime));

				var combinationResources = _skillCombinationResourceRepository.LoadSkillCombinationResources(inflatedPeriod).ToList();
				if (!combinationResources.Any())
				{
					logger.Error(Resources.DenyReasonTechnicalIssues + " Can not find any skillcombinations.");
					sendDenyCommand(personRequest, Resources.DenyReasonTechnicalIssues);
					return;
				}

				_activityRepository.LoadAll();
				var skills = _skillRepository.LoadAll().ToList();
				var skillIds = new HashSet<Guid>();
				foreach (var skillCombinationResource in combinationResources)
				{
					foreach (var skillId in skillCombinationResource.SkillCombination)
					{
						skillIds.Add(skillId);
					}
				}
				var skillInterval = skills.Where(x => skillIds.Contains(x.Id.GetValueOrDefault())).Min(x => x.DefaultResolution);

				var skillStaffingIntervals = _extractSkillForecastIntervals.GetBySkills(skills, inflatedPeriod, true).ToList();
				skillStaffingIntervals.ForEach(s => s.StaffingLevel = 0);

				var relevantSkillStaffPeriods =
					skillStaffingIntervals.GroupBy(s => skills.First(a => a.Id.GetValueOrDefault() == s.SkillId))
						.ToDictionary(k => k.Key,
							v =>
								(IResourceCalculationPeriodDictionary)
								new ResourceCalculationPeriodDictionary(v.ToDictionary(d => d.DateTimePeriod,
									s => (IResourceCalculationPeriod)s)));
				var resCalcData = new ResourceCalculationData(skills, new SlimSkillResourceCalculationPeriodWrapper(relevantSkillStaffPeriods));
				//var dateOnlyPeriod = period.ToDateOnlyPeriod(TimeZoneInfo.Utc);
				var dateOnlyPeriodOne = ExtractSkillForecastIntervals.GetLongestPeriod(skills, inflatedPeriod);

				//using (getContext(combinationResources, skills, false))
				{
					
					//consider the exception and which requests to deny
					foreach (var pRequest in allRequests)
					{
						var requestPeriod = pRequest.Request.Period;
						var schedules = _scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(pRequest.Person, new ScheduleDictionaryLoadOptions(false, false), loadSchedulesPeriodToCoverForMidnightShifts, _currentScenario.Current())[pRequest.Person];

						var dateOnlyPeriod = loadSchedulesPeriodToCoverForMidnightShifts.ToDateOnlyPeriod(pRequest.Person.PermissionInformation.DefaultTimeZone());
						using (getContext(combinationResources, skills, false))
						{
							_resourceCalculation.ResourceCalculate(dateOnlyPeriodOne, resCalcData, () => getContext(combinationResources, skills, true));
						}

						var scheduleDays = schedules.ScheduledDayCollection(dateOnlyPeriod);

						var mergedPeriod = pRequest.Request.Person.WorkflowControlSet.GetMergedAbsenceRequestOpenPeriod((IAbsenceRequest)pRequest.Request);
						var validators = _absenceRequestValidatorProvider.GetValidatorList(mergedPeriod);

						var autoGrant = mergedPeriod.AbsenceRequestProcess.GetType() != typeof(PendingAbsenceRequest);

						var shiftPeriodList = new List<DateTimePeriod>();

						var scheduleDaysOnRequest = new List<IScheduleDay>();
						foreach (var day in scheduleDays)
						{

							var projection = day.ProjectionService().CreateProjection().FilterLayers(requestPeriod);

							var layers = projection.ToResourceLayers(skillInterval).ToList();
							if (!layers.Any())
							{
								continue;
							}
							scheduleDaysOnRequest.Add(day);
							shiftPeriodList.Add(new DateTimePeriod(projection.OriginalProjectionPeriod.Value.StartDateTime, projection.OriginalProjectionPeriod.Value.EndDateTime));
						}

						var staffingThresholdValidators = validators.OfType<StaffingThresholdValidator>().ToList();
						if (staffingThresholdValidators.Any())
						{
							var useShrinkage = staffingThresholdValidators.Any(x => x.GetType() == typeof(StaffingThresholdWithShrinkageValidator));
							//var skillStaffingIntervals = _skillStaffingIntervalProvider.GetSkillStaffIntervalsAllSkills(requestPeriod, combinationResources, useShrinkage);
							var skillStaffingIntervalsToValidate = new List<SkillStaffingInterval>();
							foreach (var projectionPeriod in shiftPeriodList)
							{
								skillStaffingIntervalsToValidate.AddRange(skillStaffingIntervals.Where(x => x.StartDateTime >= projectionPeriod.StartDateTime && x.StartDateTime < projectionPeriod.EndDateTime));
							}
							var validatedRequest = staffingThresholdValidators.FirstOrDefault().ValidateLight((IAbsenceRequest)pRequest.Request, skillStaffingIntervalsToValidate);
							if (validatedRequest.IsValid)
							{
								if (!autoGrant) return;
								var result = sendApproveCommand(pRequest);
								if (result)
								{
									var skillComb = _personSkillProvider.SkillsOnPersonDate(pRequest.Person, new DateOnly(pRequest.RequestedDate));
									var relevantCombinationResources =
										combinationResources.Where(x => x.SkillCombination.SequenceEqual(skillComb.Key));
									relevantCombinationResources.ForEach(relevantCombRes =>
									{
										if ((requestPeriod.StartDateTime >= relevantCombRes.StartDateTime && requestPeriod.StartDateTime < relevantCombRes.EndDateTime) ||
											(requestPeriod.EndDateTime < relevantCombRes.EndDateTime && requestPeriod.EndDateTime >= relevantCombRes.StartDateTime))
										{
											var rPeriod = new DateTimePeriod(relevantCombRes.StartDateTime, relevantCombRes.EndDateTime);
											var intersection = requestPeriod.Intersection(rPeriod).GetValueOrDefault().ElapsedTime().TotalMinutes;
										//need a test for that
										var resource = intersection / rPeriod.ElapsedTime().TotalMinutes;
											if (relevantCombRes.Resource - resource > 0)
												relevantCombRes.Resource -= resource;
											else
											//what will happen here
											relevantCombRes.Resource = 0;
										}
									});
								}
								else
								{
									sendDenyCommand(pRequest, validatedRequest.ValidationErrors);
								}
							}
							else
							{
								sendDenyCommand(pRequest, validatedRequest.ValidationErrors);
							}
						}
						else
						{
							logger.Error(Resources.DenyReasonTechnicalIssues + " Can not find any staffingThresholdValidator.");
							sendDenyCommand(pRequest, Resources.DenyReasonTechnicalIssues);
						}
					}

				}
			}
			catch (Exception exp)
			{
				logger.Error(Resources.DenyReasonTechnicalIssues + exp);
				sendDenyCommand(personRequest, Resources.DenyReasonTechnicalIssues);
			}
		}

		private void sendDenyCommand(IPersonRequest personRequest, string denyReason)
		{
			var command = new DenyRequestCommand
			{
				PersonRequestId = personRequest.Id.GetValueOrDefault(),
				DenyReason = denyReason
			};
			_commandDispatcher.Execute(command);

			if (command.ErrorMessages.Any())
			{
				logger.Warn(command.ErrorMessages);
			}
		}

		private bool sendApproveCommand(IPersonRequest personRequest)
		{
			var command = new ApproveRequestCommand
			{
				PersonRequestId = personRequest.Id.GetValueOrDefault(),
				IsAutoGrant = true
			};
			_commandDispatcher.Execute(command);

			if (command.ErrorMessages.Any())
			{
				logger.Warn(command.ErrorMessages);
			}

			return !command.ErrorMessages.Any();
		}

		private static IDisposable getContext(List<SkillCombinationResource> combinationResources, List<ISkill> skills, bool useAllSkills)
		{
			return new ResourceCalculationContext(new Lazy<IResourceCalculationDataContainerWithSingleOperation>(() => new ResourceCalculationDataContainerFromSkillCombinations(combinationResources, skills, useAllSkills)));
		}
	}

	public interface IRequestProcessor
	{
		void Process(IPersonRequest personRequest, DateTime startTime);
	}

	public interface IAbsenceRequestSetting
	{
		int ImmediatePeriodInHours { get; }
	}

	public class AbsenceRequestOneDaySetting : IAbsenceRequestSetting
	{
		public int ImmediatePeriodInHours => 25;
	}

}