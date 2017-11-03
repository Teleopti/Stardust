using System;
using System.Collections.Generic;
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
using Teleopti.Interfaces.Domain;

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
		ICurrentBusinessUnit currentBusinessUnit	)
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
		}
		
		public WaitlistDataHolder PreloadData()
		{
			var dataHolder = new WaitlistDataHolder();
			_contractRepository.LoadAll();
			_skillTypeRepository.LoadAll();
			_partTimePercentageRepository.LoadAll();
			_contractScheduleRepository.LoadAllAggregate();
			_activityRepository.LoadAll();
			dataHolder.Skills = _skillRepository.LoadAllSkills().ToList();
		
			_stardustJobFeedback.SendProgress("Done preloading the data");

			
			var validPeriod = new DateTimePeriod(_now.UtcDateTime().AddDays(-1), _now.UtcDateTime().AddHours(_absenceRequestSetting.ImmediatePeriodInHours));
			dataHolder.LoadSchedulesPeriodToCoverForMidnightShifts = validPeriod;
			var waitlistedRequestsIds = _personRequestRepository.GetWaitlistRequests(dataHolder.LoadSchedulesPeriodToCoverForMidnightShifts).ToList();

			var waitlistedRequests = _personRequestRepository.Find(waitlistedRequestsIds);

			waitlistedRequests =
				waitlistedRequests.Where(
					x =>
						x.Request.Period.StartDateTime >= validPeriod.StartDateTime &&
						x.Request.Period.EndDateTime <= validPeriod.EndDateTime).ToList();

			dataHolder.AllRequests = waitlistedRequests.ToList();
			if (!dataHolder.AllRequests.Any())
			{
				dataHolder.InitSuccess = false;
				var mesg =
					$"No waitlisted request found within the period from {dataHolder.LoadSchedulesPeriodToCoverForMidnightShifts.StartDateTime} to {dataHolder.LoadSchedulesPeriodToCoverForMidnightShifts.EndDateTime}";
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
			
			_personRepository.FindPeople(dataHolder.AllRequests.Select(x => x.Person.Id.GetValueOrDefault()).ToList());

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
			var skillIds = new HashSet<Guid>();
			foreach (var skillCombinationResource in dataHolder.CombinationResources)
			{
				foreach (var skillId in skillCombinationResource.SkillCombination)
				{
					skillIds.Add(skillId);
				}
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
			var personHashset = new HashSet<IPerson>();
			foreach (var wr in waitlistedRequests)
			{
				personHashset.Add(wr.Person);
			}
			var persons = personHashset.ToList();
			ExtractSkillForecastIntervals.GetLongestPeriod(dataHolder.Skills, inflatedPeriod);
			dataHolder.PersonsSchedules =
				_scheduleStorage.FindSchedulesForPersons(_currentScenario.Current(),
					new PersonProvider(persons) { DoLoadByPerson = true }, new ScheduleDictionaryLoadOptions(false, false), dataHolder.LoadSchedulesPeriodToCoverForMidnightShifts, persons, true);
			dataHolder.PersonAbsenceAccounts = _personAbsenceAccountRepository.FindByUsers(persons);
			dataHolder.BusinessRules = _personalAccountUpdate.FromScheduleDictionary(dataHolder.PersonAbsenceAccounts, dataHolder.PersonsSchedules);
			
			dataHolder.RequestApprovalService = new AbsenceRequestApprovalService(_currentScenario.Current(), dataHolder.PersonsSchedules,
				dataHolder.BusinessRules, _scheduleDayChangeCallback, _globalSettingDataRepository, _checkingPersonalAccountDaysProvider);
			dataHolder.InitSuccess = true;

			return dataHolder;
		}
	}
}