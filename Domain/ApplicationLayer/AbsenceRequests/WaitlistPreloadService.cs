using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer.Intraday;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.WorkflowControl;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests
{
	public struct WaitlistDataHolder
	{
		public bool InitSuccess;
		public ResourceCalculationData ResCalcData;
		public IScheduleDictionary PersonsSchedules;
		public List<SkillCombinationResource> CombinationResources;
		public List<ISkill> Skills;
		public List<IPersonRequest> AllRequests;
		public DateTimePeriod LoadSchedulesPeriodToCoverForMidnightShifts;
		public IEnumerable<SkillStaffingInterval> SkillStaffingIntervals;
		public IRequestApprovalService RequestApprovalService;
		public INewBusinessRuleCollection BusinessRules;
		public IDictionary<IPerson, IPersonAccountCollection> PersonAbsenceAccounts;
	}

	public class WaitlistPreloadService
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(WaitlistPreloadService));

		private readonly ISkillCombinationResourceRepository _skillCombinationResourceRepository;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly ICurrentScenario _currentScenario;
		private readonly ISkillRepository _skillRepository;
		private readonly SkillCombinationResourceReadModelValidator _skillCombinationResourceReadModelValidator;
		private readonly IActivityRepository _activityRepository;
		private readonly IPersonRequestRepository _personRequestRepository;
		private readonly IAbsenceRequestSetting _absenceRequestSetting;
		private readonly ExtractSkillForecastIntervals _extractSkillForecastIntervals;
		private readonly INow _now;
		private readonly ISkillTypeRepository _skillTypeRepository;
		private readonly IPersonRepository _personRepository;
		private readonly IContractRepository _contractRepository;
		private readonly IPartTimePercentageRepository _partTimePercentageRepository;
		private readonly IContractScheduleRepository _contractScheduleRepository;
		private readonly IStardustJobFeedback _stardustJobFeedback;
		private readonly IPersonAbsenceAccountRepository _personAbsenceAccountRepository;
		private readonly IGlobalSettingDataRepository _globalSettingDataRepository;
		private readonly ICheckingPersonalAccountDaysProvider _checkingPersonalAccountDaysProvider;
		private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;
		private readonly IBusinessRulesForPersonalAccountUpdate _personalAccountUpdate;
		private readonly ICurrentBusinessUnit _currentBusinessUnit;
		private readonly IAbsenceRequestValidatorProvider _absenceRequestValidatorProvider;

		public WaitlistPreloadService(ISkillCombinationResourceRepository skillCombinationResourceRepository,
		IScheduleStorage scheduleStorage, ICurrentScenario currentScenario,
		ISkillRepository skillRepository,
		SkillCombinationResourceReadModelValidator skillCombinationResourceReadModelValidator,
		IActivityRepository activityRepository, IPersonRequestRepository personRequestRepository,
		IAbsenceRequestSetting absenceRequestSetting,
		ExtractSkillForecastIntervals extractSkillForecastIntervals,
		INow now,
		ISkillTypeRepository skillTypeRepository, IPersonRepository personRepository,
		IContractRepository contractRepository,
		IPartTimePercentageRepository partTimePercentageRepository,
		IContractScheduleRepository contractScheduleRepository,
		IStardustJobFeedback stardustJobFeedback,
		IPersonAbsenceAccountRepository personAbsenceAccountRepository,
		IGlobalSettingDataRepository globalSettingDataRepository,
		ICheckingPersonalAccountDaysProvider checkingPersonalAccountDaysProvider,
		IScheduleDayChangeCallback scheduleDayChangeCallback,
		IBusinessRulesForPersonalAccountUpdate personalAccountUpdate,
		ICurrentBusinessUnit currentBusinessUnit,
		IAbsenceRequestValidatorProvider absenceRequestValidatorProvider)
		{
			_skillCombinationResourceRepository = skillCombinationResourceRepository;
			_scheduleStorage = scheduleStorage;
			_currentScenario = currentScenario;
			_skillRepository = skillRepository;
			_skillCombinationResourceReadModelValidator = skillCombinationResourceReadModelValidator;
			_activityRepository = activityRepository;
			_personRequestRepository = personRequestRepository;
			_absenceRequestSetting = absenceRequestSetting;
			_extractSkillForecastIntervals = extractSkillForecastIntervals;
			_now = now;
			_skillTypeRepository = skillTypeRepository;
			_personRepository = personRepository;
			_contractRepository = contractRepository;
			_partTimePercentageRepository = partTimePercentageRepository;
			_contractScheduleRepository = contractScheduleRepository;
			_stardustJobFeedback = stardustJobFeedback;
			_personAbsenceAccountRepository = personAbsenceAccountRepository;
			_globalSettingDataRepository = globalSettingDataRepository;
			_checkingPersonalAccountDaysProvider = checkingPersonalAccountDaysProvider;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
			_personalAccountUpdate = personalAccountUpdate;
			_currentBusinessUnit = currentBusinessUnit;
			_absenceRequestValidatorProvider = absenceRequestValidatorProvider;
		}

		public WaitlistDataHolder PreloadData()
		{
			var stopWatch = new Stopwatch();
			stopWatch.Start();
			_stardustJobFeedback.SendProgress("Starting preloading data");
			var dataHolder = new WaitlistDataHolder();
			_contractRepository.LoadAll();
			_skillTypeRepository.LoadAll();
			_partTimePercentageRepository.LoadAll();
			_contractScheduleRepository.LoadAllAggregate();
			_activityRepository.LoadAll();
			dataHolder.Skills = _skillRepository.LoadAllSkills().ToList();
			
			var validPeriod = new DateTimePeriod(_now.UtcDateTime().AddDays(-1), _now.UtcDateTime().AddHours(_absenceRequestSetting.ImmediatePeriodInHours));
			dataHolder.LoadSchedulesPeriodToCoverForMidnightShifts = validPeriod;
			var waitlistedRequestsIds = _personRequestRepository.GetWaitlistRequests(dataHolder.LoadSchedulesPeriodToCoverForMidnightShifts).ToList();

			var waitlistedRequests = _personRequestRepository.Find(waitlistedRequestsIds);

			waitlistedRequests =
				waitlistedRequests.Where(
					x =>
						x.Request.Period.StartDateTime >= validPeriod.StartDateTime &&
						x.Request.Period.EndDateTime <= validPeriod.EndDateTime).ToList();

			_personRepository.FindPeople(waitlistedRequests.Select(x => x.Person.Id.GetValueOrDefault()).ToArray());
			dataHolder.AllRequests = waitlistedRequests.Where(isRequestUsingStaffingValidation).ToList();

			if (!dataHolder.AllRequests.Any())
			{
				dataHolder.InitSuccess = false;
				var mesg =
					$"No waitlisted request found with staffing check within the period from {dataHolder.LoadSchedulesPeriodToCoverForMidnightShifts.StartDateTime} to {dataHolder.LoadSchedulesPeriodToCoverForMidnightShifts.EndDateTime}";
				logger.Info(mesg);
				_stardustJobFeedback.SendProgress(mesg);
				return dataHolder;
			}

			if (!_skillCombinationResourceReadModelValidator.Validate())
			{
				logger.Error($"Read model is not up to date on Business Unit {_currentBusinessUnit.Current().Name}.");
				_stardustJobFeedback.SendProgress($"Read model is not up to date on Business Unit {_currentBusinessUnit.Current().Name}.");
				dataHolder.InitSuccess = false;
				return dataHolder;
			}



			var inflatedPeriod = new DateTimePeriod(dataHolder.AllRequests.Min(x => x.Request.Period.StartDateTime),
				dataHolder.AllRequests.Max(x => x.Request.Period.EndDateTime));

			dataHolder.CombinationResources = _skillCombinationResourceRepository.LoadSkillCombinationResources(inflatedPeriod).ToList();
			if (!dataHolder.CombinationResources.Any())
			{
				logger.Error(" Can not find any skillcombinations.");
				_stardustJobFeedback.SendProgress("Can not find any skillcombinations.");
				dataHolder.InitSuccess = false;
				return dataHolder;
			}
			
			dataHolder.SkillStaffingIntervals = _extractSkillForecastIntervals.GetBySkills(dataHolder.Skills, inflatedPeriod).ToList();
			dataHolder.SkillStaffingIntervals.ForEach(s => s.StaffingLevel = 0);

			var relevantSkillStaffPeriods =
				dataHolder.SkillStaffingIntervals.GroupBy(s => dataHolder.Skills.First(a => a.Id.GetValueOrDefault() == s.SkillId))
					.ToDictionary(k => k.Key,
						v =>
							(IResourceCalculationPeriodDictionary)
							new ResourceCalculationPeriodDictionary(v.ToDictionary(d => d.DateTimePeriod,
								s => (IResourceCalculationPeriod)s)));
			dataHolder.ResCalcData = new ResourceCalculationData(dataHolder.Skills, new SlimSkillResourceCalculationPeriodWrapper(relevantSkillStaffPeriods));
			
			var persons = waitlistedRequests.Select(wr => wr.Person).Distinct().ToArray();
			dataHolder.PersonsSchedules =
				_scheduleStorage.FindSchedulesForPersons(_currentScenario.Current(),
					persons, new ScheduleDictionaryLoadOptions(false, false), dataHolder.LoadSchedulesPeriodToCoverForMidnightShifts, persons, true);
			dataHolder.PersonAbsenceAccounts = _personAbsenceAccountRepository.FindByUsers(persons);
			dataHolder.BusinessRules = _personalAccountUpdate.FromScheduleDictionary(dataHolder.PersonAbsenceAccounts, dataHolder.PersonsSchedules);

			dataHolder.RequestApprovalService = new AbsenceRequestApprovalService(_currentScenario.Current(), dataHolder.PersonsSchedules,
				dataHolder.BusinessRules, _scheduleDayChangeCallback, _globalSettingDataRepository, _checkingPersonalAccountDaysProvider);
			dataHolder.InitSuccess = true;

			_stardustJobFeedback.SendProgress($"Done preloading data. {stopWatch.Elapsed.TotalSeconds:F} s elapsed.");
			
			return dataHolder;
		}

		private bool isRequestUsingStaffingValidation(IPersonRequest pRequest)
		{
			var wfc = pRequest.Request.Person.WorkflowControlSet;
			if (wfc == null) return false;
			var mergedPeriod = wfc.GetMergedAbsenceRequestOpenPeriod((IAbsenceRequest)pRequest.Request);
			var validators = _absenceRequestValidatorProvider.GetValidatorList(mergedPeriod);
			var useStaffing = validators.OfType<StaffingThresholdValidator>().Any();

			return useStaffing;
		}
	}
}