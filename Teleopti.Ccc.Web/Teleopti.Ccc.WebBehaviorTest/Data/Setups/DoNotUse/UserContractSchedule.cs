using System.Globalization;
using System.Linq;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.DoNotUse
{
	public class UserContractSchedule : IUserSetup
	{
		private readonly IContractSchedule _contractSchedule;

		public UserContractSchedule(IContractSchedule contractSchedule)
		{
			_contractSchedule = contractSchedule;
		}

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			user.PersonPeriodCollection.Single().PersonContract.ContractSchedule = _contractSchedule;
		}
	}
}