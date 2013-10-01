using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeLoggedOnUser : ILoggedOnUser
	{
		private readonly Person _person;

		public FakeLoggedOnUser()
		{
			_person = new Person();
		}

		public IPerson CurrentUser()
		{
			return _person;
		}
	}
}