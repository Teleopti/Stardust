﻿using System;
using System.Collections.Generic;
using System.Globalization;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.Logic.Restrictions;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.Logic.QueryHandler
{
    public class GetValidatedSchedulePartsForStudentAvailabilityQueryHandler : IHandleQuery<GetValidatedSchedulePartsForStudentAvailabilityQueryDto, ICollection<ValidatedSchedulePartDto>>
    {
    	private readonly IUnitOfWorkFactory _unitOfWorkFactory;
    	private readonly IShiftCategoryRepository _shiftCategoryRepository;
    	private readonly IActivityRepository _activityRepository;
    	private readonly IPersonRepository _personRepository;
    	private readonly IScheduleRepository _scheduleRepository;
    	private readonly IScenarioProvider _scenarioProvider;
    	private readonly IAssembler<IPreferenceDay, PreferenceRestrictionDto> _preferenceDayAssembler;
    	private readonly IAssembler<IStudentAvailabilityDay, StudentAvailabilityDayDto> _studentAvailabilityDayAssembler;
    	private readonly IWorkShiftWorkTime _workShiftWorkTime;

    	public GetValidatedSchedulePartsForStudentAvailabilityQueryHandler(IUnitOfWorkFactory unitOfWorkFactory, IShiftCategoryRepository shiftCategoryRepository, IActivityRepository activityRepository, IPersonRepository personRepository, IScheduleRepository scheduleRepository, IScenarioProvider scenarioProvider, IAssembler<IPreferenceDay, PreferenceRestrictionDto> preferenceDayAssembler, IAssembler<IStudentAvailabilityDay,StudentAvailabilityDayDto> studentAvailabilityDayAssembler, IWorkShiftWorkTime workShiftWorkTime)
        {
        	_unitOfWorkFactory = unitOfWorkFactory;
        	_shiftCategoryRepository = shiftCategoryRepository;
    		_activityRepository = activityRepository;
    		_personRepository = personRepository;
    		_scheduleRepository = scheduleRepository;
    		_scenarioProvider = scenarioProvider;
    		_preferenceDayAssembler = preferenceDayAssembler;
    		_studentAvailabilityDayAssembler = studentAvailabilityDayAssembler;
    		_workShiftWorkTime = workShiftWorkTime;
        }

    	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public ICollection<ValidatedSchedulePartDto> Handle(GetValidatedSchedulePartsForStudentAvailabilityQueryDto query)
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
			IVirtualSchedulePeriod schedulePeriod;
			if (!personDto.Id.HasValue)
				return new List<ValidatedSchedulePartDto>();

			var dateOnlyInPeriod = new DateOnly(dateInPeriod.DateTime);
			IPerson person;
			using (IUnitOfWork uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				using (uow.DisableFilter(QueryFilter.Deleted))
				{
					_shiftCategoryRepository.LoadAll();
					_activityRepository.LoadAll();
				}

				var timeZoneInfo = new CccTimeZoneInfo(TimeZoneInfo.FindSystemTimeZoneById(timeZoneInfoId));
				person = _personRepository.Load(personDto.Id.Value);

				if (person.PermissionInformation.Culture() == null && personDto.CultureLanguageId.HasValue)
					person.PermissionInformation.SetCulture(CultureInfo.GetCultureInfo((int)personDto.CultureLanguageId));

				schedulePeriod = person.VirtualSchedulePeriodOrNext(dateOnlyInPeriod);
				if (!schedulePeriod.IsValid)
					return new List<ValidatedSchedulePartDto>();
				DateOnlyPeriod realSchedulePeriod = schedulePeriod.DateOnlyPeriod;

				TimeSpan periodTarget = schedulePeriod.PeriodTarget();
				int daysOffTarget = 0;

				var personPeriod = person.Period(dateOnlyInPeriod);
				if (personPeriod.PersonContract.Contract.EmploymentType ==
					EmploymentType.FixedStaffNormalWorkTime) daysOffTarget = schedulePeriod.DaysOff();
				// find out wich period to load
				DateTimePeriod period = realSchedulePeriod.ToDateTimePeriod(timeZoneInfo);
				ISchedulerRangeToLoadCalculator rangeToLoadCalculator = new SchedulerRangeToLoadCalculator(period) { JusticeValue = 0 };
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

				IScheduleDictionary scheduleDictionary = _scheduleRepository.FindSchedulesOnlyInGivenPeriod(new PersonProvider(personList), new ScheduleDictionaryLoadOptions(true, false), period, _scenarioProvider.DefaultScenario());
				//rk don't know if I break stuff here...
				//scheduleDictionary.SetTimeZone(timeZoneInfo);


				using (ISchedulingResultStateHolder stateHolder = new SchedulingResultStateHolder())
				{
					stateHolder.Schedules = scheduleDictionary;
					stateHolder.PersonsInOrganization = personList;

					IFullWeekOuterWeekPeriodCreator fullWeekOuterWeekPeriodCreator =
						new FullWeekOuterWeekPeriodCreator(schedulePeriod.DateOnlyPeriod, person);
					IScheduleMatrixPro scheduleMatrix = new ScheduleMatrixPro(stateHolder, fullWeekOuterWeekPeriodCreator, schedulePeriod);

					// todo: tamasb use the following instead
					//IScheduleTargetTimeCalculator periodTargetTimeCalculator = new ScheduleTargetTimeCalculator();

					ISchedulePeriodTargetTimeCalculator periodTargetTimeCalculator =
						new SchedulePeriodTargetTimeCalculator();
					var schedulePeriodTargetBalanced =
						periodTargetTimeCalculator.TargetTime(scheduleMatrix);
					var tolerance = periodTargetTimeCalculator.TargetWithTolerance(scheduleMatrix);

					DateOnlyPeriod loadedPeriod = period.ToDateOnlyPeriod(timeZoneInfo);

					IPeriodScheduledAndRestrictionDaysOff periodScheduledAndRestrictionDaysOff =
						new PeriodScheduledAndRestrictionDaysOff();
					int numberOfDaysOff = periodScheduledAndRestrictionDaysOff.CalculatedDaysOff(scheduleMatrix, true,
																								 true, false);

					var restrictionValidator = new RestrictionsValidator(new IsEditablePredicate(),
																		 _preferenceDayAssembler,
																		 _studentAvailabilityDayAssembler,
																		 new MinMaxWorkTimeChecker(
																			 _workShiftWorkTime), clientsCulture,
																			 new PreferenceNightRestChecker(new NightlyRestFromPersonOnDayExtractor(person)));

					const bool useStudentAvailability = true;
					return restrictionValidator.ValidateSchedulePeriod(loadedPeriod,
												realSchedulePeriod, stateHolder,
												(int)periodTarget.TotalMinutes, (int)tolerance.StartTime.TotalMinutes, (int)tolerance.EndTime.TotalMinutes, daysOffTarget, person,
												schedulePeriod.MustHavePreference, (int)schedulePeriodTargetBalanced.TotalMinutes, (int)schedulePeriod.BalanceIn.TotalMinutes,
												(int)schedulePeriod.Extra.TotalMinutes, (int)schedulePeriod.BalanceOut.TotalMinutes, numberOfDaysOff, schedulePeriod.Seasonality.Value, useStudentAvailability);
				}
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
