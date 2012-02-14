using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.PeopleAdmin
{
    public class ShiftCategoryLimitationCombiner
    {
        IDictionary<IShiftCategory, ShiftCategoryLimitationCombination> _shiftCategoryLimitationDic = new Dictionary<IShiftCategory, ShiftCategoryLimitationCombination>();

        public IDictionary<IShiftCategory, ShiftCategoryLimitationCombination> Combine(IList<IShiftCategoryLimitation> shiftCategoryLimitations)
        {
            foreach (IShiftCategoryLimitation limitation in shiftCategoryLimitations)
            {
                if (!_shiftCategoryLimitationDic.ContainsKey(limitation.ShiftCategory))
                {
                    ShiftCategoryLimitationCombination combination = new ShiftCategoryLimitationCombination(limitation.ShiftCategory);
                    _shiftCategoryLimitationDic.Add(combination.ShiftCategory, combination);
                }
                else
                {
                    _shiftCategoryLimitationDic[limitation.ShiftCategory].CombineLimitations(limitation);
                }
            }
            return _shiftCategoryLimitationDic;
        }
    }
}
