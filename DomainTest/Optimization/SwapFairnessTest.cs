using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.DomainTest.Optimization
{
	[TestFixture]
	public class SwapFairnessTest
	{
		private SwapFariness _target;
		private IList<ShiftCategoryFairnessCompareResult> _groupList;
		
		[SetUp]
		public void Setup()
		{
			_groupList = new List<ShiftCategoryFairnessCompareResult>();

			//for (var i = 1; i > 3; i++)
			//{
			//    var grp = new ShiftCategoryFairnessCompareResult
			//                {
			//                    ShiftCategoryFairnessCompareValues = new List<ShiftCategoryFairnessCompareValue>()
			//                }
			//}

			// Creating Group1
			var group1 = new ShiftCategoryFairnessCompareResult
			             	{ShiftCategoryFairnessCompareValues = new List<ShiftCategoryFairnessCompareValue>()};

			var shiftCat1 = new ShiftCategoryFairnessCompareValue();
			shiftCat1.Original = 0.90;
			shiftCat1.ComparedTo = 0.1;
			shiftCat1.ShiftCategory = new ShiftCategory("Day");

			var shiftCat2 = new ShiftCategoryFairnessCompareValue();
			shiftCat2.Original = 0.10;
			shiftCat2.ComparedTo = 0.9;
			shiftCat2.ShiftCategory = new ShiftCategory("Noon");

			var shiftCat3 = new ShiftCategoryFairnessCompareValue();
			shiftCat3.Original = 0.0;
			shiftCat3.ComparedTo = 0.0;
			shiftCat3.ShiftCategory = new ShiftCategory("Night");

			group1.ShiftCategoryFairnessCompareValues.Add(shiftCat1);
			group1.ShiftCategoryFairnessCompareValues.Add(shiftCat2);
			group1.ShiftCategoryFairnessCompareValues.Add(shiftCat3);
			group1.StandardDeviation = 0.01;


			// Creating Group2
			var group2 = new ShiftCategoryFairnessCompareResult { ShiftCategoryFairnessCompareValues = new List<ShiftCategoryFairnessCompareValue>() };

			var shiftCat1Grp2 = new ShiftCategoryFairnessCompareValue();
			shiftCat1Grp2.Original = 0.0;
			shiftCat1Grp2.ComparedTo = 0.0;
			shiftCat1Grp2.ShiftCategory = new ShiftCategory("Day");

			var shiftCat2Grp2 = new ShiftCategoryFairnessCompareValue();
			shiftCat2Grp2.Original = 0.5;
			shiftCat2Grp2.ComparedTo = 0.5;
			shiftCat2Grp2.ShiftCategory = new ShiftCategory("Noon");

			var shiftCat3Grp2 = new ShiftCategoryFairnessCompareValue();
			shiftCat3Grp2.Original = 0.5;
			shiftCat3Grp2.ComparedTo = 0.5;
			shiftCat3Grp2.ShiftCategory = new ShiftCategory("Night");

			group2.ShiftCategoryFairnessCompareValues.Add(shiftCat1Grp2);
			group2.ShiftCategoryFairnessCompareValues.Add(shiftCat2Grp2);
			group2.ShiftCategoryFairnessCompareValues.Add(shiftCat3Grp2);
			group2.StandardDeviation = 0.05;

			// Creating Group3
			var group3 = new ShiftCategoryFairnessCompareResult { ShiftCategoryFairnessCompareValues = new List<ShiftCategoryFairnessCompareValue>() };

			var shiftCat1Grp3 = new ShiftCategoryFairnessCompareValue();
			shiftCat1Grp3.Original = 0.1;
			shiftCat1Grp3.ComparedTo = 0.9;
			shiftCat1Grp3.ShiftCategory = new ShiftCategory("Day");

			var shiftCat2Grp3 = new ShiftCategoryFairnessCompareValue();
			shiftCat2Grp3.Original = 0.10;
			shiftCat2Grp3.ComparedTo = 0.9;
			shiftCat2Grp3.ShiftCategory = new ShiftCategory("Noon");

			var shiftCat3Grp3 = new ShiftCategoryFairnessCompareValue();
			shiftCat3Grp3.Original = 0.0;
			shiftCat3Grp3.ComparedTo = 0.0;
			shiftCat3Grp3.ShiftCategory = new ShiftCategory("Night");

			group3.ShiftCategoryFairnessCompareValues.Add(shiftCat1Grp3);
			group3.ShiftCategoryFairnessCompareValues.Add(shiftCat2Grp3);
			group3.ShiftCategoryFairnessCompareValues.Add(shiftCat3Grp3);
			group3.StandardDeviation = 0.09;

			_groupList.Add(group1);
			_groupList.Add(group2);
			_groupList.Add(group3);

			_target = new SwapFariness();

		}

		[Test]
		public void ShouldReturnTwoGroupsForSwapping()
		{
			var result = _target.GetGroupsToSwap(_groupList);
			Assert.Equals(result.Count, Is.EqualTo(2));
			Assert.Equals(result[0].StandardDeviation, Is.EqualTo(0.01));
			Assert.Equals(result[1].StandardDeviation, Is.EqualTo(0.09));
		}

	}

	public class SwapFariness
	{
		public IList<ShiftCategoryFairnessCompareResult> GetGroupsToSwap(IList<ShiftCategoryFairnessCompareResult> groupList)
		{
			var group = from shiftCategoryFairnessCompareResult in groupList
			            orderby shiftCategoryFairnessCompareResult.StandardDeviation ascending 
			            select shiftCategoryFairnessCompareResult;
			
			var firstGroup = group.First();

			//STEP 2: Find shift category having highest value
			var highShiftCatList = from currentGroup in firstGroup.ShiftCategoryFairnessCompareValues
			                   orderby currentGroup.Original descending
			                   select currentGroup.ShiftCategory;
			
			var highShiftCat = highShiftCatList.First();

			// STEP 3: Find Shift category having lowest value
			var lowShiftCat = highShiftCatList.Last();
			
			double currentHighShiftCat=0.0;
			double currentLowShiftCat = 0.0;
			double maxHighShiftCatDiff = 0.0;
			double maxLowShiftCatDiff = 0.0;
			
			// STEP 4: Iterate other groups other than the one having best Std Dev value
			foreach (var grp in group.Skip(1))
			{
				var tempHighShiftCat = grp.ShiftCategoryFairnessCompareValues.Where(x=>x.ShiftCategory==highShiftCat);
				currentHighShiftCat = tempHighShiftCat.First().Original;

				var tempLowShiftCat = grp.ShiftCategoryFairnessCompareValues.Where(x => x.ShiftCategory == lowShiftCat);
				currentLowShiftCat = tempLowShiftCat.First().Original;

				


			}


			return new List<ShiftCategoryFairnessCompareResult>();
		}
	}
}
