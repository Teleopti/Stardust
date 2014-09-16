using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Core
{
	public interface IPersonNameProvider
	{
		string BuildNameFromSetting(IPerson person);
		string BuildNameFromSetting(string firstName, string lastName);
	}

	public class PersonNameProvider : IPersonNameProvider
	{
		private readonly ISettingsPersisterAndProvider<NameFormatSettings> _nameFormatSettings;

		public PersonNameProvider(ISettingsPersisterAndProvider<NameFormatSettings> nameFormatSettings)
		{
			_nameFormatSettings = nameFormatSettings;
		}

		public string BuildNameFromSetting(IPerson person)
		{
			return BuildNameFromSetting(person.Name.FirstName, person.Name.LastName);
		}

		public string BuildNameFromSetting(string firstName, string lastName)
		{
			string firstLast = firstName + " " + lastName;
			string lastFirst = lastName + " " + firstName;
			string name = firstLast;

			var persistedNameFormatSettings = _nameFormatSettings.Get();
			if (persistedNameFormatSettings != null)
			{
				if (persistedNameFormatSettings.NameFormatId == 0)
				{
					name = firstLast;
				}
				else if (persistedNameFormatSettings.NameFormatId == 1)
				{
					name = lastFirst;
				}
			}

			return name;
		}
	}
}