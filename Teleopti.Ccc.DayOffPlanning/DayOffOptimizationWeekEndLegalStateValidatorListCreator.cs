using System;
using System.Collections;
using System.Collections.Generic;
using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DayOffPlanning
{
    /// <summary>
    /// Creates a list of <see cref="IDayOffLegalStateValidator"/> for day off optimization functionality.
    /// </summary>
    public class DayOffOptimizationWeekendLegalStateValidatorListCreator : IDayOffLegalStateValidatorListCreator
    {
        #region Variables

        private readonly IList<IDayOffLegalStateValidator> _validators
            = new List<IDayOffLegalStateValidator>();
        private readonly IDayOffPlannerRules _dayOffPlannerRules;
        private readonly IOfficialWeekendDays _officialWeekendDays;
        private readonly MinMax<int> _periodIndexRange;

        #endregion

        #region Constructor

        public DayOffOptimizationWeekendLegalStateValidatorListCreator(
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
            CreateFreeWeekendsValidatorConditionally();
            return _validators;
        }

        #endregion

        #region Local


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
