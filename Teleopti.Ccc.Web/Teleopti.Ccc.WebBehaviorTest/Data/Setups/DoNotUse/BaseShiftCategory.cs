using System.Globalization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.DoNotUse
{
	public abstract class BaseShiftCategory : IUserDataSetup
	{
		public ShiftCategory ShiftCategory;

		protected abstract string CategoryName();

		public void Apply(ICurrentUnitOfWork currentUnitOfWork, IPerson user, CultureInfo cultureInfo)
		{
			new ShiftCategoryRepository(currentUnitOfWork).Add(ShiftCategory);
		}
	}
}
