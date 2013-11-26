using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization
{
    public interface IShiftCategoryPoints
    {
        void AssignShiftCategoryPoints(IList<IShiftCategory> shiftCategories);
        int GetPointOfShiftCategory(IShiftCategory shiftCategory);
    }

    public class ShiftCategoryPoints : IShiftCategoryPoints
    {
        private IDictionary<IShiftCategory ,int > _shiftCategorypoints;

        public ShiftCategoryPoints()
        {
            _shiftCategorypoints = new Dictionary<IShiftCategory, int>();
        }

        public void AssignShiftCategoryPoints(IList<IShiftCategory> shiftCategories)
        {
            var i = 1;
            foreach (var sc in shiftCategories.OrderBy(s => s.Description.Name))
            {
                _shiftCategorypoints.Add(sc, i);
                i++;
            }
        }

        public int GetPointOfShiftCategory(IShiftCategory shiftCategory)
        {
            return _shiftCategorypoints[shiftCategory];
        }
    }
}
