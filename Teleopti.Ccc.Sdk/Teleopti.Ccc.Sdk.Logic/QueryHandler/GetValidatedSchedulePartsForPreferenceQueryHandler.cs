using System;
using System.Collections.Generic;
using System.Globalization;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.Logic.Restrictions;

namespace Teleopti.Ccc.Sdk.Logic.QueryHandler
{
    public class GetValidatedSchedulePartsForPreferenceQueryHandler : IHandleQuery<GetValidatedSchedulePartsForPreferenceQueryDto, ICollection<ValidatedSchedulePartDto>>
	{
		private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IShiftCategoryRepository _shiftCategoryRepository;
		private readonly IActivityRepository _activityRepository;
		private readonly IPersonRepository _personRepository;
		private readonly IScheduleStorage _scheduleStorage;
        private readonly ICurrentScenario _scenarioRepository;
		private readonly IAssembler<IPreferenceDay, PreferenceRestrictionDto> _preferenceDayAssembler;
		private readonly IAssembler<IStudentAvailabilityDay, StudentAvailabilityDayDto> _studentAvailabilityDayAssembler;
    	private readonly IWorkShiftWorkTime _workShiftWorkTime;

        public GetValidatedSchedulePartsForPreferenceQueryHandler(ICurrentUnitOfWorkFactory unitOfWorkFactory, IShiftCategoryRepository shiftCategoryRepository, IActivityRepository activityRepository, IPersonRepository personRepository, IScheduleStorage scheduleStorage, ICurrentScenario scenarioRepository, IAssembler<IPreferenceDay, PreferenceRestrictionDto> preferenceDayAssembler, IAssembler<IStudentAvailabilityDay, StudentAvailabilityDayDto> studentAvailabilityDayAssembler, IWorkShiftWorkTime workShiftWorkTime)
        {
			_unitOfWorkFactory = unitOfWorkFactory;
			_shiftCategoryRepository = shiftCategoryRepository;
			_activityRepository = activityRepository;
			_personRepository = personRepository;
			_scheduleStorage = scheduleStorage;
			_scenarioRepository = scenarioRepository;
			_preferenceDayAssembler = preferenceDayAssembler;
			_studentAvailabilityDayAssembler = studentAvailabilityDayAssembler;
			_workShiftWorkTime = workShiftWorkTime;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public ICollection<ValidatedSchedulePartDto> Handle(GetValidatedSchedulePartsForPreferenceQueryDto query)
        {
        	return GetValidatedSchedulePartsOnSchedulePeriod(
        		query.Person,
        		query.DateInPeriod,
        		query.TimeZoneId);
        }

    	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dateInPeriod")]
		private ICollection<ValidatedSchedulePartDto> GetValidatedSchedulePartsOnSchedulePeriod(
			PersonDto personDto,
			DateOnlyDto dateInPeriod,
			string timeZoneInfoId)
		{
			IList<IPerson> personList = new List<IPerson>();
    		if (!personDto.Id.HasValue)
				return new List<ValidatedSchedulePartDto>();

			var dateOnlyInPeriod = dateInPeriod.ToDateOnly();
    		using (IUnitOfWork uow = _unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				using (uow.DisableFilter(QueryFilter.Deleted))
				{
					_shiftCategoryRepository.LoadAll();
					_activityRepository.LoadAll();
				}

				var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneInfoId);
				IPerson person = _personRepository.Load(personDto.Id.Value);

				if (person.PermissionInformation.Culture() == null && personDto.CultureLanguageId.HasValue)
					person.PermissionInformation.SetCulture(CultureInfo.GetCultureInfo((int)personDto.CultureLanguageId));

				IVirtualSchedulePeriod schedulePeriod = person.VirtualSchedulePeriodOrNext(dateOnlyInPeriod);
				if (!schedulePeriod.IsValid)
					return new List<ValidatedSchedulePartDto>();
				DateOnlyPeriod realSchedulePeriod = schedulePeriod.DateOnlyPeriod;

				TimeSpan periodTarget = schedulePeriod.PeriodTarget();
				int daysOffTarget = 0;

				var personPeriod = person.Period(dateOnlyInPeriod);
				if (personPeriod.PersonContract.Contract.EmploymentType ==
					EmploymentType.FixedStaffNormalWorkTime) daysOffTarget = schedulePeriod.DaysOff();
				// find out wich period to load
				var period = realSchedulePeriod.ToDateTimePeriod(timeZoneInfo);
				ISchedulerRangeToLoadCalculator rangeToLoadCalculator = new SchedulerRangeToLoadCalculator(period);
				period = rangeToLoadCalculator.SchedulerRangeToLoad(person);

				var clientsCulture = GetClientsCulture(personDto, person);

				var personTimeZone = person.PermissionInformation.DefaultTimeZone();
				if (TimeZoneHelper.ConvertFromUtc(period.StartDateTime, personTimeZone).DayOfWeek != clientsCulture.DateTimeFormat.FirstDayOfWeek)
				{
					DateTime dateTime = TimeZoneHelper.ConvertFromUtc(period.StartDateTime, personTimeZone);
					dateTime = DateHelper.GetFirstDateInWeek(dateTime, clientsCulture);
					period = new DateTimePeriod(TimeZoneHelper.ConvertToUtc(dateTime, personTimeZone), period.EndDateTime);
					period = period.ChangeEndTime(TimeSpan.FromDays(1));
				}

				period = period.ChangeEndTime(TimeSpan.FromTicks(-1));
				personList.Add(person);

				IScheduleDictionary scheduleDictionary = _scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(personList, new ScheduleDictionaryLoadOptions(true, false), period.ToDateOnlyPeriod(timeZoneInfo), _scenarioRepository.Current());


				var stateHolder = new SchedulingResultStateHolder();
					stateHolder.Schedules = scheduleDictionary;
					stateHolder.LoadedAgents = personList;

					var fullWeekOuterWeekPeriodCreator = new FullWeekOuterWeekPeriodCreator(schedulePeriod.DateOnlyPeriod, person);
					var scheduleMatrix = new ScheduleMatrixPro(stateHolder, fullWeekOuterWeekPeriodCreator, schedulePeriod);

					var periodTargetTimeCalculator = new SchedulePeriodTargetTimeCalculator();
					var schedulePeriodTargetBalanced = periodTargetTimeCalculator.TargetTime(scheduleMatrix);
					var tolerance = periodTargetTimeCalculator.TargetWithTolerance(scheduleMatrix);

					DateOnlyPeriod loadedPeriod = period.ToDateOnlyPeriod(timeZoneInfo);

					IPeriodScheduledAndRestrictionDaysOff periodScheduledAndRestrictionDaysOff = new PeriodScheduledAndRestrictionDaysOff();
					int numberOfDaysOff = periodScheduledAndRestrictionDaysOff.CalculatedDaysOff(scheduleMatrix, true,
																								 true, false);

					var restrictionValidator = new RestrictionsValidator(new IsEditablePredicate(),
																		 _preferenceDayAssembler,
																		 _studentAvailabilityDayAssembler,
																		 new MinMaxWorkTimeChecker(
																			 _workShiftWorkTime), clientsCulture,
																			 new PreferenceNightRestChecker(new NightlyRestFromPersonOnDayExtractor(person)));

					const bool useStudentAvailability = false;
					return restrictionValidator.ValidateSchedulePeriod(loadedPeriod,
												realSchedulePeriod, stateHolder,
												(int)periodTarget.TotalMinutes, (int)tolerance.StartTime.TotalMinutes, (int)tolerance.EndTime.TotalMinutes, daysOffTarget, person,
												schedulePeriod.MustHavePreference, (int)schedulePeriodTargetBalanced.TotalMinutes, (int)schedulePeriod.BalanceIn.TotalMinutes,
												(int)schedulePeriod.Extra.TotalMinutes, (int)schedulePeriod.BalanceOut.TotalMinutes, numberOfDaysOff, schedulePeriod.Seasonality.Value, useStudentAvailability);
			}
		}

		private static CultureInfo GetClientsCulture(PersonDto personDto, IPerson person)
		{
			var clientsCulture = person.PermissionInformation.Culture();//Fallback, ...
			if (personDto.CultureLanguageId != null)
				clientsCulture = CultureInfo.GetCultureInfo((int)personDto.CultureLanguageId); //This is the correct culture
			return clientsCulture;
		}
    }
}
