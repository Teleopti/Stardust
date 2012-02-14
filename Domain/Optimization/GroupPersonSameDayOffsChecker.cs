using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface IGroupPersonSameDayOffsChecker
    {
        bool CheckGroupPerson(IList<IScheduleMatrixPro> matrixes, IGroupPerson groupPerson,
                              IList<DateOnly> daysOffToRemove, IList<DateOnly> daysOffToAdd);
    }

    public class GroupPersonSameDayOffsChecker : IGroupPersonSameDayOffsChecker
    {
        public bool CheckGroupPerson(IList<IScheduleMatrixPro> matrixes, IGroupPerson groupPerson, IList<DateOnly> daysOffToRemove, IList<DateOnly> daysOffToAdd)
        {
            if (matrixes == null || groupPerson == null || daysOffToRemove == null || daysOffToAdd == null)
                return false;
            if (!daysOffToAdd.Count.Equals(daysOffToRemove.Count))
                return false;
            if (daysOffToRemove.Count == 0)
                return false;

            var members = groupPerson.GroupMembers;

            foreach (var person in members)
            {
                IScheduleMatrixPro matrix = null;
                foreach (var scheduleMatrix in matrixes)
                {
                    if (scheduleMatrix.Person == person)
                    {
                        if (scheduleMatrix.SchedulePeriod.DateOnlyPeriod.Contains(daysOffToRemove[0]))
                        {
                            matrix = scheduleMatrix;
                        }
                    }
                }

                if (matrix == null)
                    return false;

                if (!hasDayOff(matrix, daysOffToRemove))
                    return false;

                if (hasDayOff(matrix, daysOffToAdd))
                    return false;
            }

            return true;
        }

        private static bool hasDayOff(IScheduleMatrixPro scheduleMatrixPro, IEnumerable<DateOnly> daysOffToRemove)
        {
            foreach (var dateOnly in daysOffToRemove)
            {
                var scheduleDay = scheduleMatrixPro.GetScheduleDayByKey(dateOnly);
                if (!scheduleDay.DaySchedulePart().SignificantPart().Equals(SchedulePartView.DayOff))
                    return false;
            }

            return true;
        }

    }
}