using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority
{
	public interface IShiftCategoryPointExtractor
	{
		IDictionary<IShiftCategory, int> ExtractShiftCategoryPoints();
	}

	public class ShiftCategoryPointExtractor : IShiftCategoryPointExtractor
	{
		private readonly IList<IShiftCategory> _shiftCategories;

		public ShiftCategoryPointExtractor(IList<IShiftCategory> shiftCategories)
		{
			_shiftCategories = shiftCategories;
		}

		public IDictionary<IShiftCategory, int> ExtractShiftCategoryPoints()
		{
			var result = new Dictionary<IShiftCategory, int>();
			var shiftCategoryPoint = 0;
			foreach (var shiftCategory in _shiftCategories.OrderByDescending(s => s.Description.Name))
			{
				result.Add(shiftCategory, shiftCategoryPoint);
				shiftCategoryPoint++;
			}

			return result;
		}
	}
}
