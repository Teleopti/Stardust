using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.Restriction
{
    public class AvailableRestrictionViewModel:RestrictionViewModel
    {
        public AvailableRestrictionViewModel(IStudentAvailabilityRestriction availabilityRestriction,IRestrictionAltered parent)
        {
            ParentToCommitChanges = parent;
            Restriction = availabilityRestriction;
            StartTimeLimits = new LimitationViewModel(availabilityRestriction.StartTimeLimitation, true, false);
            EndTimeLimits = new LimitationViewModel(availabilityRestriction.EndTimeLimitation, false, true);
            WorkTimeLimits = new LimitationViewModel(availabilityRestriction.WorkTimeLimitation, true, true);
        }

        public override string Description
        {
            get
            {
                return UserTexts.Resources.StudentAvailability;
            }
        }

        public override void CommitChanges()
        {
            {
                if (ParentToCommitChanges != null)
                {
                    UpdateTimeProperties();

                    if (ScheduleDay != null && !BelongsToPart() && Restriction.IsRestriction())
                        ScheduleDay.Add((IStudentAvailabilityDay)Restriction.Parent);

                    if (ScheduleDay != null && BelongsToPart() && !Restriction.IsRestriction())
                        ScheduleDay.Remove((IStudentAvailabilityDay)Restriction.Parent);

                   ParentToCommitChanges.RestrictionIsAltered = true;
                }
            }
        }

        public bool Available
        {
            get { return !((IStudentAvailabilityDay)Restriction.Parent).NotAvailable; }
        }

        protected override bool CanExecuteDeleteCommand()
        {
            return Available;
        }

        public override bool BelongsToPart()
        {
            if (ScheduleDay == null)
                return false;

            IEnumerable<IStudentAvailabilityDay> studentAvailabilityDays = ScheduleDay.PersonRestrictionCollection().OfType<IStudentAvailabilityDay>();

            foreach (var studentAvailabilityDay in studentAvailabilityDays)
            {
                if (studentAvailabilityDay.RestrictionCollection.Count > 0 && studentAvailabilityDay.RestrictionCollection[0].Equals(Restriction))
                    return true;
            }

            return false;
        }
    }
}