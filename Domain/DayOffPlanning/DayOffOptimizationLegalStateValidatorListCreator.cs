using System;
using System.Collections;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.DayOffPlanning
{
    /// <summary>
    /// Creates a list of <see cref="IDayOffLegalStateValidator"/> for day off optimization functionality.
    /// </summary>
    public class DayOffOptimizationLegalStateValidatorListCreator : IDayOffLegalStateValidatorListCreator
    {
        #region Variables
        
        private readonly IList<IDayOffLegalStateValidator> _validators 
            = new List<IDayOffLegalStateValidator>();
        private readonly IDaysOffPreferences _daysOffPreferences;
        private readonly IOfficialWeekendDays _officialWeekendDays;
        private readonly MinMax<int> _periodIndexRange;
        private readonly BitArray _originalPeriod;
        
        #endregion

        #region Constructor

        public DayOffOptimizationLegalStateValidatorListCreator(
            IDaysOffPreferences daysOffPreferences,
            IOfficialWeekendDays officialWeekendDays,
            BitArray originalPeriod,
            MinMax<int> periodIndexRange)
        {
            _daysOffPreferences = daysOffPreferences;
            _officialWeekendDays = officialWeekendDays;
            _originalPeriod = originalPeriod;
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
            CreateKeepFreeWeekendsValidatorConditionally();
            CreateKeepFreeWeekendDaysValidatorConditionally();
            return _validators;
        }

        #endregion

        #region Local

        private void CreateConsecutiveDayOffValidatorConditionally()
        {
            if (_daysOffPreferences.UseConsecutiveDaysOff)
            {
                IDayOffLegalStateValidator validator = new ConsecutiveDayOffValidator(
                    _daysOffPreferences.ConsecutiveDaysOffValue,
                    _daysOffPreferences.ConsiderWeekBefore,
                    _daysOffPreferences.ConsiderWeekAfter);
                _validators.Add(validator);
            }
        }

        private void CreateConsecutiveWorkdayValidatorConditionally()
        {
            if (_daysOffPreferences.UseConsecutiveWorkdays)
            {
                IDayOffLegalStateValidator validator = new ConsecutiveWorkdayValidator( 
                    _daysOffPreferences.ConsecutiveWorkdaysValue,
                    _daysOffPreferences.ConsiderWeekBefore,
                    _daysOffPreferences.ConsiderWeekAfter);
                _validators.Add(validator);
            }
        }

        private void CreateWeeklyDayOffValidatorConditionally()
        {
            if (_daysOffPreferences.UseDaysOffPerWeek)
            {
                IDayOffLegalStateValidator validator =
                    new WeeklyDayOffValidator(_daysOffPreferences.DaysOffPerWeekValue);
                _validators.Add(validator);
            }
        }

        private void CreateFreeWeekendDaysValidatorConditionally()
        {
            if (_daysOffPreferences.UseWeekEndDaysOff)
            {
                IDayOffLegalStateValidator validator =
                    new FreeWeekendDayValidator(
                        _daysOffPreferences.WeekEndDaysOffValue,
                        _officialWeekendDays,
                        _periodIndexRange);
                _validators.Add(validator);
            }
        }

        private void CreateFreeWeekendsValidatorConditionally()
        {
            if (_daysOffPreferences.UseFullWeekendsOff)
            {
                IDayOffLegalStateValidator validator =
                    new FreeWeekendValidator(
                        _daysOffPreferences.FullWeekendsOffValue,
                        _officialWeekendDays, 
                        _periodIndexRange);
                _validators.Add(validator);
            }
        }

        private void CreateKeepFreeWeekendDaysValidatorConditionally()
        {
            // ***** check settings
            if (_daysOffPreferences.KeepFreeWeekendDays)
            {
                IDayOffLegalStateValidator validator =
                    new KeepFreeWeekendDayValidator(
                        _originalPeriod,
                        _officialWeekendDays,
                        _periodIndexRange);
                _validators.Add(validator);
            }
        }

        private void CreateKeepFreeWeekendsValidatorConditionally()
        {
            // ***** check settings
            if (_daysOffPreferences.KeepFreeWeekends)
            {
                IDayOffLegalStateValidator validator =
                    new KeepFreeWeekendValidator(
                        _originalPeriod,
                        _officialWeekendDays,
                        _periodIndexRange);
                _validators.Add(validator);
            }
        }

        
        #endregion

    }
}