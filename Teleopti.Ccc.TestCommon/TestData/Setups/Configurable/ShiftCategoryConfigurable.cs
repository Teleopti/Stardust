using System.Linq;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class ShiftCategoryConfigurable : IDataSetup
	{
		public string Name { get; set; }
		public string Color { get; set; }
		public string BusinessUnit { get; set; }

		public IShiftCategory ShiftCategory;

		public void Apply(IUnitOfWork uow)
		{
			ShiftCategory = new ShiftCategory(Name);
			if (Color != null)
				ShiftCategory.DisplayColor = System.Drawing.Color.FromName(Color);
			if (!string.IsNullOrEmpty(BusinessUnit))
				ShiftCategory.SetBusinessUnit(new BusinessUnitRepository(uow).LoadAll().Single(b => b.Name == BusinessUnit));

			var shiftCategoryRepository = new ShiftCategoryRepository(uow);
			shiftCategoryRepository.Add(ShiftCategory);
		}

	}
}