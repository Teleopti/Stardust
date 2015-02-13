using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority
{
    public interface IShiftCategoryPoints
	{
		IDictionary<IShiftCategory, int> ExtractShiftCategoryPoints(IList<IShiftCategory> shiftCategories);
	}

    public class ShiftCategoryPoints : IShiftCategoryPoints
	{

		public IDictionary<IShiftCategory, int> ExtractShiftCategoryPoints(IList<IShiftCategory> shiftCategories)
		{
			var result = new Dictionary<IShiftCategory, int>();
			var shiftCategoryPoint = 1;
			
			foreach (var shiftCategory in shiftCategories.OrderByDescending(x => x.Rank.HasValue ? x.Rank : int.MaxValue).ThenByDescending(x => x.Description.Name))
			{
				result.Add(shiftCategory, shiftCategoryPoint);
				shiftCategoryPoint++;
			}

			return result;
		}
	}
}
