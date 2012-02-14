using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface IGroupPersonPreOptimizationChecker
    {
        IGroupPerson CheckPersonOnDates(IList<IScheduleMatrixPro> matrixes, IPerson person, IList<DateOnly> daysOffToRemove, IList<DateOnly> daysOffToAdd,
                                                IList<IPerson> allSelectedPersons);

        IGroupPersonsBuilder GroupPersonBuilder { get; }
    }

    public class GroupPersonPreOptimizationChecker : IGroupPersonPreOptimizationChecker
    {
        private readonly IGroupPersonSameForPersonOnDateChecker _groupPersonSameForPersonOnDateChecker;
        private readonly IGroupPersonSameDayOffsChecker _groupPersonSameDayOffsChecker;
        private readonly IGroupPersonSchedulePeriodChecker _groupPersonSchedulePeriodChecker;

        public GroupPersonPreOptimizationChecker(IGroupPersonSameForPersonOnDateChecker groupPersonSameForPersonOnDateChecker, IGroupPersonSameDayOffsChecker groupPersonSameDayOffsChecker,
            IGroupPersonSchedulePeriodChecker groupPersonSchedulePeriodChecker)
        {
            _groupPersonSameForPersonOnDateChecker = groupPersonSameForPersonOnDateChecker;
            _groupPersonSameDayOffsChecker = groupPersonSameDayOffsChecker;
            _groupPersonSchedulePeriodChecker = groupPersonSchedulePeriodChecker;
        }

        public IGroupPerson CheckPersonOnDates(IList<IScheduleMatrixPro> matrixes, IPerson person, IList<DateOnly> daysOffToRemove, 
            IList<DateOnly> daysOffToAdd, IList<IPerson> allSelectedPersons)
        {
            var dateOnlyList = new List<DateOnly>(daysOffToRemove);
            dateOnlyList.AddRange(daysOffToAdd);

            var groupPerson = _groupPersonSameForPersonOnDateChecker.FindCommonGroupPersonForPersonOnDates(person,
                                                                                                           dateOnlyList,
                                                                                                           allSelectedPersons);
            if (groupPerson == null)
                return null;

            if (!_groupPersonSchedulePeriodChecker.AllInSameGroupHasSameSchedulePeriod(groupPerson, dateOnlyList))
                return null;

            if (!_groupPersonSameDayOffsChecker.CheckGroupPerson(matrixes, groupPerson, daysOffToRemove, daysOffToAdd))
                return null;

            

            return groupPerson;
        }

        public IGroupPersonsBuilder GroupPersonBuilder
        {
            get { return _groupPersonSameForPersonOnDateChecker.GroupPersonsBuilder; }
        }
    }
}