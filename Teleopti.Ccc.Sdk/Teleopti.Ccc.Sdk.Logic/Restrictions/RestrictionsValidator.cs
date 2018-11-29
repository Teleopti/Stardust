using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;

namespace Teleopti.Ccc.Sdk.Logic.Restrictions
{
    public interface IRestrictionsValidator {
        IList<ValidatedSchedulePartDto> ValidateSchedulePeriod(DateOnlyPeriod loadedPeriod, 
            DateOnlyPeriod schedulePeriod, ISchedulingResultStateHolder stateHolder,
            int periodTargetInMinutes, int periodNegativeTolerance, int periodPositiveTolerance, int periodDayOffsTarget, IPerson person, int mustHave,
            int balancedPeriodTargetInMinutes, int balanceInInMinutes, int extraInMinutes, int balanceOutInMinutes, int numberOfDaysOff, double seasonality, bool useStudentAvailability);
        }

    public class RestrictionsValidator : IRestrictionsValidator
    {
        private readonly IIsEditablePredicate _isEditablePredicate;
        private readonly IAssembler<IPreferenceDay, PreferenceRestrictionDto> _preferenceDayAssembler;
        private readonly IMinMaxWorkTimeChecker _minMaxWorkTimeChecker;
        private readonly CultureInfo _culture;
    	private readonly IPreferenceNightRestChecker _preferenceNightRestChecker;
    	private readonly IAssembler<IStudentAvailabilityDay, StudentAvailabilityDayDto> _studentAvailabilityDayAssembler;

        public RestrictionsValidator(IIsEditablePredicate isEditablePredicate, IAssembler<IPreferenceDay, PreferenceRestrictionDto> preferenceDayAssembler, 
			IAssembler<IStudentAvailabilityDay, StudentAvailabilityDayDto> studentAvailabilityDayAssembler, IMinMaxWorkTimeChecker minMaxWorkTimeChecker, 
			CultureInfo culture, IPreferenceNightRestChecker preferenceNightRestChecker)
        {
            _isEditablePredicate = isEditablePredicate;
            _preferenceDayAssembler = preferenceDayAssembler;
            _studentAvailabilityDayAssembler = studentAvailabilityDayAssembler;
            _minMaxWorkTimeChecker = minMaxWorkTimeChecker;
            _culture = culture;
        	_preferenceNightRestChecker = preferenceNightRestChecker;
        }
		
	    public IList<ValidatedSchedulePartDto> ValidateSchedulePeriod(DateOnlyPeriod loadedPeriod,
	                                                                  DateOnlyPeriod schedulePeriod,
	                                                                  ISchedulingResultStateHolder stateHolder,
	                                                                  int periodTargetInMinutes,
	                                                                  int periodNegativeTolerance,
	                                                                  int periodPositiveTolerance, int periodDayOffsTarget,
	                                                                  IPerson person, int mustHave,
	                                                                  int balancedPeriodTargetInMinutes,
	                                                                  int balanceInInMinutes, int extraInMinutes,
	                                                                  int balanceOutInMinutes, int numberOfDaysOff,
	                                                                  double seasonality, bool useStudentAvailability)
	    {
		    if (stateHolder == null)
			    throw new ArgumentNullException(nameof(stateHolder));

		    if (person == null)
			    throw new ArgumentNullException(nameof(person));

		    IList<ValidatedSchedulePartDto> result = new List<ValidatedSchedulePartDto>();

		    foreach (DateOnly dateOnly in loadedPeriod.DayCollection())
		    {
			    var dto = new ValidatedSchedulePartDto
				    {
					    MustHave = mustHave,
					    DateOnly = new DateOnlyDto {DateTime = dateOnly.Date},
					    IsInsidePeriod = schedulePeriod.Contains(dateOnly),
					    PeriodTargetInMinutes = periodTargetInMinutes,
					    PeriodDayOffsTarget = periodDayOffsTarget,
					    BalancedPeriodTargetInMinutes = balancedPeriodTargetInMinutes,
					    BalanceInInMinutes = balanceInInMinutes,
					    ExtraInInMinutes = extraInMinutes,
					    BalanceOutInMinutes = balanceOutInMinutes,
					    Seasonality = seasonality
				    };

			    IScheduleDay scheduleDay = stateHolder.Schedules[person].ScheduledDay(dateOnly);
			    var restrictionExtractor = new RestrictionExtractor(new RestrictionCombiner(), new RestrictionRetrievalOperation());
			    var restrictionResult = restrictionExtractor.Extract(scheduleDay);
				IEffectiveRestriction effectiveRestriction = restrictionResult.CombinedRestriction(new SchedulingOptions
					    {
						    UseRotations = false,
						    UsePreferences = true,
						    UseAvailability = true,
						    UseStudentAvailability = useStudentAvailability,
						    UsePreferencesMustHaveOnly = false
					    });
			    IPersonPeriod personPeriod = person.Period(dateOnly);

			    if (personPeriod == null) continue;
			    
			    dto.IsPreferenceEditable = _isEditablePredicate.IsPreferenceEditable(dateOnly, person);
			    dto.IsStudentAvailabilityEditable = _isEditablePredicate.IsStudentAvailabilityEditable(dateOnly, person);

			    IRuleSetBag ruleSetBag = personPeriod.RuleSetBag;

			    dto.WeekMaxInMinutes = (int) personPeriod.PersonContract.Contract.WorkTimeDirective.MaxTimePerWeek.TotalMinutes;
				if (ruleSetBag == null)
				{
					result.Add(dto);
					continue;
				}

			    IEnumerable<IPreferenceDay> preferenceList =
				    (from r in scheduleDay.PersistableScheduleDataCollection() where r is IPreferenceDay select (IPreferenceDay) r);
			    if (!preferenceList.IsEmpty())
				    dto.PreferenceRestriction = _preferenceDayAssembler.DomainEntityToDto(preferenceList.First());

			    var studentAvailabilityDays = (from r in scheduleDay.PersistableScheduleDataCollection()
			                                   where r is IStudentAvailabilityDay
			                                   select (IStudentAvailabilityDay) r);
			    if (!studentAvailabilityDays.IsEmpty())
				    dto.StudentAvailabilityDay = _studentAvailabilityDayAssembler.DomainEntityToDto(studentAvailabilityDays.First());

	            var assignment = scheduleDay.PersonAssignment();
			    var personMeetingCollection = scheduleDay.PersonMeetingCollection();

			    var significant = scheduleDay.SignificantPartForDisplay();
			    if (significant == SchedulePartView.DayOff)
                    checkPersonDayOffCollection(assignment.DayOff(), dto);

			    if (significant == SchedulePartView.MainShift)
                    checkPersonAssignmentCollection(assignment, dto);

			    if (significant == SchedulePartView.FullDayAbsence || significant == SchedulePartView.ContractDayOff)
			    {
				    IVisualLayerCollection visualLayerCollection = scheduleDay.ProjectionService().CreateProjection();
				    dto = AddFullDayAbsence(visualLayerCollection, dto, person);
			    }

			    if (significant == SchedulePartView.ContractDayOff)
				    dto.IsContractDayOff = true;

			    DateOnly? schedulePeriodStartDay = person.SchedulePeriodStartDate(dateOnly);
			    if (!schedulePeriodStartDay.HasValue)
				    continue;

			    dto.IsWorkday = personPeriod.PersonContract.ContractSchedule.IsWorkday(schedulePeriodStartDay.Value, dateOnly, person.FirstDayOfWeek);

			    dto.TargetTimeNegativeToleranceInMinutes = periodNegativeTolerance;
			    dto.TargetTimePositiveToleranceInMinutes = periodPositiveTolerance;

			    IWorkTimeMinMax minMaxLength = _minMaxWorkTimeChecker.MinMaxWorkTime(scheduleDay, ruleSetBag, effectiveRestriction, false);
				IWorkTimeMinMax minMaxContractLength = _minMaxWorkTimeChecker.MinMaxWorkTime(scheduleDay, ruleSetBag, effectiveRestriction, true);

			    AddMinMaxToDto(dto, minMaxLength);
				AddMinMaxContractTimeToDto(dto, minMaxContractLength);

				if ((personMeetingCollection.Length > 0 || (assignment!=null && assignment.PersonalActivities().Any())) 
					&& !(dto.HasShift || dto.HasDayOff || dto.HasAbsence))
				{
					dto.HasPersonalAssignmentOnly = true;
					dto.TipText = ScheduleDayStringVisualizer.GetToolTipPersonalAssignments(scheduleDay,
					                                                                        person.PermissionInformation
					                                                                              .DefaultTimeZone(),
					                                                                        person.PermissionInformation.Culture());
				}
				result.Add(dto);
		    }
		    TrimValidatedPartListToFirstDayOfWeek(result);
		    foreach (var partDto in result)
		    {
			    partDto.PeriodDayOffs = numberOfDaysOff;
		    }
		    _preferenceNightRestChecker.CheckNightlyRest(result);
		    return result;
	    }

		private static void AddMinMaxContractTimeToDto(ValidatedSchedulePartDto dto, IWorkTimeMinMax minMaxContractLength)
		{
			if (minMaxContractLength != null)
			{
				if (minMaxContractLength.WorkTimeLimitation.EndTime.HasValue)
					dto.MaxContractTimeInMinutes = (int)minMaxContractLength.WorkTimeLimitation.EndTime.Value.TotalMinutes;
				if (minMaxContractLength.WorkTimeLimitation.StartTime != null)
					dto.MinContractTimeInMinutes = (int)minMaxContractLength.WorkTimeLimitation.StartTime.Value.TotalMinutes;
			}
			else
			{
				dto.MaxContractTimeInMinutes = 0;
				dto.MinContractTimeInMinutes = 0;
			}	
		}

	    private static void AddMinMaxToDto(ValidatedSchedulePartDto dto, IWorkTimeMinMax minMaxLength)
        {
            if (minMaxLength != null)
            {
                dto.LegalState = true;
                if( minMaxLength.WorkTimeLimitation.EndTime.HasValue)
                    dto.MaxWorkTimeInMinutes = (int)minMaxLength.WorkTimeLimitation.EndTime.Value.TotalMinutes;
                if (minMaxLength.WorkTimeLimitation.StartTime != null)
                    dto.MinWorkTimeInMinutes = (int)minMaxLength.WorkTimeLimitation.StartTime.Value.TotalMinutes;

                if (minMaxLength.StartTimeLimitation.EndTime.HasValue)
                    dto.MaxStartTimeMinute = (int)minMaxLength.StartTimeLimitation.EndTime.Value.TotalMinutes;
                if (minMaxLength.StartTimeLimitation.StartTime != null)
                    dto.MinStartTimeMinute = (int)minMaxLength.StartTimeLimitation.StartTime.Value.TotalMinutes;
                
                if (minMaxLength.EndTimeLimitation.EndTime.HasValue)
                    dto.MaxEndTimeMinute = (int)minMaxLength.EndTimeLimitation.EndTime.Value.TotalMinutes;
                if (minMaxLength.EndTimeLimitation.StartTime != null)
                    dto.MinEndTimeMinute = (int)minMaxLength.EndTimeLimitation.StartTime.Value.TotalMinutes;
            }
            else
            {
                dto.LegalState = false;
                dto.MaxWorkTimeInMinutes = 0;
                dto.MinWorkTimeInMinutes = 0;
                dto.MaxStartTimeMinute = (int)TimeSpan.FromHours(24).TotalMinutes;
            }
        }

       
        /// <summary>
        /// Handles if the first person period of the
        /// agent does not start on the first day of week.
        /// This method add days until first day of week.
        /// </summary>
        /// <param name="parts"></param>
        public void TrimValidatedPartListToFirstDayOfWeek (IList<ValidatedSchedulePartDto> parts)
        {
            while (parts.Count > 0 && (parts[0].DateOnly.DateTime.DayOfWeek != _culture.DateTimeFormat.FirstDayOfWeek && parts[0].DateOnly.DateTime > DateTime.MinValue))
            {
                var partDto = new ValidatedSchedulePartDto
                                  {
                                      IsPreferenceEditable = false,
                                      IsStudentAvailabilityEditable = false,
									  DateOnly = new DateOnlyDto { DateTime = parts[0].DateOnly.DateTime.AddDays(-1) },
                                      LegalState = true,
                                      PeriodTargetInMinutes = parts[0].PeriodTargetInMinutes
                                  };
                parts.Insert(0, partDto);
            }
        }

        private static void checkPersonDayOffCollection(DayOff dayOff, ValidatedSchedulePartDto resultSoFar)
        {
					if (dayOff != null)
					{
						resultSoFar.ScheduledItemName = dayOff.Description.Name;
						resultSoFar.ScheduledItemShortName = dayOff.Description.ShortName;
						resultSoFar.DisplayColor = new ColorDto(dayOff.DisplayColor);
						resultSoFar.HasDayOff = true;
					}
        }

        private static void checkPersonAssignmentCollection(IPersonAssignment personAssignment, ValidatedSchedulePartDto resultSoFar)
        {
            if (personAssignment?.ShiftCategory == null)
                return;

            IShiftCategory cat = personAssignment.ShiftCategory;
            resultSoFar.HasShift = true;
            resultSoFar.DisplayColor = new ColorDto(cat.DisplayColor);
            resultSoFar.ScheduledItemName = cat.Description.Name;
            resultSoFar.ScheduledItemShortName = cat.Description.ShortName;
        }

    	private static ValidatedSchedulePartDto AddFullDayAbsence(IEnumerable<IVisualLayer> visualLayerCollection, ValidatedSchedulePartDto resultSoFar, IPerson person)
        {
            if (visualLayerCollection.IsEmpty())
                return resultSoFar;

            var layer = visualLayerCollection.First();
            resultSoFar.HasAbsence = true;
            resultSoFar.ScheduledItemName = layer.Payload.ConfidentialDescription(person).Name;
            resultSoFar.ScheduledItemShortName = layer.Payload.ConfidentialDescription(person).ShortName;
            resultSoFar.DisplayColor = new ColorDto(layer.Payload.ConfidentialDisplayColor(person));
            
            return resultSoFar;
        }
    }
}
