using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Optimization.ShiftCategoryFairness;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
    public class GroupShiftCategoryFairnessCreator : IGroupShiftCategoryFairnessCreator
    {
        private readonly Func<IGroupPagePerDateHolder> _groupPagePerDateHolder;
        private readonly Func<ISchedulingResultStateHolder> _resultStateHolder;
        
        public GroupShiftCategoryFairnessCreator(Func<IGroupPagePerDateHolder> groupPagePerDateHolder, Func<ISchedulingResultStateHolder> resultStateHolder)
        {
            _groupPagePerDateHolder = groupPagePerDateHolder;
            _resultStateHolder = resultStateHolder;
        }

        public IShiftCategoryFairnessHolder CalculateGroupShiftCategoryFairness(IPerson person, DateOnly dateOnly)
        {
            IGroupPage groupPage = _groupPagePerDateHolder().ShiftCategoryFairnessGroupPagePerDate.GetGroupPageByDate(dateOnly);
            return CalculateGroupShiftCategoryFairness(groupPage, person, dateOnly);
        }

        public IShiftCategoryFairnessHolder CalculateGroupShiftCategoryFairness(IGroupPage groupPage, IPerson person, DateOnly dateOnly)
        {
            var rootGroups = groupPage.RootGroupCollection;
            var retLis = new List<IPerson>();

            foreach (var rootPersonGroup in rootGroups)
            {
                CheckGroupCollection(person, rootPersonGroup, retLis);
            }

            IShiftCategoryFairnessHolder groupFairnessHolder = new ShiftCategoryFairnessHolder();
            foreach (IPerson member in retLis)
            {
				if (member.TerminalDate != null && member.TerminalDate < dateOnly)
					continue;
                if (!member.VirtualSchedulePeriod(dateOnly).IsValid)
                    continue;
				IScheduleRange range = _resultStateHolder().Schedules[member];
                IShiftCategoryFairnessHolder fairnessHolder = range.CachedShiftCategoryFairness();
                groupFairnessHolder = groupFairnessHolder.Add(fairnessHolder);
            }
            return groupFairnessHolder;
        }

        private void CheckGroupCollection(IPerson person, IPersonGroup group, List<IPerson> retLis)
        {

            if(group.PersonCollection.Contains(person))
            {
                foreach (IPerson member in group.PersonCollection)
                {
                    retLis.Add(member);
                }
                return;
            }
            foreach (IChildPersonGroup childGroup in group.ChildGroupCollection)
            {
                CheckChildGroupCollection(person, childGroup, retLis);
            }
        }

        private void CheckChildGroupCollection(IPerson person, IChildPersonGroup group, List<IPerson> retLis)
        {
             if(group.PersonCollection.Contains(person))
            {
                foreach (IPerson member in group.PersonCollection)
                {
                    retLis.Add(member);
                }
                return;
            }
            foreach (IChildPersonGroup childGroup in group.ChildGroupCollection)
            {
                CheckGroupCollection(person, childGroup, retLis);
            }
        }
    }
}
