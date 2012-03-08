using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.Infrastructure.Persisters;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.Logic.Restrictions;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.WcfService.Factory
{
    public class ScheduleFactory
    {
        private readonly IScheduleRepository _scheduleRepository;
        private readonly IScheduleDictionarySaver _scheduleDictionarySaver;
        private readonly IShiftCategoryRepository _shiftCategoryRepository;
        private readonly IAssembler<IPerson, PersonDto> _personAssembler;
        private readonly IRuleSetProjectionService _ruleSetProjectionService;
        private readonly IAssembler<IScheduleDay, SchedulePartDto> _scheduleDayAssembler;
        private readonly IActivityRepository _activityRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IScheduleDataAssembler<IPreferenceDay, PreferenceRestrictionDto> _preferenceDayAssembler;
        private readonly IScheduleDataAssembler<IStudentAvailabilityDay,StudentAvailabilityDayDto> _studentAvailabilityDayAssembler;
        private readonly IAssembler<DateTimePeriod, DateTimePeriodDto> _dateTimePeriodAssembler;
        private readonly IScenarioProvider _scenarioProvider;

        public ScheduleFactory(IScheduleRepository scheduleRepository, IScheduleDictionarySaver scheduleDictionarySaver, IShiftCategoryRepository shiftCategoryRepository, IAssembler<IPerson, PersonDto> personAssembler, IRuleSetProjectionService ruleSetProjectionService, IActivityRepository activityRepository, IPersonRepository personRepository, IAssembler<IScheduleDay, SchedulePartDto> scheduleDayAssembler, IScheduleDataAssembler<IPreferenceDay, PreferenceRestrictionDto> preferenceDayAssembler, IScheduleDataAssembler<IStudentAvailabilityDay, StudentAvailabilityDayDto> studentAvailabilityDayAssembler, IAssembler<DateTimePeriod, DateTimePeriodDto> dateTimePeriodAssembler, IScenarioProvider scenarioProvider)
        {
            _scheduleRepository = scheduleRepository;
            _scheduleDictionarySaver = scheduleDictionarySaver;
            _shiftCategoryRepository = shiftCategoryRepository;
            _personAssembler = personAssembler;
            _ruleSetProjectionService = ruleSetProjectionService;
            _activityRepository = activityRepository;
            _personRepository = personRepository;
            _scheduleDayAssembler = scheduleDayAssembler;
            _preferenceDayAssembler = preferenceDayAssembler;
            _studentAvailabilityDayAssembler = studentAvailabilityDayAssembler;
            _dateTimePeriodAssembler = dateTimePeriodAssembler;
            _scenarioProvider = scenarioProvider;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        internal ICollection<MultiplicatorDataDto> CreateMultiplicatorData(ICollection<PersonDto> personCollection, DateOnlyDto startDate, DateOnlyDto endDate, string timeZoneInfoId)
        {
            IList<MultiplicatorDataDto> multiplicatorDataDtos = new List<MultiplicatorDataDto>();

            var timeZone = new CccTimeZoneInfo(TimeZoneInfo.FindSystemTimeZoneById(timeZoneInfoId));
            var datePeriod = new DateOnlyPeriod(new DateOnly(startDate.DateTime), new DateOnly(endDate.DateTime));
            DateTimePeriod period = new DateOnlyPeriod(datePeriod.StartDate, datePeriod.EndDate.AddDays(1)).ToDateTimePeriod(timeZone);
            
            using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                IList<IPerson> personList = _personAssembler.DtosToDomainEntities(personCollection).ToList();
                IScheduleDictionary scheduleDictonary = _scheduleRepository.FindSchedulesOnlyInGivenPeriod(new PersonProvider(personList), new ScheduleDictionaryLoadOptions(false, false), period, _scenarioProvider.DefaultScenario());

                //rk don't know if I break stuff here...
                //scheduleDictonary.SetTimeZone(timeZone);
                foreach (IPerson person in personList)
                {
                    IScheduleRange scheduleRange = scheduleDictonary[person];
                    foreach (DateOnly dateOnly in datePeriod.DayCollection())
                    {
                        var scheduleDay = scheduleRange.ScheduledDay(dateOnly);
                        var multiplicatorProjectionService = new MultiplicatorProjectionService(scheduleDay, dateOnly);
                        //Create Activity Layers
                        foreach (IMultiplicatorLayer layer in multiplicatorProjectionService.CreateProjection())
                        {
                            var multiplicatorDataDto = new MultiplicatorDataDto
                                                           {
                                                               Date = dateOnly,
                                                               ActualDate =
                                                                   layer.Period.StartDateTimeLocal(scheduleDay.TimeZone)
                                                                   .Date,
                                                               Amount = layer.Period.ElapsedTime(),
                                                               Multiplicator = new MultiplicatorDto(layer.Payload),
                                                               PersonId = person.Id
                                                           };

                            multiplicatorDataDtos.Add(multiplicatorDataDto);
                        }
                    }
                }
            }

            return multiplicatorDataDtos;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dateInPeriod")]
        public ICollection<ValidatedSchedulePartDto> GetValidatedSchedulePartsOnSchedulePeriod(
            PersonDto personDto, 
            DateOnlyDto dateInPeriod, 
            string timeZoneInfoId, bool useStudentAvailability)
        {
            IList<IPerson> personList = new List<IPerson>();
            IVirtualSchedulePeriod schedulePeriod;
            if (!personDto.Id.HasValue)
                return new List<ValidatedSchedulePartDto>();

            var dateOnlyInPeriod = new DateOnly(dateInPeriod.DateTime); 
            IPerson person;
            using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
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
                    EmploymentType.FixedStaffNormalWorkTime)                    daysOffTarget = schedulePeriod.DaysOff();
                // find out wich period to load
                DateTimePeriod period = realSchedulePeriod.ToDateTimePeriod(timeZoneInfo);
                ISchedulerRangeToLoadCalculator rangeToLoadCalculator = new SchedulerRangeToLoadCalculator(period)
                                                                            {JusticeValue = 0};
                period = rangeToLoadCalculator.SchedulerRangeToLoad(person);

                var clientsCulture = GetClientsCulture(personDto, person);

                var personTimeZone = person.PermissionInformation.DefaultTimeZone();
                if (TimeZoneHelper.ConvertFromUtc(period.StartDateTime, personTimeZone).DayOfWeek != clientsCulture.DateTimeFormat.FirstDayOfWeek)
                {
                    DateTime dateTime = TimeZoneHelper.ConvertFromUtc(period.StartDateTime,personTimeZone);
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
                        new SchedulePeriodTargetTimeTimeCalculator();
                    var schedulePeriodTargetBalanced = 
                        periodTargetTimeCalculator.TargetTime(scheduleMatrix);
                    var tolerance = periodTargetTimeCalculator.TargetWithTolerance(scheduleMatrix);

                    DateOnlyPeriod loadedPeriod = period.ToDateOnlyPeriod(timeZoneInfo);

                    IPeriodScheduledAndRestrictionDaysOff periodScheduledAndRestrictionDaysOff =
                        new PeriodScheduledAndRestrictionDaysOff();
                    IRestrictionExtractor extractor = new RestrictionExtractor(stateHolder);
                    int numberOfDaysOff = periodScheduledAndRestrictionDaysOff.CalculatedDaysOff(extractor,
                                                                                                 scheduleMatrix, true,
                                                                                                 true, false);

                    var restrictionValidator = new RestrictionsValidator(new IsEditablePredicate(),
                                                                         _preferenceDayAssembler,
                                                                         _studentAvailabilityDayAssembler,
                                                                         new MinMaxWorkTimeChecker(
																			 _ruleSetProjectionService), clientsCulture, 
																			 new PreferenceNightRestChecker(new NightlyRestFromPersonOnDayExtractor(person)));

                    return restrictionValidator.ValidateSchedulePeriod(loadedPeriod,
                                                realSchedulePeriod, stateHolder,
                                                (int) periodTarget.TotalMinutes, (int)tolerance.StartTime.TotalMinutes, (int)tolerance.EndTime.TotalMinutes, daysOffTarget, person,
                                                schedulePeriod.MustHavePreference, (int)schedulePeriodTargetBalanced.TotalMinutes,(int)schedulePeriod.BalanceIn.TotalMinutes,
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public ICollection<SchedulePartDto> CreateSchedulePartCollection(IEnumerable<PersonDto> personCollection, 
																		DateOnlyDto startDate, 
																		DateOnlyDto endDate, 
																		string timeZoneInfoId,
																		string specialProjection)
        {
            IList<SchedulePartDto> returnList = new List<SchedulePartDto>();

            var timeZone = new CccTimeZoneInfo(TimeZoneInfo.FindSystemTimeZoneById(timeZoneInfoId));
            var datePeriod = new DateOnlyPeriod(new DateOnly(startDate.DateTime), new DateOnly(endDate.DateTime));
            DateTimePeriod period = new DateOnlyPeriod(datePeriod.StartDate,datePeriod.EndDate.AddDays(1)).ToDateTimePeriod(timeZone);

            ((DateTimePeriodAssembler) _dateTimePeriodAssembler).TimeZone = timeZone;
            using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                IList<IPerson> personList = _personAssembler.DtosToDomainEntities(personCollection).ToList();

                IScheduleDictionary scheduleDictionary = _scheduleRepository.FindSchedulesOnlyInGivenPeriod(new PersonProvider(personList), new ScheduleDictionaryLoadOptions(true, false), period, _scenarioProvider.DefaultScenario());
                foreach (IPerson person in personList)
                {
                    IScheduleRange scheduleRange = scheduleDictionary[person];
                    foreach (DateOnly dateOnly in datePeriod.DayCollection())
                    {
                        IScheduleDay part = scheduleRange.ScheduledDay(dateOnly);
						//rk - ugly hack until ScheduleProjectionService is stateless (=not depended on schedulday in ctor)
						//when that's done - inject a IProjectionService instead.
                    	var schedAss = ((SchedulePartAssembler) _scheduleDayAssembler);
						schedAss.SpecialProjection = specialProjection;
                    	schedAss.TimeZone = timeZone;
                        returnList.Add(_scheduleDayAssembler.DomainEntityToDto(part));
                    }
                }
            }

            return returnList;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        internal void SaveSchedulePart(SchedulePartDto schedulePartDto)
        {
            using (new MessageBrokerSendEnabler())
            {
                using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
                {
                    IScheduleDay schedulePart = _scheduleDayAssembler.DtoToDomainEntity(schedulePartDto);
                    var dic = (ReadOnlyScheduleDictionary)schedulePart.Owner;
                    dic.MakeEditable();
                    IList<IBusinessRuleResponse> skipList = new List<IBusinessRuleResponse>();
                    IBusinessRuleResponse toAdd = new BusinessRuleResponse(typeof (OpenHoursRule), "", false, false,
                                                                           new DateTimePeriod(), schedulePart.Person, new DateOnlyPeriod());
                    skipList.Add(toAdd);

                    //inga regler för nu
                    var invalidList = dic.Modify(ScheduleModifier.Scheduler,
                                                                          schedulePart,
                                                                          NewBusinessRuleCollection.Minimum(),new EmptyScheduleDayChangeCallback(), new ScheduleTagSetter(NullScheduleTag.Instance));

                    if (invalidList.Count() > 0)
                        throw new NotImplementedException("FIX THIS LATER" + invalidList.First().Message);

                    _scheduleDictionarySaver.MarkForPersist(uow, _scheduleRepository, dic.DifferenceSinceSnapshot());
                    uow.PersistAll();
                }
            }
        }
    }
}
