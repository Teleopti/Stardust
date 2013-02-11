using System.Globalization;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Specific
{
	public class SetName : IUserSetup
	{
		private readonly Name _name;

		public SetName(string userName)
		{
			_name = new CreateName().FromString(userName);
		}

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			user.Name = _name;
		}
	}
}