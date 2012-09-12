﻿using System.Collections;
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
		IList<ShiftCategoryFairnessCompareValue> ShiftCategoryFairnessCompareValues { get; set; }
		double StandarDeviation { get; set; }
	}

	public class ShiftCategoryFairnessCompareResult : IShiftCategoryFairnessCompareResult
	{
		public IList<ShiftCategoryFairnessCompareValue> ShiftCategoryFairnessCompareValues { get; set; }
		public double StandarDeviation { get; set; }
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
			//TODO Calculate standard deviation
			return new ShiftCategoryFairnessCompareResult { ShiftCategoryFairnessCompareValues = result }; 
		}

	}
}