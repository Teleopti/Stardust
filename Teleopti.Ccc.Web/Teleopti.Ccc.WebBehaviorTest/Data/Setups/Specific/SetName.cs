using System.Globalization;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Specific
{
	public class SetName : IUserSetup
	{
		private Name _name;

		public SetName(string userName)
		{
			if (userName.Contains(" "))
			{
				var splitted = userName.Split(' ');
				_name = new Name(splitted[0], splitted[1]);
			}
			else
			{
				_name = new Name("", userName);
			}
		}

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			user.Name = _name;
		}
	}
}