using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class ShiftCategoryConfigurable : IDataSetup
	{
		public string Name { get; set; }
		public string ShortName { get; set; }
		public string Color { get; set; }
		public string BusinessUnit { get; set; }

		public IShiftCategory ShiftCategory;

		public void Apply(ICurrentUnitOfWork currentUnitOfWork)
		{
			ShiftCategory = new ShiftCategory(Name)
			{
				Description = new Description(Name, ShortName ?? string.Empty)
			};
			if (Color != null)
				ShiftCategory.DisplayColor = System.Drawing.Color.FromName(Color);
			if (!string.IsNullOrEmpty(BusinessUnit))
				ShiftCategory.SetBusinessUnit(BusinessUnitRepository.DONT_USE_CTOR(currentUnitOfWork, null, null).LoadAll().Single(b => b.Name == BusinessUnit));

			var shiftCategoryRepository = new ShiftCategoryRepository(currentUnitOfWork);
			shiftCategoryRepository.Add(ShiftCategory);
		}

	}
}