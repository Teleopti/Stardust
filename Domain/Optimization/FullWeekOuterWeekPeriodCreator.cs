﻿using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public class FullWeekOuterWeekPeriodCreator : IFullWeekOuterWeekPeriodCreator
    {
        private DateOnlyPeriod _selectedPeriod;
        private readonly IPerson _person;
        private DateOnlyPeriod _fullWeekPeriod;
        private DateOnlyPeriod _outerPeriod;

        /// <summary>
        /// Initializes a new instance of the <see cref="FullWeekOuterWeekPeriodCreator"/> class.
        /// </summary>
        /// <param name="selectedPeriod">The period.</param>
        /// <param name="person">The person.</param>
        public FullWeekOuterWeekPeriodCreator(DateOnlyPeriod selectedPeriod, IPerson person)
        {
            _selectedPeriod = selectedPeriod;
            _person = person;
            CreateOuterAndExtendedPeriods(person);
        }

        public DateOnlyPeriod EffectivePeriod()
        {
            return _selectedPeriod;
        }

        public DateOnlyPeriod FullWeekPeriod()
        {
            return _fullWeekPeriod;
        }

        public DateOnlyPeriod OuterWeekPeriod()
        {
            return _outerPeriod;
        }

        public IPerson Person
        {
            get { return _person; }
        }

        private void CreateOuterAndExtendedPeriods(IPerson person)
        {
            var firstPeriodsFirstDateLocal = new DateOnly(DateHelper.GetFirstDateInWeek(_selectedPeriod.StartDate, person.FirstDayOfWeek));
            var lastPeriodsLastDateLocal = new DateOnly(DateHelper.GetLastDateInWeek(_selectedPeriod.EndDate, person.FirstDayOfWeek));

            _fullWeekPeriod = new DateOnlyPeriod(firstPeriodsFirstDateLocal, lastPeriodsLastDateLocal);
            _outerPeriod = new DateOnlyPeriod(_fullWeekPeriod.StartDate.AddDays(-7), _fullWeekPeriod.EndDate.AddDays(7));

        }
    }
}