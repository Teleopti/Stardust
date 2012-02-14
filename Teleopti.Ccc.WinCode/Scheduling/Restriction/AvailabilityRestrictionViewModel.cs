using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.Restriction
{
    public class AvailabilityRestrictionViewModel : RestrictionViewModel
    {
        public AvailabilityRestrictionViewModel(IAvailabilityRestriction availabilityRestriction)
        {
            Restriction = availabilityRestriction;
            StartTimeLimits = new LimitationViewModel(availabilityRestriction.StartTimeLimitation, true, false);

            EndTimeLimits = new LimitationViewModel(availabilityRestriction.EndTimeLimitation, false, true);
            WorkTimeLimits = new LimitationViewModel(availabilityRestriction.WorkTimeLimitation, true, true);
        }

        public override string Description
        {
            get
            {
                return UserTexts.Resources.Availability;
            }
        }

        protected override bool CanExecuteDeleteCommand()
        {
            return false;
        }

        public override void CommitChanges()
        {
            //Will not commit any changes on availability, this will not get called 
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value")]
        public bool HasDayOff
        {
            get { return ((IAvailabilityRestriction)Restriction).NotAvailable; }
            set { }
        }
    }
}