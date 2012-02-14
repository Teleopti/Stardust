using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.Restriction
{
    public class RotationRestrictionViewModel : RestrictionViewModel
    {
        public RotationRestrictionViewModel(IRotationRestriction rotationRestriction)
        {
            Restriction = rotationRestriction;
            StartTimeLimits = new LimitationViewModel(rotationRestriction.StartTimeLimitation);
            StartTimeLimits.Editable = false;
            EndTimeLimits = new LimitationViewModel(rotationRestriction.EndTimeLimitation);
            WorkTimeLimits = new LimitationViewModel(rotationRestriction.WorkTimeLimitation);
        }

        public override string Description
        {
            get
            {
                return UserTexts.Resources.Rotation;
            }
        }

        protected override bool CanExecuteDeleteCommand()
        {
            return false;
        }

        public override void CommitChanges()
        {
            //Will not comit any changes on rotation, this will not get called 
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value")]
        public bool DayOff
        {
            get
            {
                if (((IRotationRestriction)Restriction).DayOffTemplate != null)
                {
                    return true;
                }
                return false;
            }
            set { }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value")]
        public string ShiftCategory
        {
            get
            {
                return ((IRotationRestriction) Restriction).ShiftCategory.Description.Name;
            }
            set { }
        }
    }
}