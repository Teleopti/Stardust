using System.Collections.Generic;

namespace Teleopti.Ccc.WinCode.Scheduling.ShiftCategoryDistribution
{
    public static class ShiftDistributionCalculator
    {
        public static IList<ShiftDistribution> Extract(IList<ShiftCategoryStructure> mappedShiftCategoriesList)
        {
            var shiftDistributionList = new List<ShiftDistribution>();

            foreach (var shiftCategoryStructure in mappedShiftCategoriesList)
            {
                if (!assignShiftDistrubuionIfExists(shiftDistributionList,shiftCategoryStructure))
                {
                    shiftDistributionList.Add(new ShiftDistribution(shiftCategoryStructure.DateOnlyValue,
                                                                    shiftCategoryStructure.ShiftCategory, 1));
                }
                
            }

            return shiftDistributionList;
        }

        private static bool assignShiftDistrubuionIfExists(IEnumerable<ShiftDistribution> shiftDistributionList, 
                                                           ShiftCategoryStructure shiftCategoryStructure)

        {
            foreach (var shiftDistribution in shiftDistributionList)
            {
                if (shiftDistribution.DateOnly == shiftCategoryStructure.DateOnlyValue && shiftCategoryStructure.ShiftCategory != null 
                    && shiftCategoryStructure.ShiftCategory.Equals(shiftDistribution.ShiftCategory))
                {
                    shiftDistribution.Count++;
                    return true;
                }
                    
            }
            return false;
        }
    }
}