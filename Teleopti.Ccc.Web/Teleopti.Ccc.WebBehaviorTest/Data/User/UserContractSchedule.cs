using System.Globalization;
using System.Linq;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User
{
	public class UserContractSchedule : IUserSetup
	{
		private readonly IContractSchedule _contractSchedule;

		public UserContractSchedule(IContractSchedule contractSchedule) {
			_contractSchedule = contractSchedule;
		}

		public void Apply(IPerson user, CultureInfo cultureInfo)
		{
			user.PersonPeriodCollection.Single().PersonContract.ContractSchedule = _contractSchedule;
		}
	}
}