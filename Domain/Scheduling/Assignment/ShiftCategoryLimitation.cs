using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
    public class ShiftCategoryLimitation : IShiftCategoryLimitation
    {
        private IShiftCategory _shiftCategory;

        public bool Weekly { get; set;}
        public int MaxNumberOf{ get; set;}

        private ShiftCategoryLimitation(){}

        public ShiftCategoryLimitation(IShiftCategory shiftCategory):this()
        {
            InParameter.NotNull("shiftCategory", shiftCategory);
            _shiftCategory = shiftCategory;
        }

        public IShiftCategory ShiftCategory
        {
            get { return _shiftCategory; }
            set
            {
                InParameter.NotNull("ShiftCategory", value);
                _shiftCategory = value;
            }
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

    }
}