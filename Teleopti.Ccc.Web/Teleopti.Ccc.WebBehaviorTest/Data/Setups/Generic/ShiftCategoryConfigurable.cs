using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic
{
	public class ShiftCategoryConfigurable : IDataSetup
	{
		public string Name { get; set; }

		public void Apply(IUnitOfWork uow)
		{
			var shiftCategory = new ShiftCategory(Name);
			var shiftCategoryRepository = new ShiftCategoryRepository(uow);
			shiftCategoryRepository.Add(shiftCategory);
		}
	}
}