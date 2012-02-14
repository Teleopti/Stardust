using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.Restriction
{
    // used for studentavailability and preference when the user don't have permission to alter them
    public class ReadOnlyRestrictionViewModel : RestrictionViewModel
    {
        public ReadOnlyRestrictionViewModel(IRestrictionBase restriction)
        {
            Restriction = restriction;
            StartTimeLimits = new LimitationViewModel(restriction.StartTimeLimitation,false,false);
            EndTimeLimits = new LimitationViewModel(restriction.EndTimeLimitation, false, false);
            WorkTimeLimits = new LimitationViewModel(restriction.WorkTimeLimitation, false, false);
            PersistableScheduleData = (IScheduleData)restriction.Parent;
        }

        public override string Description
        {
            get
            {
                string ret = "";
                var pref = Restriction as IPreferenceRestriction;
                if (pref != null)
                    ret =  UserTexts.Resources.Preference;

                var stud = Restriction as IStudentAvailabilityRestriction;
                if (stud != null)
                    ret = UserTexts.Resources.StudentAvailability;

                return ret;
            }
        }

        protected override bool CanExecuteDeleteCommand()
        {
            return false;
        }

        public override void CommitChanges()
        {
            //Will not commit any changes , this will not get called 
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value")]
        public bool DayOff
        {
            get
            {
                var pref = Restriction as IPreferenceRestriction;
                if (pref != null)
                    return pref.DayOffTemplate != null;

                var stud = Restriction.Parent as IStudentAvailabilityDay;
                if (stud != null)
                    return stud.NotAvailable;
                
                return false;
            }
            set { }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value")]
        public string ShiftCategory
        {
            get
            {
                var pref = Restriction as IPreferenceRestriction;
                if (pref != null)
                    return pref.ShiftCategory.Description.Name;

                return "";
            }
            set { }
        }
    }
}