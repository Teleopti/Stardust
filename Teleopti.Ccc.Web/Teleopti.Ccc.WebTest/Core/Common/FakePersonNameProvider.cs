using Teleopti.Ccc.Web.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Common
{
	public class FakePersonNameProvider : IPersonNameProvider
	{
		public string BuildNameFromSetting(Name name)
		{
			return name.LastName + " " + name.FirstName;
		}

		public string BuildNameFromSetting(string firstName, string lastName)
		{
			return lastName + " " + firstName;
		}
	}
}
