using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.DoNotUse
{
	public abstract class BaseShiftCategory : IUserDataSetup
	{
		public ShiftCategory ShiftCategory;

		protected abstract string CategoryName();

		public void Apply(ICurrentUnitOfWork unitOfWork, IPerson person, CultureInfo cultureInfo)
		{
			ShiftCategoryRepository.DONT_USE_CTOR(unitOfWork).Add(ShiftCategory);
		}
	}
}
