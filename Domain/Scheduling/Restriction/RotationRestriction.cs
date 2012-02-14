using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Restriction
{

    public class RotationRestriction : RestrictionBase, IRotationRestriction
    {
        private IShiftCategory _shiftCategory;
        private IDayOffTemplate _dayOffTemplate;

        public virtual IShiftCategory ShiftCategory
        {
            get { return _shiftCategory; }
            set { _shiftCategory = value; }
        }

        public virtual IDayOffTemplate DayOffTemplate
        {
            get { return _dayOffTemplate; }
            set { _dayOffTemplate = value; }
        }

        public override bool IsRestriction()
        {
            if (ShiftCategory != null || DayOffTemplate != null)
                return true;

            return base.IsRestriction();
        }
    }

}