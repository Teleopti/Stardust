using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
    public class GroupShiftCategoryFairnessCreator : IGroupShiftCategoryFairnessCreator
    {
        private readonly IGroupPagePerDateHolder _groupPagePerDateHolder;
        private readonly ISchedulingResultStateHolder _resultStateHolder;
        
        public GroupShiftCategoryFairnessCreator(IGroupPagePerDateHolder groupPagePerDateHolder, ISchedulingResultStateHolder resultStateHolder)
        {
            _groupPagePerDateHolder = groupPagePerDateHolder;
            _resultStateHolder = resultStateHolder;
        }

        private IScheduleDictionary ScheduleDictionary
        { get { return _resultStateHolder.Schedules; } }

        public IShiftCategoryFairness CalculateGroupShiftCategoryFairness(IPerson person, DateOnly dateOnly)
        {
            IGroupPage groupPage = _groupPagePerDateHolder.ShiftCategoryFairnessGroupPagePerDate.GetGroupPageByDate(dateOnly);
            return CalculateGroupShiftCategoryFairness(groupPage, person, dateOnly);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public IShiftCategoryFairness CalculateGroupShiftCategoryFairness(IGroupPage groupPage, IPerson person, DateOnly dateOnly)
        {
            var rootGroups = groupPage.RootGroupCollection;
            var retLis = new List<IPerson>();

            foreach (var rootPersonGroup in rootGroups)
            {
                CheckGroupCollection(person, rootPersonGroup, retLis);
            }

            IShiftCategoryFairness groupFairness = new ShiftCategoryFairness();
            foreach (IPerson member in retLis)
            {
				if (member.TerminalDate != null && member.TerminalDate < dateOnly)
					continue;
                if (!member.VirtualSchedulePeriod(dateOnly).IsValid)
                    continue;
                IScheduleRange range = ScheduleDictionary[member];
                IShiftCategoryFairness fairness = range.CachedShiftCategoryFairness();
                groupFairness = groupFairness.Add(fairness);
            }
            return groupFairness;
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
