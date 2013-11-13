using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.ShiftCategoryFairness
{
	public interface IShiftCategoryFairnessCompareValue
	{
		IShiftCategory ShiftCategory { get; set; }
		double Original { get; set; }
		double ComparedTo { get; set; }
	}

	public class ShiftCategoryFairnessCompareValue : IShiftCategoryFairnessCompareValue
	{
		public IShiftCategory ShiftCategory { get; set; }
		public double Original { get; set; }
		public double ComparedTo { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof (ShiftCategoryFairnessCompareValue) && Equals((ShiftCategoryFairnessCompareValue) obj);
        }

        public bool Equals(ShiftCategoryFairnessCompareValue other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other.ShiftCategory.Description.Name.Equals(ShiftCategory.Description.Name) &&
                   other.Original.Equals(Original) && other.ComparedTo.Equals(ComparedTo);
        }

	    public override int GetHashCode()
	    {
	        unchecked
	        {
	            var result = (ShiftCategory != null ? ShiftCategory.GetHashCode() : 0);
	            result = (result*397) ^ Original.GetHashCode();
	            result = (result*397) ^ ComparedTo.GetHashCode();
	            return result;
	        }
	    }

        public override string ToString()
        {
            return ShiftCategory.Description.Name;
        }
	}

	public interface IShiftCategoryFairnessCompareResult
	{
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]      
		IList<IShiftCategoryFairnessCompareValue> ShiftCategoryFairnessCompareValues { get; set; }
		double StandardDeviation { get; set; }
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
		IList<IPerson> OriginalMembers { get; set; } 
	}

	public class ShiftCategoryFairnessCompareResult : IShiftCategoryFairnessCompareResult
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
		public IList<IShiftCategoryFairnessCompareValue> ShiftCategoryFairnessCompareValues { get; set; }
        
		public double StandardDeviation { get; set; }
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
		public IList<IPerson> OriginalMembers { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof (ShiftCategoryFairnessCompareResult) && Equals((ShiftCategoryFairnessCompareResult) obj);
        }

	    public bool Equals(ShiftCategoryFairnessCompareResult other)
	    {
	        if (ReferenceEquals(null, other)) return false;
	        if (ReferenceEquals(this, other)) return true;
	        return OriginalMembers.Count == other.OriginalMembers.Count &&
                   OriginalMembers.SequenceEqual(other.OriginalMembers, new PersonEqualityComparer());
	    }

	    public override int GetHashCode()
	    {
	        unchecked
	        {
	            var result = (OriginalMembers != null ? OriginalMembers.GetHashCode() : 0);
	            return result;
	        }
	    }
	}

	public interface IShiftCategoryFairnessComparer
	{
		ShiftCategoryFairnessCompareResult Compare(IShiftCategoryFairnessHolder original, IShiftCategoryFairnessHolder compareTo, IEnumerable<IShiftCategory> shiftCategories);
	}

	public class ShiftCategoryFairnessComparer : IShiftCategoryFairnessComparer
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public ShiftCategoryFairnessCompareResult Compare(IShiftCategoryFairnessHolder original, IShiftCategoryFairnessHolder compareTo, IEnumerable<IShiftCategory> shiftCategories)
		{
			var totalOriginal = original.ShiftCategoryFairnessDictionary.Values.Sum();
			var totalCompare = compareTo.ShiftCategoryFairnessDictionary.Values.Sum();

			var result = shiftCategories.Select(shiftCategory => new ShiftCategoryFairnessCompareValue { ShiftCategory = shiftCategory }).OfType<IShiftCategoryFairnessCompareValue>().ToList();
			foreach (var shiftCategoryFairnessComparerResult in result)
			{
				if (original.ShiftCategoryFairnessDictionary.ContainsKey(shiftCategoryFairnessComparerResult.ShiftCategory))
				{
					shiftCategoryFairnessComparerResult.Original =
						(double)original.ShiftCategoryFairnessDictionary[shiftCategoryFairnessComparerResult.ShiftCategory]/totalOriginal;
				}
				if (compareTo.ShiftCategoryFairnessDictionary.ContainsKey(shiftCategoryFairnessComparerResult.ShiftCategory))
				{
					shiftCategoryFairnessComparerResult.ComparedTo =
						(double)compareTo.ShiftCategoryFairnessDictionary[shiftCategoryFairnessComparerResult.ShiftCategory] / totalCompare;
				}
			}

		    var mean = calculateMeanValue(result);
		    var stdDev = calculateStandardDeviation(result, mean);

		    var shiftCategoryFairnessCompareResult =
		        new ShiftCategoryFairnessCompareResult
		            {StandardDeviation = stdDev, ShiftCategoryFairnessCompareValues = result};
                
		    return shiftCategoryFairnessCompareResult;
		}

	    private static double calculateStandardDeviation(List<IShiftCategoryFairnessCompareValue> result, double mean)
	    {
	        var sumOfSquareOfDifference = 0.0;

	        foreach (var shiftCategoryFairnessCompareValue in result)
	        {
	            var difference = (shiftCategoryFairnessCompareValue.ComparedTo - shiftCategoryFairnessCompareValue.Original);
	            var differenceWithMean = (difference - mean);
	            sumOfSquareOfDifference += (differenceWithMean)*(differenceWithMean);
	        }

	        var stdDev = System.Math.Sqrt(sumOfSquareOfDifference/result.Count);
	        return stdDev;
	    }

	    private static double calculateMeanValue(List<IShiftCategoryFairnessCompareValue> result)
	    {
	        var sum = 0.0;
	        
            foreach (var shiftCategoryFairnessCompareValue in result)
            {
                sum = sum + (shiftCategoryFairnessCompareValue.ComparedTo - shiftCategoryFairnessCompareValue.Original);
            }

	        return sum/result.Count;
	    }

        
	}
}