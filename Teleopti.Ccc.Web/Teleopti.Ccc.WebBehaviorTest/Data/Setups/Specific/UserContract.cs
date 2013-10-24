using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Specific
{
	public class UserContract : IUserSetup
	{
		private readonly IContract _contract;

		public UserContract(IContract contract)
		{
			_contract = contract;
		}

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			user.PersonPeriodCollection.Single().PersonContract.Contract = _contract;
		}
	}
}