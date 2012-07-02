using System.Globalization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User
{
	public abstract class BaseShiftCategory : IUserDataSetup
	{
		public ShiftCategory ShiftCategory;

		protected abstract string CategoryName();

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			new ShiftCategoryRepository(uow).Add(ShiftCategory);
		}
	}
}
