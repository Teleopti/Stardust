using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.ShiftCategoryFairness
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
            return obj.GetType() == typeof(ShiftCategoryFairnessSwap) && equals((ShiftCategoryFairnessSwap)obj);
        }

        private bool equals(ShiftCategoryFairnessSwap other)
        {
        	//return GetHashCode().Equals(other.GetHashCode());
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
				var g1 = (Group1 != null ? Group1.GetHashCode() : 0);
				var g2 = (Group2 != null ? Group2.GetHashCode() : 0);
                var result = (g1 ^ g2) * 397;
	            var s1 = (ShiftCategoryFromGroup1 != null ? ShiftCategoryFromGroup1.GetHashCode() : 0);
	            var s2 = (ShiftCategoryFromGroup2 != null ? ShiftCategoryFromGroup2.GetHashCode() : 0);
				var result2 = (s1 ^ s2) * 397;
	            result = result ^ result2;
                return result;
            }
        }
    }
}
