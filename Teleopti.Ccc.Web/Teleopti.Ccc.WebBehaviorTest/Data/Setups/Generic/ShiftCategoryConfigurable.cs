using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic
{
	public class ShiftCategoryConfigurable : IDataSetup
	{
		public string Name { get; set; }
		public string Color { get; set; }

		public void Apply(IUnitOfWork uow)
		{
			var shiftCategory = new ShiftCategory(Name);
			if (Color != null)
				shiftCategory.DisplayColor = System.Drawing.Color.FromName(Color);
			var shiftCategoryRepository = new ShiftCategoryRepository(uow);
			shiftCategoryRepository.Add(shiftCategory);
		}

	}

	public class ShiftCategoryDataSetup : IDataSetup
	{
		private readonly IEnumerable<ShiftCategoryConfigurable> _set;

		public ShiftCategoryDataSetup(IEnumerable<ShiftCategoryConfigurable> set)
		{
			_set = set;
		}

		public void Apply(IUnitOfWork uow)
		{
			var shiftCategoryRepository = new ShiftCategoryRepository(uow);
			foreach (var shiftCategoryConfigurable in _set)
			{
				var shiftCategory = new ShiftCategory(shiftCategoryConfigurable.Name);
				shiftCategoryRepository.Add(shiftCategory);
			}
		}
	}
}