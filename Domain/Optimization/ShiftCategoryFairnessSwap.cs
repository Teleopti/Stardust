using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface IShiftCategoryFairnessSwap
    {
        IShiftCategoryFairnessCompareResult Group1 { get; set; }
        IShiftCategoryFairnessCompareResult Group2 { get; set; }
        IShiftCategory ShiftCategoryFromGroup1 { get; set; }
        IShiftCategory ShiftCategoryFromGroup2 { get; set; }
    }

    public class ShiftCategoryFairnessSwap : IShiftCategoryFairnessSwap
    {
        public IShiftCategoryFairnessCompareResult Group1 { get; set; }
        public IShiftCategoryFairnessCompareResult Group2 { get; set; }
        public IShiftCategory ShiftCategoryFromGroup1 { get; set; }
        public IShiftCategory ShiftCategoryFromGroup2 { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(ShiftCategoryFairnessSwap) && Equals((ShiftCategoryFairnessSwap)obj);
        }

        public bool Equals(ShiftCategoryFairnessSwap other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return (Equals(other.Group1, Group1) && Equals(other.Group2, Group2) &&
                   other.ShiftCategoryFromGroup1.Description.Name == ShiftCategoryFromGroup1.Description.Name &&
                   other.ShiftCategoryFromGroup2.Description.Name == ShiftCategoryFromGroup2.Description.Name)
                   || 
                   (Equals(other.Group1, Group2) && Equals(other.Group2, Group1) &&
                   other.ShiftCategoryFromGroup1.Description.Name == ShiftCategoryFromGroup2.Description.Name &&
                   other.ShiftCategoryFromGroup2.Description.Name == ShiftCategoryFromGroup1.Description.Name);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var result = (Group1 != null ? Group1.GetHashCode() : 0);
                result = (result * 397) ^ (Group2 != null ? Group2.GetHashCode() : 0);
                result = (result * 397) ^ (ShiftCategoryFromGroup1 != null ? ShiftCategoryFromGroup1.GetHashCode() : 0);
                result = (result * 397) ^ (ShiftCategoryFromGroup2 != null ? ShiftCategoryFromGroup2.GetHashCode() : 0);
                return result;
            }
        }
    }
}
