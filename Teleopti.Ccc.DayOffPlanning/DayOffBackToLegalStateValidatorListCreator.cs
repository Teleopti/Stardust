using System;
using System.Collections;
using System.Collections.Generic;
using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DayOffPlanning
{
    /// <summary>
    /// Creates a list of <see cref="IDayOffLegalStateValidator"/> for day off back to legal state functionality.
    /// </summary>
    public class DayOffBackToLegalStateValidatorListCreator : IDayOffLegalStateValidatorListCreator
    {
        #region Variables
        
        private readonly IList<IDayOffLegalStateValidator> _validators 
            = new List<IDayOffLegalStateValidator>();
        private readonly IDayOffPlannerRules _dayOffPlannerRules;
        private readonly IOfficialWeekendDays _officialWeekendDays;
        private readonly MinMax<int> _periodIndexRange;
        
        #endregion

        #region Constructor

        public DayOffBackToLegalStateValidatorListCreator(
            IDayOffPlannerRules dayOffPlannerRules,
            IOfficialWeekendDays officialWeekendDays,
            MinMax<int> periodIndexRange)
        {
            _dayOffPlannerRules = dayOffPlannerRules;
            _officialWeekendDays = officialWeekendDays;
            _periodIndexRange = periodIndexRange;
            if (_periodIndexRange.Minimum < 7)
                throw new ArgumentOutOfRangeException("periodIndexRange", "PeriodIndexRange.Minimum can never be less than 7");
        }

        #endregion

        #region Interface

        public IList<IDayOffLegalStateValidator> BuildActiveValidatorList()
        {
            _validators.Clear();
            CreateConsecutiveDayOffValidatorConditionally();
            CreateConsecutiveWorkdayValidatorConditionally();
            CreateWeeklyDayOffValidatorConditionally();
            CreateFreeWeekendDaysValidatorConditionally();
            CreateFreeWeekendsValidatorConditionally();
            return _validators;
        }

        #endregion

        #region Local

        private void CreateConsecutiveDayOffValidatorConditionally()
        {
            if (_dayOffPlannerRules.UseConsecutiveDaysOff)
            {
                IDayOffLegalStateValidator validator = new ConsecutiveDayOffValidator(
                    _dayOffPlannerRules.ConsecutiveDaysOff,
                    _dayOffPlannerRules.UsePreWeek,
                    _dayOffPlannerRules.UsePostWeek);
                _validators.Add(validator);
            }
        }

        private void CreateConsecutiveWorkdayValidatorConditionally()
        {
            if (_dayOffPlannerRules.UseConsecutiveWorkdays)
            {
                IDayOffLegalStateValidator validator = new ConsecutiveWorkdayValidator( 
                    _dayOffPlannerRules.ConsecutiveWorkdays,
                    _dayOffPlannerRules.UsePreWeek, 
                    _dayOffPlannerRules.UsePostWeek);
                _validators.Add(validator);
            }
        }

        private void CreateWeeklyDayOffValidatorConditionally()
        {
            if (_dayOffPlannerRules.UseDaysOffPerWeek)
            {
                IDayOffLegalStateValidator validator =
                    new WeeklyDayOffValidator(_dayOffPlannerRules.DaysOffPerWeek);
                _validators.Add(validator);
            }
        }

        private void CreateFreeWeekendDaysValidatorConditionally()
        {
            if (_dayOffPlannerRules.UseFreeWeekendDays)
            {
                IDayOffLegalStateValidator validator =
                    new FreeWeekendDayValidator(
                        _dayOffPlannerRules.FreeWeekendDays,
                        _officialWeekendDays,
                        _periodIndexRange);
                _validators.Add(validator);
            }
        }

        private void CreateFreeWeekendsValidatorConditionally()
        {
            if (_dayOffPlannerRules.UseFreeWeekends)
            {
                IDayOffLegalStateValidator validator =
                    new FreeWeekendValidator(
                        _dayOffPlannerRules.FreeWeekends,
                        _officialWeekendDays, 
                        _periodIndexRange);
                _validators.Add(validator);
            }
        }

        #endregion

    }
}