using System.Collections.Generic;

namespace Teleopti.Ccc.WinCode.Scheduling.ShiftCategoryDistribution
{
    public static class ShiftCategoryPerAgentCalculator
    {
        public static IList<ShiftCategoryPerAgent > Extract(IList<ShiftCategoryStructure> mappedShiftCategoriesList)
        {
            var shiftCategoryPerAgentList = new List<ShiftCategoryPerAgent>();

            foreach (var shiftCategoryStructure in mappedShiftCategoriesList)
            {
                if (assignShiftDistrubuionIfExists(shiftCategoryPerAgentList, shiftCategoryStructure))
                {

                }
                else
                {
                    shiftCategoryPerAgentList.Add(new ShiftCategoryPerAgent(shiftCategoryStructure.PersonValue ,
                                                                            shiftCategoryStructure.ShiftCategory, 1));
                }

            }

            return shiftCategoryPerAgentList;
        }

        private static bool assignShiftDistrubuionIfExists(IEnumerable<ShiftCategoryPerAgent> shiftCategoryPerAgentList,
                                                           ShiftCategoryStructure shiftCategoryStructure)
        {
            foreach (var shiftDistribution in shiftCategoryPerAgentList)
            {
                if (shiftDistribution.Person == shiftCategoryStructure.PersonValue
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