using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Web.Core;

namespace Teleopti.Ccc.WebTest.Core.Common
{
	public class FakePersonNameProvider : IPersonNameProvider
	{
		public string BuildNameFromSetting(Name name)
		{
			return name.LastName + " " + name.FirstName;
		}

		public string BuildNameFromSetting(string firstName, string lastName, NameFormatSettings setting)
		{
			throw new System.NotImplementedException();
		}

		public string BuildNameFromSetting(string firstName, string lastName)
		{
			return lastName + " " + firstName;
		}
	}
}
