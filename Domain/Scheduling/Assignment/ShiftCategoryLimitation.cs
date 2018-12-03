using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

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
            InParameter.NotNull(nameof(shiftCategory), shiftCategory);
            _shiftCategory = shiftCategory;
        }

        public IShiftCategory ShiftCategory
        {
            get { return _shiftCategory; }
            set
            {
                InParameter.NotNull(nameof(ShiftCategory), value);
                _shiftCategory = value;
            }
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}