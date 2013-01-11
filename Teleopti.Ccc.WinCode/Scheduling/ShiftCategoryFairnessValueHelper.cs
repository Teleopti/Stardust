using System.Collections.Generic;
using Teleopti.Ccc.Domain.Optimization.ShiftCategoryFairness;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public class ShiftCategoryFairnessValueHelper
	{
		private readonly IList<IShiftCategoryFairnessCompareValue> _agentValues;
		private readonly IList<IShiftCategoryFairnessCompareValue> _teamValues;

		public ShiftCategoryFairnessValueHelper(IList<IShiftCategoryFairnessCompareValue> agentValues, IList<IShiftCategoryFairnessCompareValue> teamValues)
		{
			_agentValues = agentValues;
			_teamValues = teamValues;
		}

		public IList<IShiftCategory> ShiftCategories()
		{
			var shiftCategories = new List<IShiftCategory>();

			foreach (var value in _agentValues)
			{
				if(!shiftCategories.Contains(value.ShiftCategory))
					shiftCategories.Add(value.ShiftCategory);
			}

			foreach (var value in _teamValues)
			{
				if(!shiftCategories.Contains(value.ShiftCategory))
					shiftCategories.Add(value.ShiftCategory);
			}

			shiftCategories.Sort(new ShiftCategorySorter());

			return shiftCategories;
		}

		public IShiftCategoryFairnessCompareValue AgentValue(IEntity shiftCategory)
		{
			foreach (var shiftCategoryFairnessCompareValue in _agentValues)
			{
				if (shiftCategoryFairnessCompareValue.ShiftCategory.Equals(shiftCategory))
					return shiftCategoryFairnessCompareValue;
			}

			return new ShiftCategoryFairnessCompareValue();
		}

		public IShiftCategoryFairnessCompareValue TeamValue(IEntity shiftCategory)
		{
			foreach (var shiftCategoryFairnessCompareValue in _teamValues)
			{
				if (shiftCategoryFairnessCompareValue.ShiftCategory.Equals(shiftCategory))
					return shiftCategoryFairnessCompareValue;
			}

			return new ShiftCategoryFairnessCompareValue();
		}
	}
}
