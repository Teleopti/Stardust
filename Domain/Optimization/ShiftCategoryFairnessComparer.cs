using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
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
	}

	public interface IShiftCategoryFairnessCompareResult
	{
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]      
		IList<ShiftCategoryFairnessCompareValue> ShiftCategoryFairnessCompareValues { get; set; }
		Guid Id { get; }
		double StandardDeviation { get; set; }
	}

	public class ShiftCategoryFairnessCompareResult : IShiftCategoryFairnessCompareResult
	{
		private readonly Guid _groupId;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
		public IList<ShiftCategoryFairnessCompareValue> ShiftCategoryFairnessCompareValues { get; set; }

		public ShiftCategoryFairnessCompareResult()
		{
			_groupId = Guid.NewGuid();
		}
		public Guid Id
		{
			get { return _groupId; }
		}

		public double StandardDeviation { get; set; }
	}

	public interface IShiftCategoryFairnessComparer
	{
		ShiftCategoryFairnessCompareResult Compare(IShiftCategoryFairness original, IShiftCategoryFairness compareTo, IList<IShiftCategory> shiftCategories);
	}

	public class ShiftCategoryFairnessComparer : IShiftCategoryFairnessComparer
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public ShiftCategoryFairnessCompareResult Compare(IShiftCategoryFairness original, IShiftCategoryFairness compareTo, IList<IShiftCategory> shiftCategories)
		{
			var totalOriginal = original.ShiftCategoryFairnessDictionary.Values.Sum();
			var totalCompare = compareTo.ShiftCategoryFairnessDictionary.Values.Sum();

			var result = shiftCategories.Select(shiftCategory => new ShiftCategoryFairnessCompareValue { ShiftCategory = shiftCategory }).ToList();
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

	    private static double calculateStandardDeviation(List<ShiftCategoryFairnessCompareValue> result, double mean)
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

	    private static double calculateMeanValue(List<ShiftCategoryFairnessCompareValue> result)
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